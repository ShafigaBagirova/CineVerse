namespace Application.AdminDashboard.Dtos;

public sealed class RecentUserDto
{
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool IsVip { get; set; }
    public DateTime CreatedAt { get; set; }
}