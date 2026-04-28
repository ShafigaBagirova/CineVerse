using Application.Abstracts.Repositories;
using Application.Abstracts.Services;
using Application.Options;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Persistence.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtOptions> jwtOptions)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtOptions = jwtOptions.Value;
    }
    private static string GenerateToken(int byteLength)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return BitConverter.ToString(bytes).Replace("-", "");
    }
    public async Task<string> CreateAsync(User user, CancellationToken ct = default)
    {
        var token = GenerateToken(32);

        var refreshtoken = new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshExpirationDays)
        };

        await _refreshTokenRepository.AddAsync(refreshtoken, ct);
        return token;
    }

    public async Task<User?> ValidateAndConsumeAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var refreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(token, ct);
        if (refreshToken is null)
            return null;

        if (refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
            return null;


        var deleted = await _refreshTokenRepository.DeleteByTokenAsync(token, ct);
        if (!deleted)
            return null;

        return refreshToken.User;
    }
}

