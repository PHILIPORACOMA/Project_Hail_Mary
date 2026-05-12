using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
    [Route("Account")]
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

        // POST /account/login
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

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserFirstName", user.FirstName);
            HttpContext.Session.SetString("UserLastName", user.LastName);
            HttpContext.Session.SetString("UserProfilePicture", user.ProfilePicture ?? "");

            return RedirectToAction("Index", "Home");
        }

        // POST /account/logout
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET /account/login
        [HttpGet("login")]
        public IActionResult Login() => View();

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

            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                // To prevent email enumeration, pretend it succeeded
                return RedirectToAction("ForgotPasswordConfirmation", new { email = model.Email });
            }

            // Simulated email send
            return RedirectToAction("ForgotPasswordConfirmation", new { email = model.Email });
        }

        [HttpGet("forgot-password-confirmation")]
        public IActionResult ForgotPasswordConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");
            
            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost("reset-password")]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            // Update password (plain text currently based on login logic)
            user.Password = model.NewPassword;
            user.ConfirmPassword = model.ConfirmPassword;
            _db.SaveChanges();

            TempData["ResetSuccess"] = "Your password has been successfully reset. Please login.";
            return RedirectToAction("Login");
        }

		[HttpGet("orders")]
		public IActionResult Orders()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login");

			var orders = _db.Orders
				.Include(o => o.Items)
				.Where(o => o.UserId == userId.Value)
				.OrderByDescending(o => o.OrderDate)
				.ToList();

			return View(orders);
		}

		[HttpGet("profile")]
		public IActionResult Profile(string? edit)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login");

			var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value);
			if (user == null)
				return RedirectToAction("Login");

			if (edit == "address")
			{
				ViewBag.EditingAddress = true;
			}
			else if (edit == "info")
			{
				ViewBag.EditingInfo = true;
			}

			return View(user);
		}

		// POST /account/update-info
		[HttpPost("update-info")]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateInfo(string firstName, string lastName, string email)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return RedirectToAction("Login");

			var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value);
			if (user == null) return RedirectToAction("Login");

			user.FirstName = firstName;
			user.LastName = lastName;
			user.Email = email;
			_db.SaveChanges();

			// Update session
			HttpContext.Session.SetString("UserEmail", user.Email);
			HttpContext.Session.SetString("UserFirstName", user.FirstName);
			HttpContext.Session.SetString("UserLastName", user.LastName);

			return RedirectToAction("Profile");
		}

		// POST /account/update-address
		[HttpPost("update-address")]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateAddress(string address)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return RedirectToAction("Login");

			var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value);
			if (user == null) return RedirectToAction("Login");

			user.Address = address;
			_db.SaveChanges();
			return RedirectToAction("Profile");
		}

		// POST /account/upload-picture
		[HttpPost("upload-picture")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UploadPicture(IFormFile profilePicture)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return RedirectToAction("Login");

			var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value);
			if (user == null) return RedirectToAction("Login");

			if (profilePicture != null && profilePicture.Length > 0)
			{
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
				Directory.CreateDirectory(uploadsFolder);
				var fileName = $"user_{userId}{Path.GetExtension(profilePicture.FileName)}";
				var filePath = Path.Combine(uploadsFolder, fileName);
				using var stream = new FileStream(filePath, FileMode.Create);
				await profilePicture.CopyToAsync(stream);
				user.ProfilePicture = $"/uploads/{fileName}";
				_db.SaveChanges();
				HttpContext.Session.SetString("UserProfilePicture", user.ProfilePicture);
			}
			return RedirectToAction("Profile");
		}
	}
}