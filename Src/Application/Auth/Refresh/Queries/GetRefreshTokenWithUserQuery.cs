using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Auth.Refresh.Queries;

public sealed record RefreshTokenWithUserResult(
    string Token,
    DateTime ExpiresAtUtc,
    string UserId,
    string UserName,
    string Email
);
public sealed record GetRefreshTokenWithUserQuery(string Token)
    : IRequest<RefreshTokenWithUserResult?>;