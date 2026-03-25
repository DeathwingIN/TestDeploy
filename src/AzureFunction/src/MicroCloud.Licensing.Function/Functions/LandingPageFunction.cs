using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MicroCloud.Licensing.Function.Functions
{
    public class LandingPageFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public LandingPageFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LandingPageFunction>();
        }

        [Function("LandingPage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Landing page loaded by customer.");

            // 1. Get the token from the URL
            string? token = req.Query["token"];

            if (string.IsNullOrEmpty(token))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Error: Missing Microsoft token in URL.");
                return errorResponse;
            }

            try
            {
                // 2. Send the token to Microsoft to find out who the customer is
                var customerData = await ResolveTokenWithMicrosoftAsync(token);

                if (customerData == null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Error: Could not verify token with Microsoft.");
                    return errorResponse;
                }

                // 3. Extract exact details
                string customerTenantId = customerData.Value.TenantId;
                string subscriptionId = customerData.Value.SubscriptionId;
                string customerEmail = customerData.Value.Email;

                // 4. Push data to Business Central
                string bcUpdateResult = await UpdateBusinessCentralAsync(customerTenantId, subscriptionId, customerEmail);

                if (bcUpdateResult != "SUCCESS")
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"DEBUG ERROR: {bcUpdateResult}");
                    return errorResponse;
                }

                // 5. Show the Welcome Page if everything succeeded!
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/html; charset=utf-8");

                string html = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Welcome to MicroCloud 360</title>
                        <style>
                            body {{ font-family: Arial; text-align: center; padding: 50px; background-color: #f4f4f9; }}
                            .box {{ border: 1px solid #ccc; padding: 30px; border-radius: 10px; background: white; display: inline-block; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }}
                            h1 {{ color: #0078D4; }}
                            .info-box {{ text-align: left; background: #eef; padding: 15px; border-radius: 5px; margin-top: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='box'>
                            <h1>Thank you for subscribing!</h1>
                            <p>Your Google Address Management app is almost ready.</p>
                            
                            <div class='info-box'>
                                <strong>Success! We found your details securely from Microsoft:</strong><br/><br/>
                                <b>Tenant ID:</b> {customerTenantId}<br/>
                                <b>Subscription ID:</b> {subscriptionId}<br/>
                                <b>Admin Email:</b> {customerEmail}
                            </div>
                            
                            <p style='margin-top:20px; color: green;'><strong>You can now close this window and return to Business Central.</strong></p>
                        </div>
                    </body>
                    </html>";

                await response.WriteStringAsync(html);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Landing Page Error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"CRASH DETAILS: {ex.ToString()}");
                return errorResponse;
            }
        }

        private async Task<(string TenantId, string SubscriptionId, string Email)?> ResolveTokenWithMicrosoftAsync(string marketplaceToken)
        {
            // --- TEST MODE BYPASS ---
            if (marketplaceToken == "test12345")
            {
                return ("23a0c336-e5f0-4bbc-90b2-2899179cbbb5", "sub-imesh-999", "imesh@test.com");
            }
            // ------------------------

            string marketplaceScope = "20e940b3-4c77-4b0c-9a53-9e16a1b010a7/.default";

            string? tenantId = Environment.GetEnvironmentVariable("AadTenantId");
            string? clientId = Environment.GetEnvironmentVariable("AzureClientId");
            string? clientSecret = Environment.GetEnvironmentVariable("AzureClientSecret");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Missing Azure credentials in environment variables.");
                return null;
            }

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var tokenRequestContext = new TokenRequestContext(new[] { marketplaceScope });
            var accessToken = await credential.GetTokenAsync(tokenRequestContext, new CancellationToken());

            var request = new HttpRequestMessage(HttpMethod.Post, "https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version=2018-08-31");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            request.Headers.Add("x-ms-marketplace-token", marketplaceToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;
                    string subId = root.GetProperty("id").GetString() ?? "";
                    var subscription = root.GetProperty("subscription");
                    var purchaser = subscription.GetProperty("purchaser");

                    string customerTenantId = purchaser.GetProperty("tenantId").GetString() ?? "";
                    string customerEmail = purchaser.GetProperty("emailId").GetString() ?? "";

                    return (customerTenantId, subId, customerEmail);
                }
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Microsoft Resolve API failed. Status: {response.StatusCode}. Error: {error}");
                return null;
            }
        }

        private async Task<string> UpdateBusinessCentralAsync(string customerTenantId, string subscriptionId, string customerEmail)
        {
            try
            {
                string? myTenantId = Environment.GetEnvironmentVariable("AadTenantId");
                string? clientId = Environment.GetEnvironmentVariable("AzureClientId");
                string? clientSecret = Environment.GetEnvironmentVariable("AzureClientSecret");
                string? bcCompanyId = Environment.GetEnvironmentVariable("BcCompanyId");

                // Safety check
                if (string.IsNullOrEmpty(bcCompanyId))
                    return "ERROR: BcCompanyId is missing in your settings!";

                var credential = new ClientSecretCredential(myTenantId, clientId, clientSecret);
                var tokenRequestContext = new TokenRequestContext(new[] { "https://api.businesscentral.dynamics.com/.default" });
                var accessToken = await credential.GetTokenAsync(tokenRequestContext, new CancellationToken());

                ///TODO:Update CORRECT 
                string environmentName = "DemoMV"; 
                string companyName = Uri.EscapeDataString("CRONUS International Ltd.");

                // string bcUrl = $"https://api.businesscentral.dynamics.com/v2.0/{myTenantId}/{environmentName}/api/microcloud/licensing/v1.0/companies({bcCompanyId})/isvSubscriptions('{subscriptionId}')";
                string bcUrl = $"https://api.businesscentral.dynamics.com/v2.0/{myTenantId}/{environmentName}/api/microcloud/licensing/v1.0/isvSubscriptions('{subscriptionId}')?company={companyName}";

                var request = new HttpRequestMessage(HttpMethod.Patch, bcUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                request.Headers.Add("If-Match", "*");

                var payload = new { aadTenantId = customerTenantId, customerName = customerEmail };
                request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return $"BC REJECTED IT: Status {response.StatusCode} - {error} --- URL Tried: {bcUrl}";
                }

                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return $"C SHARP CRASH: {ex.Message}";
            }
        }
    }
}