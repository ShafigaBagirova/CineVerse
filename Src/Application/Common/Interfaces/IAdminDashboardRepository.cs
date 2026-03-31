using Application.AdminDashboard.Dtos;

namespace Application.Common.Interfaces;

public interface IAdminDashboardRepository
{
    Task<List<RevenueChartItemDto>> GetRevenueChartAsync(int days, CancellationToken cancellationToken);
    Task<List<TopMovieDto>> GetTopMoviesAsync(int take, CancellationToken cancellationToken);
    Task<List<RecentPaymentDto>> GetRecentPaymentsAsync(int take, CancellationToken cancellationToken);
    Task<List<RecentUserDto>> GetRecentUsersAsync(int take, CancellationToken cancellationToken);
    Task<List<ScreeningOccupancyDto>> GetScreeningOccupancyAsync(int take, CancellationToken cancellationToken);

}
