using Microsoft.AspNetCore.Mvc;
using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
	[Route("account")]
	public class AccountController : Controller
	{
		// GET: /account/register
		[HttpGet("register")]
		public IActionResult Register()
		{
			return View();
		}

		// POST: /account/register
		[HttpPost("register")]
		[ValidateAntiForgeryToken]
		public IActionResult Register(Users model)
		{
			if (!ModelState.IsValid)
				return View(model);

			// TODO: Save user to database here

			return RedirectToAction("Login");
		}

		// GET: /account/login
		[HttpGet("login")]
		public IActionResult Login()
		{
			return View();
		}

		// POST: /account/login
		[HttpPost("login")]
		[ValidateAntiForgeryToken]
		public IActionResult Login(Users model)
		{
			if (!ModelState.IsValid)
				return View(model);

			// TODO: Authenticate user here

			return RedirectToAction("Index", "Home");
		}

		// POST: /account/logout
		[HttpPost("logout")]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			// TODO: Clear session/cookie here

			return RedirectToAction("login");
		}
	}
}