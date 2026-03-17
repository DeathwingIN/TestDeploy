using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using MicroCloud.Licensing.Function.Config;

namespace MicroCloud.Licensing.Function.Services
{
    public class BusinessCentralService : IBusinessCentralService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BusinessCentralService> _logger;

        public BusinessCentralService(HttpClient httpClient, ILogger<BusinessCentralService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            string bcScope = "https://api.businesscentral.dynamics.com/.default";
            TokenCredential credential;
            
            if (!string.IsNullOrEmpty(AppConfig.AzureClientId) && !string.IsNullOrEmpty(AppConfig.AzureClientSecret))
            {
                credential = new ClientSecretCredential(AppConfig.AadTenantId, AppConfig.AzureClientId, AppConfig.AzureClientSecret);
            }
            else
            {
                credential = new DefaultAzureCredential(); 
            }

            var tokenRequestContext = new TokenRequestContext(new[] { bcScope });
            var accessToken = await credential.GetTokenAsync(tokenRequestContext, new CancellationToken());
            return accessToken.Token;
        }

        public async Task<bool> ForwardWebhookAsync(string payload)
        {
            try
            {
                if (string.IsNullOrEmpty(AppConfig.BcWebhookEndpoint))
                {
                    _logger.LogError("BC Webhook Endpoint URL is not configured.");
                    return false;
                }

                string token = await GetAccessTokenAsync();

                string endpointUrl = AppConfig.BcWebhookEndpoint
                    .Replace("{{TenantId}}", AppConfig.AadTenantId)
                    .Replace("{{EnvironmentName}}", AppConfig.BcEnvironmentName);

                if (!string.IsNullOrEmpty(AppConfig.BcCompanyId))
                {
                    endpointUrl = endpointUrl.Replace("{{CompanyId}}", AppConfig.BcCompanyId);
                }
                else
                {
                    endpointUrl = endpointUrl.Replace("/companies({{CompanyId}})", "");
                }

                if (!string.IsNullOrEmpty(AppConfig.BcCompanyName))
                {
                    string separator = endpointUrl.Contains("?") ? "&" : "?";
                    endpointUrl += $"{separator}company={Uri.EscapeDataString(AppConfig.BcCompanyName)}";
                }

                var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                if (!string.IsNullOrEmpty(AppConfig.BcCompanyId))
                {
                    request.Headers.Add("OData-MaxVersion", "4.0");
                    request.Headers.Add("OData-Version", "4.0");
                }

                request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to forward webhook to BC. Status: {response.StatusCode}. Error: {errorResponse}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error communicating with Business Central: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckLicenseAsync(string requestTenantId)
        {
            try
            {
                string token = await GetAccessTokenAsync();
                
                string tenantEnvironment = !string.IsNullOrEmpty(AppConfig.BcEnvironmentName) ? AppConfig.BcEnvironmentName : "Production";
                string endpointUrl = $"https://api.businesscentral.dynamics.com/v2.0/{AppConfig.AadTenantId}/{tenantEnvironment}/api/microcloud360/licensing/v1.0/licenseChecks?company={Uri.EscapeDataString(AppConfig.BcCompanyName)}&$filter=aadTenantId eq '{requestTenantId}'";

                var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string bcResponse = await response.Content.ReadAsStringAsync();
                    return bcResponse.Contains("\"status\":\"Subscribed\"");
                }
                
                _logger.LogError($"Failed to query BC. Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing license request: {ex.Message}");
                return false;
            }
        }
    }
}