using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MicroCloud.Licensing.Function.Config;

namespace MicroCloud.Licensing.Function.Services
{
    public class TokenValidatorService : ITokenValidatorService
    {
        private readonly ILogger<TokenValidatorService> _logger;
        private readonly ConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;

        public TokenValidatorService(ILogger<TokenValidatorService> logger)
        {
            _logger = logger;
            
            if (!string.IsNullOrEmpty(AppConfig.AadTenantId))
            {
                string stsDiscoveryEndpoint = $"https://login.microsoftonline.com/{AppConfig.AadTenantId}/v2.0/.well-known/openid-configuration";
                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            }
        }

        public async Task<bool> ValidateAppSourceTokenAsync(string token)
        {
            if (AppConfig.SkipTokenValidation == "true")
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

                var config = await _configurationManager.GetConfigurationAsync();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateIssuer = true,
                    ValidIssuer = $"https://login.microsoftonline.com/{AppConfig.AadTenantId}/v2.0",
                    ValidateAudience = true,
                    ValidAudience = AppConfig.IsvClientId, 
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation failed: {ex.Message}");
                return false;
            }
        }
    }
}