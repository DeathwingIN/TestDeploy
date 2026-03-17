using System;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicroCloud.Licensing.Function.Config;
using MicroCloud.Licensing.Function.Services; // Added this to use your new services

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddLogging();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<LoggerFilterOptions>(options =>
        {
            // Application Insights SDK adds a default filter for Warning+. Remove to capture Information+.
            LoggerFilterRule? appInsightsRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (appInsightsRule is not null)
            {
                options.Rules.Remove(appInsightsRule);
            }
        });

        // 1. Register configuration locally
        services.AddSingleton<ICustomApplicationConfig, AppConfig>();

        // 2. Register HTTP Client for Business Central Service
        // This is the best way to handle HttpClient in Azure Functions
        services.AddHttpClient<IBusinessCentralService, BusinessCentralService>();

        // 3. Register the Token Validator Service
        services.AddSingleton<ITokenValidatorService, TokenValidatorService>();
    })
    .Build();

try
{
    host.Run();
}
catch (Exception ex)
{
    // Log startup / DI errors that occur before functions can log
    Console.WriteLine($"CRITICAL ERROR during host startup: {ex}");
    Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
    }
    throw;
}