using Application.Common.Responses;
using MediatR;

namespace Application.Auth.User.Commands;

public sealed record DeleteUserAvatarCommand()
    : IRequest<BaseResponse>;