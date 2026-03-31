using AdminDashboardMvc.Models;
using System.Text.Json;

namespace AdminDashboardMvc.Services;

public class DashboardService : IDashboardService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var model = new DashboardViewModel();

        var summaryResponse = await client.GetAsync("api/admin/dashboard/summary");
        if (summaryResponse.IsSuccessStatusCode)
        {
            var summaryJson = await summaryResponse.Content.ReadAsStringAsync();
            var summaryData = JsonSerializer.Deserialize<BaseResponse<DashboardSummaryResponse>>(
                summaryJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.Summary = summaryData?.Data;
        }

        var revenueResponse = await client.GetAsync("api/admin/dashboard/revenue-chart?days=7");
        if (revenueResponse.IsSuccessStatusCode)
        {
            var revenueJson = await revenueResponse.Content.ReadAsStringAsync();
            var revenueData = JsonSerializer.Deserialize<BaseResponse<List<RevenueChartItemResponse>>>(
                revenueJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.RevenueChart = revenueData?.Data ?? new List<RevenueChartItemResponse>();
        }

        var topMoviesResponse = await client.GetAsync("api/admin/dashboard/top-movies?take=5");
        if (topMoviesResponse.IsSuccessStatusCode)
        {
            var topMoviesJson = await topMoviesResponse.Content.ReadAsStringAsync();
            var topMoviesData = JsonSerializer.Deserialize<BaseResponse<List<TopMovieResponse>>>(
                topMoviesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.TopMovies = topMoviesData?.Data ?? new List<TopMovieResponse>();
        }

        var recentPaymentsResponse = await client.GetAsync("api/admin/dashboard/recent-payments?take=10");
        if (recentPaymentsResponse.IsSuccessStatusCode)
        {
            var recentPaymentsJson = await recentPaymentsResponse.Content.ReadAsStringAsync();
            var recentPaymentsData = JsonSerializer.Deserialize<BaseResponse<List<RecentPaymentResponse>>>(
                recentPaymentsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.RecentPayments = recentPaymentsData?.Data ?? new List<RecentPaymentResponse>();
        }

        var recentUsersResponse = await client.GetAsync("api/admin/dashboard/recent-users?take=10");
        if (recentUsersResponse.IsSuccessStatusCode)
        {
            var recentUsersJson = await recentUsersResponse.Content.ReadAsStringAsync();
            var recentUsersData = JsonSerializer.Deserialize<BaseResponse<List<RecentUserResponse>>>(
                recentUsersJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.RecentUsers = recentUsersData?.Data ?? new List<RecentUserResponse>();
        }

        var occupancyResponse = await client.GetAsync("api/admin/dashboard/screening-occupancy?take=10");
        if (occupancyResponse.IsSuccessStatusCode)
        {
            var occupancyJson = await occupancyResponse.Content.ReadAsStringAsync();
            var occupancyData = JsonSerializer.Deserialize<BaseResponse<List<ScreeningOccupancyResponse>>>(
                occupancyJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.ScreeningOccupancies = occupancyData?.Data ?? new List<ScreeningOccupancyResponse>();
        }

        return model;
    }
}