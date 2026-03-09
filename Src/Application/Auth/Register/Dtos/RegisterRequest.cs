using Domain.Enums;

namespace Application.Auth.Register.Dtos;

public sealed class RegisterRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get;set; }
    public Gender Gender { get; set; }
}
