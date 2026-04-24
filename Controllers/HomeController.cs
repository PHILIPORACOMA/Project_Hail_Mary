using Microsoft.AspNetCore.Mvc;

namespace Project_Hail_Mary.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("about")]
        public IActionResult About()
        {
            return View();
        }
    }
}