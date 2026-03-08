using Application.Auth.Password.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Password.Commands;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request)
    : IRequest<BaseResponse>;
