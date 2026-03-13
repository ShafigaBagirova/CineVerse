namespace Application.Common.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool SendEmails { get; set; } = true;

    public string SmtpHost { get; set; } = default!;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; } = true;

    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;

    public string ConfirmationBaseUrl { get; set; } = default!;
}
