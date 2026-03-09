using Application.Auth.UserName.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.UserName.Commands;

public sealed record UpdateUserNameCommand(string UserId,
    string NewUserName)
    : IRequest<BaseResponse>;
