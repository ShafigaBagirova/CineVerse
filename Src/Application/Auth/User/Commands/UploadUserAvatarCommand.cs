using Application.Auth.User.Dtos;
using Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Auth.User.Commands;

public sealed record UploadUserAvatarCommand(UploadAvatarRequest Request)
    : IRequest<BaseResponse<string>>;