using System;
using System.Diagnostics.CodeAnalysis;

namespace MicroCloud.Licensing.Function.Config
{
    [ExcludeFromCodeCoverage]
    public class AppConfig : ICustomApplicationConfig
    {
        // Entra ID / AppSource
        public static string AadTenantId       => Environment.GetEnvironmentVariable("AadTenantId") ?? string.Empty;
        public static string IsvClientId        => Environment.GetEnvironmentVariable("IsvClientId") ?? string.Empty;

        // Business Central endpoint
        public static string BcWebhookEndpoint  => Environment.GetEnvironmentVariable("BcWebhookEndpoint") ?? string.Empty;
        public static string BcEnvironmentName  => Environment.GetEnvironmentVariable("BcEnvironmentName") ?? "Production";
        public static string BcCompanyId        => Environment.GetEnvironmentVariable("BcCompanyId") ?? string.Empty;
        public static string BcCompanyName      => Environment.GetEnvironmentVariable("BcCompanyName") ?? string.Empty;

        // Azure credential (falls back to Managed Identity if not set)
        public static string AzureClientId      => Environment.GetEnvironmentVariable("AzureClientId") ?? string.Empty;
        public static string AzureClientSecret  => Environment.GetEnvironmentVariable("AzureClientSecret") ?? string.Empty;

        // Dev/testing
        public static string SkipTokenValidation => Environment.GetEnvironmentVariable("SkipTokenValidation") ?? "false";

        // General
        public static string EnvironmentName   => Environment.GetEnvironmentVariable("EnvironmentName") ?? string.Empty;
    }
}
