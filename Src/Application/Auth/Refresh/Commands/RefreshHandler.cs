using Application.Auth.Login.Commands;
using Application.Auth.Login.Dtos;
using MediatR;

namespace Application.Auth.Refresh.Commands;

public sealed class RefreshHandler(IMediator mediator)
    : IRequestHandler<RefreshCommand, TokenResponse?>
{
    public async Task<TokenResponse?> Handle(RefreshCommand request, CancellationToken ct)
    {
        var user = await mediator.Send(
            new RefreshTokenValidateAndConsumeCommand(request.RefreshToken), ct);

        if (user is null)
            return null;

        return await mediator.Send(new IssueTokenPairCommand(user), ct);
    }
}
