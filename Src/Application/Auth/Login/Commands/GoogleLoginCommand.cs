using Application.Auth.Login.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Login.Commands;

public sealed record GoogleLoginCommand(string IdToken)
    : IRequest<BaseResponse<AuthResponse>>;
