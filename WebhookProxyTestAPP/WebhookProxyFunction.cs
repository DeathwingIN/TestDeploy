using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;
using Azure.Identity;
using Azure.Core;

namespace WebhookProxy
{
    public class AppSourceWebhookProxy
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();
        
        // Environment Variables
        private readonly string _aadTenantId = Environment.GetEnvironmentVariable("AadTenantId") ?? "";
        private readonly string _isvClientId = Environment.GetEnvironmentVariable("IsvClientId") ?? "";
        private readonly string _bcWebhookEndpoint = Environment.GetEnvironmentVariable("BcWebhookEndpoint") ?? "";
        private readonly string _bcEnvironmentName = Environment.GetEnvironmentVariable("BcEnvironmentName") ?? "Production";
        private readonly string _bcCompanyId = Environment.GetEnvironmentVariable("BcCompanyId") ?? "";
        private readonly string _bcCompanyName = Environment.GetEnvironmentVariable("BcCompanyName") ?? "";
        
        // OpenID Configuration for Microsoft Entra ID
        private static ConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;

        public AppSourceWebhookProxy(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AppSourceWebhookProxy>();
            
            if (_configurationManager == null && !string.IsNullOrEmpty(_aadTenantId))
            {
                string stsDiscoveryEndpoint = $"https://login.microsoftonline.com/{_aadTenantId}/v2.0/.well-known/openid-configuration";
                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            }
        }

        [Function("AppSourceWebhookProxy")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // 1. Validate the JWT Bearer Token from Microsoft AppSource
            if (!req.Headers.TryGetValues("Authorization", out var authHeaders) || !authHeaders.Any())
            {
                _logger.LogWarning("Missing Authorization header.");
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var authHeader = authHeaders.First();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization scheme. Expected Bearer.");
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            bool isValid = await ValidateAppSourceTokenAsync(token);
            if (!isValid)
            {
                _logger.LogWarning("JWT Token validation failed.");
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            // 2. Read the actual webhook payload
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Received valid AppSource webhook: {requestBody}");

            // 3. Forward the payload to Business Central
            bool forwarded = await ForwardToBusinessCentralAsync(requestBody);
            
            if (forwarded)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Webhook processed and forwarded successfully.");
                return response;
            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Failed to forward webhook to Business Central.");
                return response;
            }
        }

        private async Task<bool> ValidateAppSourceTokenAsync(string token)
        {
            // Bypass validation for local testing if configured
            if (Environment.GetEnvironmentVariable("SkipTokenValidation") == "true")
            {
                _logger.LogInformation("Skipping token validation for local testing.");
                return true;
            }

            try
            {
                if (_configurationManager == null) 
                {
                    _logger.LogError("Configuration manager is not initialized (Missing AadTenantId).");
                    return false;
                }

                // Retrieve the OpenID Connect configuration from Entra ID
                var config = await _configurationManager.GetConfigurationAsync();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateIssuer = true,
                    ValidIssuer = $"https://login.microsoftonline.com/{_aadTenantId}/v2.0",
                    ValidateAudience = true,
                    ValidAudience = _isvClientId, // AppSource webhooks are minted for your ISV App's Client ID
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                
                // ValidateToken throws an exception if the token is invalid
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                return true;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during token validation: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ForwardToBusinessCentralAsync(string payload)
        {
            try
            {
                if (string.IsNullOrEmpty(_bcWebhookEndpoint))
                {
                    _logger.LogError("BC Webhook Endpoint URL is not configured.");
                    return false;
                }

                // 1. Get S2S Access Token for Business Central using Managed Identity (or Client Secret if configured in Azure)
                // Note: In Azure, ensure the Function App has a System Assigned Managed Identity 
                // and it has been granted access to Dynamics 365 Business Central API.
                // Alternatively, you can use ClientSecretCredential if you have configured those environment variables.
                
                string bcScope = "https://api.businesscentral.dynamics.com/.default";
                TokenCredential credential;
                
                string? clientId = Environment.GetEnvironmentVariable("AzureClientId");
                string? clientSecret = Environment.GetEnvironmentVariable("AzureClientSecret");

                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogInformation("Using ClientSecretCredential for authentication.");
                    credential = new ClientSecretCredential(_aadTenantId, clientId, clientSecret);
                }
                else
                {
                    _logger.LogInformation("Using DefaultAzureCredential for authentication.");
                    credential = new DefaultAzureCredential(); 
                }

                var tokenRequestContext = new TokenRequestContext(new[] { bcScope });
                var accessToken = await credential.GetTokenAsync(tokenRequestContext, new CancellationToken());

                // 2. Construct the BC Endpoint URL if needed (can be hardcoded or dynamically built)
                // Assuming _bcWebhookEndpoint is the full URL to the custom API:
                // e.g., https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/partner/licensing/v1.0/companies({companyId})/marketplaceWebhook

                // Construct the true BC endpoint by replacing variables if they exist
                string endpointUrl = _bcWebhookEndpoint
                    .Replace("{{TenantId}}", _aadTenantId)
                    .Replace("{{EnvironmentName}}", _bcEnvironmentName);

                if (!string.IsNullOrEmpty(_bcCompanyId))
                {
                    endpointUrl = endpointUrl.Replace("{{CompanyId}}", _bcCompanyId);
                }
                else
                {
                    endpointUrl = endpointUrl.Replace("/companies({{CompanyId}})", "");
                }

                if (!string.IsNullOrEmpty(_bcCompanyName))
                {
                    string separator = endpointUrl.Contains("?") ? "&" : "?";
                    endpointUrl += $"{separator}company={Uri.EscapeDataString(_bcCompanyName)}";
                }

                var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                
                // Set the specific company if defined (sometimes handled in the URL path, sometimes in headers)
                if (!string.IsNullOrEmpty(_bcCompanyId))
                {
                    // Business Central OData requires If-Match for PATCH, but this is a POST to bound action/entity
                    // Just ensuring standard headers
                    request.Headers.Add("OData-MaxVersion", "4.0");
                    request.Headers.Add("OData-Version", "4.0");
                }

                request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation($"Forwarding to BC: {endpointUrl}");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully forwarded webhook to Business Central.");
                    return true;
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to forward webhook to BC. Status: {response.StatusCode}. Error: {errorResponse}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error communicating with Business Central: {ex.Message}");
                return false;
            }
        }

        [Function("VerifyLicenseProxy")]
        public async Task<HttpResponseData> VerifyLicense([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a license verification request.");

            try
            {
                // Parse the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(requestBody);
                
                string? requestTenantId = data.TryGetProperty("tenantId", out var tenantIdProp) ? tenantIdProp.GetString() : null;

                if (string.IsNullOrEmpty(requestTenantId))
                {
                    _logger.LogWarning("Missing 'tenantId' in the request payload.");
                    var badReqResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badReqResponse.WriteStringAsync("Missing 'tenantId' in the request payload.");
                    return badReqResponse;
                }

                _logger.LogInformation($"Checking license for tenant: {requestTenantId}");

                // 1. Get S2S Access Token for Business Central
                string bcScope = "https://api.businesscentral.dynamics.com/.default";
                TokenCredential credential;
                
                string? clientId = Environment.GetEnvironmentVariable("AzureClientId");
                string? clientSecret = Environment.GetEnvironmentVariable("AzureClientSecret");

                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    credential = new ClientSecretCredential(_aadTenantId, clientId, clientSecret);
                }
                else
                {
                    credential = new DefaultAzureCredential(); 
                }

                var tokenRequestContext = new TokenRequestContext(new[] { bcScope });
                var accessToken = await credential.GetTokenAsync(tokenRequestContext, new CancellationToken());

                // 2. Construct the BC Endpoint URL to query the ISV Subscription check
                // This makes an OData GET request to the License Check API Page
                string tenantEnvironment = !string.IsNullOrEmpty(_bcEnvironmentName) ? _bcEnvironmentName : "Production";
                string endpointUrl = $"https://api.businesscentral.dynamics.com/v2.0/{_aadTenantId}/{tenantEnvironment}/api/microcloud360/licensing/v1.0/licenseChecks?company={Uri.EscapeDataString(_bcCompanyName)}&$filter=aadTenantId eq '{requestTenantId}'";

                var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                
                _logger.LogInformation($"Querying BC for license: {endpointUrl}");
                
                var response = await _httpClient.SendAsync(request);
                
                bool isLicensed = false;

                if (response.IsSuccessStatusCode)
                {
                    string bcResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Successfully queried BC. Response: {bcResponse}");

                    // Check if the response contains an active "Subscribed" record
                    if (bcResponse.Contains("\"status\":\"Subscribed\""))
                    {
                        isLicensed = true;
                    }
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to query BC. Status: {response.StatusCode}. Error: {errorResponse}");
                }

                // Return the result to the caller
                var finalResponse = req.CreateResponse(HttpStatusCode.OK);
                finalResponse.Headers.Add("Content-Type", "application/json");
                
                var responsePayload = new { isLicensed = isLicensed };
                await finalResponse.WriteStringAsync(JsonSerializer.Serialize(responsePayload));
                
                return finalResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing license request: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error processing license request.");
                return errorResponse;
            }
        }
    }
}
