using CineVerse.Application.Repositories;
using CineVerse.Application.Users.Commands;
using Domain.Entities;
using MediatR;

namespace CineVerse.Infrastructure.Persistence.Services;

public sealed class RefreshTokenValidateAndConsumeHandler(IRefreshTokenRepository _refreshTokenRepository) :
    IRequestHandler<RefreshTokenValidateAndConsumeCommand, User?>
{
    public async Task<User?> Handle(RefreshTokenValidateAndConsumeCommand request, CancellationToken ct=default)
    {
        if (string.IsNullOrWhiteSpace(request.token))
            return null;

        var refreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(request.token, ct);
        if (refreshToken is null)
            return null;

        if (refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
            return null;


        var deleted = await _refreshTokenRepository.DeleteByTokenAsync(request.token, ct);
        if (!deleted)
            return null;

        return refreshToken.User;
    }
}
