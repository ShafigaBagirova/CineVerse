using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard
{
    public class TopMoviesResponse : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
