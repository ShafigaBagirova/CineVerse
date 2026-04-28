using Domain.Entities;

namespace CineVerse.Application.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<bool> DeleteByTokenAsync(string token, CancellationToken ct = default);
}
