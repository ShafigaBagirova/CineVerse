namespace Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
