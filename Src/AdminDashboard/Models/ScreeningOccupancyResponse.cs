using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class ScreeningOccupancyResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
