using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class ApiSettings : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
