using Application.Auth.Login.Commands;
using Application.Auth.Login.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Refresh.Commands;

public sealed class RefreshHandler( IMediator mediator, ILogger<RefreshHandler> logger)
    : IRequestHandler<RefreshCommand, TokenResponse?>
{
    public async Task<TokenResponse?> Handle(RefreshCommand request, CancellationToken ct)
    {
        logger.LogInformation("Refresh token flow started");

        var user = await mediator.Send(
            new RefreshTokenValidateAndConsumeCommand(request.RefreshToken), ct);

        if (user is null)
        {
            logger.LogWarning("Refresh token validation failed");
            return null;
        }

        logger.LogInformation(
            "Refresh token validated successfully. Issuing new token pair for UserId: {UserId}",
            user);

        return await mediator.Send(new IssueTokenPairCommand(user), ct);
    }
}