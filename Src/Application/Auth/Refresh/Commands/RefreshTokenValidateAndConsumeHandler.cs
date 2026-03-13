using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;

namespace Application.Auth.Refresh.Commands;

using MediatR;
using Microsoft.Extensions.Logging;

public sealed class RefreshTokenValidateAndConsumeHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IIdentityService identityService,
    ILogger<RefreshTokenValidateAndConsumeHandler> logger)
    : IRequestHandler<RefreshTokenValidateAndConsumeCommand, JwtUserInfoDto?>
{
    public async Task<JwtUserInfoDto?> Handle(
        RefreshTokenValidateAndConsumeCommand request,
        CancellationToken ct)
    {
        logger.LogInformation("Validating refresh token");

        var rt = await refreshTokenRepository.GetByTokenWithUserAsync(request.RefreshToken, ct);

        if (rt is null)
        {
            logger.LogWarning("Refresh token validation failed. Token not found");
            return null;
        }

        if (rt.ExpiresAtUtc <= DateTime.UtcNow)
        {
            logger.LogWarning(
                "Refresh token validation failed. Token expired. UserId: {UserId}",
                rt.UserId);

            return null;
        }

        var deleted = await refreshTokenRepository.DeleteByTokenAsync(request.RefreshToken, ct);

        if (!deleted)
        {
            logger.LogWarning(
                "Refresh token consume failed. Token could not be deleted. UserId: {UserId}",
                rt.UserId);

            return null;
        }

        logger.LogInformation(
            "Refresh token consumed successfully. UserId: {UserId}",
            rt.UserId);

        var userInfo = await identityService.GetUserInfoAsync(rt.UserId);

        if (userInfo is null)
        {
            logger.LogWarning(
                "User not found during refresh token validation. UserId: {UserId}",
                rt.UserId);

            return null;
        }

        logger.LogInformation(
            "User info retrieved successfully for refresh flow. UserId: {UserId}",
            userInfo.UserId);

        return new JwtUserInfoDto(
            userInfo.UserId,
            userInfo.UserName,
            userInfo.Email,
            userInfo.Roles
        );
    }
}