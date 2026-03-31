using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class RevenuChartItemResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
