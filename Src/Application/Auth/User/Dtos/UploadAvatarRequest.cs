using Microsoft.AspNetCore.Http;

namespace Application.Auth.User.Dtos;

public class UploadAvatarRequest
{
    public IFormFile Avatar { get; set; } = default!;
}
