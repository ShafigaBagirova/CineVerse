using Application.AdminDashboard.Dtos;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AdminDashboard.Queries;


public sealed class GetAdminTopMoviesQueryHandler
    : IRequestHandler<GetAdminTopMoviesQuery, List<TopMovieDto>>
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ILogger<GetAdminTopMoviesQueryHandler> _logger;

    public GetAdminTopMoviesQueryHandler(
        IAdminDashboardRepository adminDashboardRepository,
        ILogger<GetAdminTopMoviesQueryHandler> logger)
    {
        _adminDashboardRepository = adminDashboardRepository;
        _logger = logger;
    }

    public async Task<List<TopMovieDto>> Handle(
        GetAdminTopMoviesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting top movies. Take: {Take}",
            request.Take);

        var result = await _adminDashboardRepository.GetTopMoviesAsync(
            request.Take,
            cancellationToken);

        _logger.LogInformation(
            "Top movies retrieved successfully. Count: {Count}",
            result.Count);

        return result;
    }
}