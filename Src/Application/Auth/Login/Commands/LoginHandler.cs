using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;

namespace Application.Auth.Login.Commands;

public sealed class LoginHandler
    : IRequestHandler<LoginCommand, TokenResponse?>
{
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;

    public LoginHandler(
        IIdentityService identityService,
        IMediator mediator)
    {
        _identityService = identityService;
        _mediator = mediator;
    }

    public async Task<TokenResponse?> Handle(LoginCommand command, CancellationToken ct)
    {

        var (success, userId) = await _identityService
    .ValidateUserAsync(new LoginRequest
    {
        Login = command.Login,
        Password = command.Password
    });

        if (!success || string.IsNullOrWhiteSpace(userId))
            return null;

        var isEmailConfirmed = await _identityService.IsEmailConfirmedAsync(userId);

        if (!isEmailConfirmed)
            return null;

        var userInfo = await _identityService.GetUserInfoAsync(userId);

        if (userInfo is null)
            return null;

        var jwtUser = new JwtUserInfoDto(
            userInfo.UserId,
            userInfo.UserName,
            userInfo.Email,
            userInfo.Roles
        );

        return await _mediator.Send(new IssueTokenPairCommand(jwtUser), ct);
    }
}