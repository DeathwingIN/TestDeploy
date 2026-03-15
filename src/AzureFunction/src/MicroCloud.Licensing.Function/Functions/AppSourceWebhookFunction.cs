using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MicroCloud.Licensing.Function.Functions
{
    public class AppSourceWebhookFunction
    {
        private readonly ILogger _logger;

        public AppSourceWebhookFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AppSourceWebhookFunction>();
        }

        [Function("AppSourceWebhook")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "webhook")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request. Received AppSource Webhook payload.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Payload details: {requestBody}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync("Welcome to MicroCloud Licensing Webhook. Payload received.");

            return response;
        }
    }
}
