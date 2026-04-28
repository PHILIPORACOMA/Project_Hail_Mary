using Microsoft.AspNetCore.Mvc;
using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // GET /account/register
        [HttpGet("register")]
        public IActionResult Register() => View();

        // POST /account/register
        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Users model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: hash password before saving
            _db.Users.Add(model);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

		// GET /account/login
		[HttpPost("login")]
		[ValidateAntiForgeryToken]
		public IActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			// Check credentials against DB
			var user = _db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

			if (user == null)
			{
				ModelState.AddModelError("", "Invalid email or password");
				return View(model);
			}

			return RedirectToAction("Index", "Home");
		}

		// GET /account/login
		[HttpGet("login")]
		public IActionResult Login()
		{
			return View();
		}

		// GET /account/forgot-password
		[HttpGet("forgot-password")]
        public IActionResult ForgotPassword() => View();

        // POST /account/forgot-password
        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: send reset email
            return RedirectToAction("Login");
        }

        // POST /account/logout
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // TODO: clear session/cookie
            return RedirectToAction("Login");
        }
    }
}