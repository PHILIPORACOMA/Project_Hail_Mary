using Microsoft.AspNetCore.Mvc;
using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Users model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: Add user registration logic here
            // - Check if email exists
            // - Hash password
            // - Save to database
            // - Send confirmation email, etc.

            return RedirectToAction("Login");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Users model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: Add authentication logic here
            // - Verify email and password
            // - Create session/cookie
            // - Redirect to home or dashboard

            return RedirectToAction("Index", "Home");
        }
    }
}