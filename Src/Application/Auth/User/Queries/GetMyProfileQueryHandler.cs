using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetMyProfileQueryHandler
    : IRequestHandler<GetMyProfileQuery, UserProfileDto?>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetMyProfileQueryHandler> _logger;

    public GetMyProfileQueryHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<GetMyProfileQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<UserProfileDto?> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetMyProfile failed: user is not authenticated.");
            return null;
        }

        _logger.LogInformation("Getting profile for current user: {UserId}", userId);

        var user = await _identityService.GetUserByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("User not found. UserId: {UserId}", userId);
            return null;
        }

        _logger.LogInformation("User profile retrieved successfully. UserId: {UserId}", userId);

        return user;
    }
}