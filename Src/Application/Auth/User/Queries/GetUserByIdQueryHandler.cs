using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserProfileDto?>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IIdentityService identityService,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<UserProfileDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting user by id. UserId: {UserId}", request.UserId);

        var user = await _identityService.GetUserByIdAsync(request.UserId);

        if (user == null)
        {
            _logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            return null;
        }

        _logger.LogInformation("User retrieved successfully. UserId: {UserId}", request.UserId);

        return user;
    }
}