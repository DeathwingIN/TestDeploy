using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MicroCloud.Licensing.Function.Services;

namespace MicroCloud.Licensing.Function.Functions
{
    public class AppSourceWebhookFunction
    {
        private readonly ILogger _logger;
        private readonly ITokenValidatorService _tokenValidator;
        private readonly IBusinessCentralService _bcService;

        // Dependency Injection automatically passes the services in here!
        public AppSourceWebhookFunction(
            ILoggerFactory loggerFactory, 
            ITokenValidatorService tokenValidator,
            IBusinessCentralService bcService)
        {
            _logger = loggerFactory.CreateLogger<AppSourceWebhookFunction>();
            _tokenValidator = tokenValidator;
            _bcService = bcService;
        }

        [Function("AppSourceWebhookProxy")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Webhook triggered.");

            if (!req.Headers.TryGetValues("Authorization", out var authHeaders) || !authHeaders.Any())
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            var token = authHeaders.First().Replace("Bearer ", "").Trim();
            
            if (!await _tokenValidator.ValidateAppSourceTokenAsync(token))
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            bool success = await _bcService.ForwardWebhookAsync(requestBody);
            
            var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(success ? "Webhook processed." : "Failed to process.");
            return response;
        }
    }
}