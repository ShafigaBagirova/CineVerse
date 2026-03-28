namespace Application.Auth.Login.Dtos;

public sealed class GoogleUserInfo
{
    public string Subject { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool EmailVerified { get; set; }
    public string Name { get; set; } = default!;
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? PictureUrl { get; set; }
}