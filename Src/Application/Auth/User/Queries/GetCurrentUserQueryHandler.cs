using Application.Auth.Login.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, JwtUserInfoDto?>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        IIdentityService identityService,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<JwtUserInfoDto?> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting current user info for UserId: {UserId}", request.UserId);

        var user = await _identityService.GetUserInfoAsync(request.UserId);

        if (user == null)
        {
            _logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            return null;
        }

        _logger.LogInformation("User info retrieved successfully. UserId: {UserId}", request.UserId);

        return user;
    }
}
