using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AdminDashboard.Queries;

public sealed class GetAdminRecentPaymentsQueryHandler
    : IRequestHandler<GetAdminRecentPaymentsQuery, List<RecentPaymentDto>>
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ILogger<GetAdminRecentPaymentsQueryHandler> _logger;

    public GetAdminRecentPaymentsQueryHandler(
        IAdminDashboardRepository adminDashboardRepository,
        ILogger<GetAdminRecentPaymentsQueryHandler> logger)
    {
        _adminDashboardRepository = adminDashboardRepository;
        _logger = logger;
    }

    public async Task<List<RecentPaymentDto>> Handle(
        GetAdminRecentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting recent payments. Take: {Take}",
            request.Take);

        var result = await _adminDashboardRepository.GetRecentPaymentsAsync(
            request.Take,
            cancellationToken);

        _logger.LogInformation(
            "Recent payments retrieved successfully. Count: {Count}",
            result.Count);

        return result;
    }
}