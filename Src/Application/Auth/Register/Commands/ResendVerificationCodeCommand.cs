using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Register.Commands;

public sealed record ResendVerificationCodeCommand(string Email) : IRequest<BaseResponse>;
