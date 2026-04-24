using Microsoft.AspNetCore.Mvc;
using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
    public class RegisterController : Controller
    {
        [HttpPost]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register(Users user)
        {
            return View(user);
        }
    }
}
