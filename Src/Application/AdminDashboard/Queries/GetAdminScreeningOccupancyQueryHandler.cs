using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AdminDashboard.Queries;

public sealed class GetAdminScreeningOccupancyQueryHandler
    : IRequestHandler<GetAdminScreeningOccupancyQuery, List<ScreeningOccupancyDto>>
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ILogger<GetAdminScreeningOccupancyQueryHandler> _logger;

    public GetAdminScreeningOccupancyQueryHandler(
        IAdminDashboardRepository adminDashboardRepository,
        ILogger<GetAdminScreeningOccupancyQueryHandler> logger)
    {
        _adminDashboardRepository = adminDashboardRepository;
        _logger = logger;
    }

    public async Task<List<ScreeningOccupancyDto>> Handle(
        GetAdminScreeningOccupancyQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting screening occupancy. Take: {Take}",
            request.Take);

        var result = await _adminDashboardRepository.GetScreeningOccupancyAsync(
            request.Take,
            cancellationToken);

        _logger.LogInformation(
            "Screening occupancy retrieved successfully. Count: {Count}",
            result.Count);

        return result;
    }
}