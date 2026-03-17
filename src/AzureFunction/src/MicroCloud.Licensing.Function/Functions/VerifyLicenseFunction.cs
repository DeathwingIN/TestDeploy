using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MicroCloud.Licensing.Function.Services;

namespace MicroCloud.Licensing.Function.Functions
{
    public class VerifyLicenseFunction
    {
        private readonly ILogger _logger;
        private readonly IBusinessCentralService _bcService;

        public VerifyLicenseFunction(ILoggerFactory loggerFactory, IBusinessCentralService bcService)
        {
            _logger = loggerFactory.CreateLogger<VerifyLicenseFunction>();
            _bcService = bcService;
        }

        [Function("VerifyLicenseProxy")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("License verification triggered.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(requestBody);
            
            string? requestTenantId = data.TryGetProperty("tenantId", out var tenantIdProp) ? tenantIdProp.GetString() : null;

            if (string.IsNullOrEmpty(requestTenantId))
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Missing 'tenantId'.");
                return badReq;
            }

            bool isLicensed = await _bcService.CheckLicenseAsync(requestTenantId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(new { isLicensed }));
            
            return response;
        }
    }
}