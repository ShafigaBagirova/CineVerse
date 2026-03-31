using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class IDashboardService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
