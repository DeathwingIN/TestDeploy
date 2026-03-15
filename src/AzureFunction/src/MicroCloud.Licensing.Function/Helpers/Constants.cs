namespace MicroCloud.Licensing.Function.Helpers
{
    public static class Constants
    {
        // ── Azure / HTTP ────────────────────────────────────────────────────────
        public const string JsonContentType         = "application/json";
        public const string ApimSubscriptionKey     = "Ocp-Apim-Subscription-Key";

        // ── Business Central ────────────────────────────────────────────────────
        public const string BcApiScope              = "https://api.businesscentral.dynamics.com/.default";
        public const string BcApiVersion            = "v2.0";
        public const string BcApiNamespace          = "microcloud360/licensing/v1.0";

        // ── Azure Function names ────────────────────────────────────────────────
        public const string AppSourceWebhookFunctionName = "AppSourceWebhookProxy";
        public const string VerifyLicenseFunctionName    = "VerifyLicenseProxy";

        // ── AppSource / Entra ID ────────────────────────────────────────────────
        public const string AuthorizationHeader     = "Authorization";
        public const string BearerScheme            = "Bearer";

        // ── Subscription status ─────────────────────────────────────────────────
        public const string SubscribedStatus        = "Subscribed";
    }
}
