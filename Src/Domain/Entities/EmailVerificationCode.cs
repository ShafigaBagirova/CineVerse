using Domain.Enums;

namespace Domain.Entities;

public class EmailVerificationCode:BaseEntity<Guid>
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CodeHash { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
