using Application.Auth.Login.Dtos;
using MediatR;

namespace Application.Auth.Refresh.Commands;

public sealed record RefreshCommand(string RefreshToken) : IRequest<TokenResponse?>;

