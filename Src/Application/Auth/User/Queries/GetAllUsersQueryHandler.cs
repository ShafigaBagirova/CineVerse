using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, List<UserProfileDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IIdentityService identityService,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<List<UserProfileDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting all users");

        var users = await _identityService.GetAllUsersAsync();

        _logger.LogInformation("Retrieved all users successfully. Count: {UserCount}", users.Count);

        return users;
    }
}
