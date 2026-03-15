using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicroCloud.Licensing.Function.Config;

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
            // See: https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            LoggerFilterRule? appInsightsRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (appInsightsRule is not null)
            {
                options.Rules.Remove(appInsightsRule);
            }
        });

        // Register services locally
        services.AddSingleton<ICustomApplicationConfig, AppConfig>();
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
