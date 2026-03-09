using Application.Auth.Login.Dtos;
using Application.Auth.Refresh.Commands;
using Application.Auth.Register.Commands;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Login.Commands;

public sealed class IssueTokenPairHandler(
    IJwtTokenGenerator jwtTokenGenerator,
    IMediator mediator
) : IRequestHandler<IssueTokenPairCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(IssueTokenPairCommand request, CancellationToken ct)
    {
        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateAccessToken(request.User,request.User.Roles);

        var newRt = await mediator.Send(new CreateRefreshTokenCommand(request.User.UserId), ct);

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
            RefreshToken = newRt.Token
        };
    }
}
