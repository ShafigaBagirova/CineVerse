using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class DashboardSummaryResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
