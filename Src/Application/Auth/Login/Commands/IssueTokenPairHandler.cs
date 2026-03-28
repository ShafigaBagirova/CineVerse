using Application.Auth.Login.Dtos;
using Application.Auth.Refresh.Commands;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Login.Commands;

public sealed class IssueTokenPairHandler(
    IJwtTokenGenerator jwtTokenGenerator,
    IMediator mediator,
    ILogger<IssueTokenPairHandler> logger)
    : IRequestHandler<IssueTokenPairCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(IssueTokenPairCommand request, CancellationToken ct)
    {
        logger.LogInformation(
            "Issuing token pair for UserId: {UserId}",
            request.User.UserId);

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateAccessToken( request.User);

        var newRt = await mediator.Send(
            new CreateRefreshTokenCommand(request.User.UserId), ct);

        logger.LogInformation(
            "Token pair issued successfully for UserId: {UserId}, AccessTokenExpiresAtUtc: {ExpiresAtUtc}",
            request.User.UserId,
            expiresAtUtc);

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
            RefreshToken = newRt.Token
        };
    }
}