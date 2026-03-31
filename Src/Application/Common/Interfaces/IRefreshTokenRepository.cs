using Application.Auth.Refresh.Queries;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenWithUserResult?> GetByTokenWithUserAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<bool> DeleteByTokenAsync(string token, CancellationToken ct = default);
}