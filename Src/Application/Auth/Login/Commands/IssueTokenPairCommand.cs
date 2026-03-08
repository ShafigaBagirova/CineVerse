using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Login.Commands;

public sealed record IssueTokenPairCommand(JwtUserInfoDto User)
    : IRequest<TokenResponse>;
