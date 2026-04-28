using Application.Abstracts.Services;
using CineVerse.Application.Common.Options;
using CineVerse.Application.Repositories;
using Domain.Entities;
using MediatR;
using System.Security.Cryptography;

namespace CineVerse.Application.Users.Commands;

public sealed class CreateRefreshTokenHandler(IRefreshTokenRepository _refreshTokenRepository,JwtOptions _jwtOptions) : 
    IRequestHandler<CreateRefreshTokenCommand, string>
{
    private static string GenerateToken(int byteLength)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return BitConverter.ToString(bytes).Replace("-", "");
    }
    public async Task<string> Handle(CreateRefreshTokenCommand request, CancellationToken ct=default)
    {
        var token = GenerateToken(32);

        var refreshtoken = new RefreshToken
        {
            Token = token,
            UserId= request.user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshExpirationDays)
        };

        await _refreshTokenRepository.AddAsync(refreshtoken, ct);
        return token;
    }

}
