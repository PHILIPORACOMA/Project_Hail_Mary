using Microsoft.AspNetCore.Mvc;

namespace Project_Hail_Mary.Controllers
{
    public class HomeController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
