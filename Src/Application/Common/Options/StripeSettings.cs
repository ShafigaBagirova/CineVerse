namespace Application.Common.Options;

public sealed class StripeSettings
{
    public string SecretKey { get; set; } = default!;
    public string PublishableKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
}
