using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Refresh.Queries;

public sealed class GetRefreshTokenWithUserQueryHandler(
    IRefreshTokenRepository refreshTokenRepository
) : IRequestHandler<GetRefreshTokenWithUserQuery, RefreshTokenWithUserResult?>
{
    public Task<RefreshTokenWithUserResult?> Handle(GetRefreshTokenWithUserQuery request, CancellationToken ct)
        => refreshTokenRepository.GetByTokenWithUserAsync(request.Token, ct);
}
