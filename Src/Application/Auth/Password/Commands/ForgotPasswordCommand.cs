using Application.Auth.Password.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Password.Commands;

public sealed record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<BaseResponse>;

