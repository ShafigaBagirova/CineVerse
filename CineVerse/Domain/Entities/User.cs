using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User:IdentityUser
{
    public string FullName { get; set; } = default!;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
