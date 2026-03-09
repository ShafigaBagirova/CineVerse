using Application.Auth.Options;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Application.Auth.Refresh.Commands;

public sealed class CreateRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtOptions> jwtOptions)
    : IRequestHandler<CreateRefreshTokenCommand, CreateRefreshTokenResponse>
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<CreateRefreshTokenResponse> Handle(CreateRefreshTokenCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new InvalidOperationException("UserId cannot be empty.");

        var token = GenerateSecureHexToken(byteLength: 32); 
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwt.RefreshExpirationMinutes);

        var entity = new RefreshToken
        {
            UserId = request.UserId,
            Token = token,
            ExpiresAtUtc = expiresAtUtc
        };

        await refreshTokenRepository.AddAsync(entity, ct);

        return new CreateRefreshTokenResponse(token, expiresAtUtc);
    }

    private static string GenerateSecureHexToken(int byteLength)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToHexString(bytes);
    }
}