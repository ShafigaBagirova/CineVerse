using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class RecentPaymentResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
