using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;

namespace Application.Auth.Refresh.Commands;

using MediatR;

public sealed class RefreshTokenValidateAndConsumeHandler(
    IRefreshTokenRepository refreshTokenRepository, IIdentityService identityService)
    : IRequestHandler<RefreshTokenValidateAndConsumeCommand, JwtUserInfoDto?>
{
    public async Task<JwtUserInfoDto?> Handle(
        RefreshTokenValidateAndConsumeCommand request,
        CancellationToken ct)
    {
        var rt = await refreshTokenRepository.GetByTokenWithUserAsync(request.RefreshToken, ct);

        if (rt is null)
            return null;

        if (rt.ExpiresAtUtc <= DateTime.UtcNow)
            return null;

        var deleted = await refreshTokenRepository.DeleteByTokenAsync(request.RefreshToken, ct);

        if (!deleted)
            return null;
        var userInfo = await identityService.GetUserInfoAsync(rt.UserId);
        if (userInfo is null)
            return null;

        return new JwtUserInfoDto(
            userInfo.UserId,
            userInfo.UserName,
            userInfo.Email,
            userInfo.Roles
        );
    }
}