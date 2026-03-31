using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class BaseResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
