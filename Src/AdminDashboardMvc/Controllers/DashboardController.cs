using AdminDashboardMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboardMvc.Controllers;

    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dashboardService.GetDashboardDataAsync();
            return View(model);
        }
    }

