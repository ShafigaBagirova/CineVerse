using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class RecentUserResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
