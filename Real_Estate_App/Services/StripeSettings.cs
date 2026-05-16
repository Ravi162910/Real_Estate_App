namespace Real_Estate_App.Services
{
    // Bound from the "Stripe" configuration section. Real keys live in
    // appsettings.Development.json / user-secrets / env vars - never the
    // committed appsettings.json (see the placeholder note there).
    public class StripeSettings
    {
        public string PublishableKey { get; set; } = string.Empty;

        // Server-side secret. Treat like a password.
        public string SecretKey { get; set; } = string.Empty;

        // "whsec_..." used to verify the /stripe/webhook signature.
        public string WebhookSecret { get; set; } = string.Empty;

        // ISO currency for the Checkout Session. Property prices are entered
        // in NZD throughout the app.
        public string Currency { get; set; } = "nzd";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(SecretKey) &&
            !string.IsNullOrWhiteSpace(PublishableKey);
    }
}
