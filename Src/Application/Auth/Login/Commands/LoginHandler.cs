using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Login.Commands;

public sealed class LoginHandler
    : IRequestHandler<LoginCommand, TokenResponse?>
{
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IIdentityService identityService,
        IMediator mediator,
        ILogger<LoginHandler> logger)
    {
        _identityService = identityService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<TokenResponse?> Handle(LoginCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Login attempt for Login: {Login}",
            command.Login);

        var (success, userId) = await _identityService.ValidateUserAsync(
            new LoginRequest
            {
                Login = command.Login,
                Password = command.Password
            });

        if (!success || string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "Login failed. Invalid credentials for Login: {Login}",
                command.Login);

            return null;
        }

        var isEmailConfirmed = await _identityService.IsEmailConfirmedAsync(userId);

        if (!isEmailConfirmed)
        {
            _logger.LogWarning(
                "Login failed. Email not confirmed. UserId: {UserId}",
                userId);

            return null;
        }

        var userInfo = await _identityService.GetUserInfoAsync(userId);

        if (userInfo is null)
        {
            _logger.LogWarning(
                "Login failed. User info not found. UserId: {UserId}",
                userId);

            return null;
        }

        _logger.LogInformation(
            "Login successful for UserId: {UserId}",
            userId);

        var jwtUser = new JwtUserInfoDto(
            userInfo.UserId,
            userInfo.UserName,
            userInfo.Email,
            userInfo.Roles
        );

        return await _mediator.Send(new IssueTokenPairCommand(jwtUser), ct);
    }
}