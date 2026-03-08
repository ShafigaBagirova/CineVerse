using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class CineVerseUser: IdentityUser
{
    public string FullName { get; set; }=default!;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVip { get; set; }
    public DateTime? VipExpiresAt { get; set; }
    public UserStatus Status { get; set; }= UserStatus.Active;
    public DateTime? LastLoginAt { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}
