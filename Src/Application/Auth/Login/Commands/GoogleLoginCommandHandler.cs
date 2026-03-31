using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Login.Commands;

public class GoogleLoginCommandHandler
    : IRequestHandler<GoogleLoginCommand, BaseResponse<AuthResponse>>
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        IGoogleTokenValidator googleTokenValidator,
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _googleTokenValidator = googleTokenValidator;
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<AuthResponse>> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Google login started.");

        GoogleUserInfo googleUser;

        try
        {
            googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Google token validation failed.");
            return BaseResponse<AuthResponse>.Fail("Invalid Google token.");
        }

        if (!googleUser.EmailVerified)
        {
            _logger.LogWarning("Google email is not verified. Email: {Email}", googleUser.Email);
            return BaseResponse<AuthResponse>.Fail("Google email is not verified.");
        }

        var user = await _identityService.GetUserByEmailAsync(googleUser.Email, cancellationToken);

        if (user is null)
        {
            var createResult = await _identityService.CreateGoogleUserAsync(
                googleUser.Email,
                googleUser.GivenName,
                googleUser.FamilyName,
                "Google",
                googleUser.Subject,
                googleUser.PictureUrl,
                cancellationToken);

            if (!createResult.Success || string.IsNullOrWhiteSpace(createResult.UserId))
            {
                return BaseResponse<AuthResponse>.Fail(
                    createResult.Errors.FirstOrDefault() ?? "Google user creation failed.");
            }

            user = await _identityService.GetUserByIdAsync(createResult.UserId, cancellationToken);

            if (user is null)
            {
                return BaseResponse<AuthResponse>.Fail(
                    "User could not be loaded after Google registration.");
            }
        }
        var jwtUser = new JwtUserInfoDto(user.Id,user.Email,user.UserName ?? user.Email,user.Roles);

        var (accessToken, expiresAtUtc) = _jwtTokenGenerator.GenerateAccessToken(jwtUser);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        var fullName =user.FullName;
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = user.UserName ?? user.Email;

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            Email = user.Email,
            FullName = fullName
        };

        _logger.LogInformation("Google login completed successfully. UserId: {UserId}", user.Id);

        return BaseResponse<AuthResponse>.Ok(response, "Google login successful.");
    }
}