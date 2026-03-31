using AdminDashboardMvc.Models;

namespace AdminDashboardMvc.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}