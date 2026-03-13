using Application.Common.Interfaces;
using Application.Common.Options;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Application.Auth.Refresh.Commands;

public sealed class CreateRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtOptions> jwtOptions,
    ILogger<CreateRefreshTokenCommandHandler> logger)
    : IRequestHandler<CreateRefreshTokenCommand, CreateRefreshTokenResponse>
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<CreateRefreshTokenResponse> Handle(CreateRefreshTokenCommand request, CancellationToken ct)
    {
        logger.LogInformation(
            "Creating refresh token for UserId: {UserId}",
            request.UserId);

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            logger.LogError("Refresh token creation failed. UserId is empty.");
            throw new InvalidOperationException("UserId cannot be empty.");
        }

        var token = GenerateSecureHexToken(byteLength: 32);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwt.RefreshExpirationMinutes);

        var entity = new RefreshToken
        {
            UserId = request.UserId,
            Token = token,
            ExpiresAtUtc = expiresAtUtc
        };

        await refreshTokenRepository.AddAsync(entity, ct);

        logger.LogInformation(
            "Refresh token created successfully. UserId: {UserId}, ExpiresAtUtc: {ExpiresAtUtc}",
            request.UserId,
            expiresAtUtc);

        return new CreateRefreshTokenResponse(token, expiresAtUtc);
    }

    private static string GenerateSecureHexToken(int byteLength)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToHexString(bytes);
    }
}