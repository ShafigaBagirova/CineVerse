using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AdminDashboard.Queries;

public sealed class GetAdminRevenueChartQueryHandler
    : IRequestHandler<GetAdminRevenueChartQuery, List<RevenueChartItemDto>>
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ILogger<GetAdminRevenueChartQueryHandler> _logger;

    public GetAdminRevenueChartQueryHandler(
        IAdminDashboardRepository adminDashboardRepository,
        ILogger<GetAdminRevenueChartQueryHandler> logger)
    {
        _adminDashboardRepository = adminDashboardRepository;
        _logger = logger;
    }

    public async Task<List<RevenueChartItemDto>> Handle(
        GetAdminRevenueChartQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting admin revenue chart. Days: {Days}",
            request.Days);

        var result = await _adminDashboardRepository.GetRevenueChartAsync(
            request.Days,
            cancellationToken);

        _logger.LogInformation(
            "Admin revenue chart retrieved successfully. ItemCount: {Count}",
            result.Count);

        return result;
    }
}