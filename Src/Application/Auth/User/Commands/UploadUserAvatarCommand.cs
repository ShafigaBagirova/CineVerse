using Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Auth.User.Commands;

public sealed record UploadUserAvatarCommand(IFormFile File)
    : IRequest<BaseResponse<string>>;