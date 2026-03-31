using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class DashboardService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
