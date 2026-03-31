using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AdminDashboard.Queries;

public sealed class GetAdminRecentUsersQueryHandler
    : IRequestHandler<GetAdminRecentUsersQuery, List<RecentUserDto>>
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ILogger<GetAdminRecentUsersQueryHandler> _logger;

    public GetAdminRecentUsersQueryHandler(
        IAdminDashboardRepository adminDashboardRepository,
        ILogger<GetAdminRecentUsersQueryHandler> logger)
    {
        _adminDashboardRepository = adminDashboardRepository;
        _logger = logger;
    }

    public async Task<List<RecentUserDto>> Handle(
        GetAdminRecentUsersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting recent users. Take: {Take}",
            request.Take);

        var result = await _adminDashboardRepository.GetRecentUsersAsync(
            request.Take,
            cancellationToken);

        _logger.LogInformation(
            "Recent users retrieved successfully. Count: {Count}",
            result.Count);

        return result;
    }
}