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
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

		[HttpGet("profile")]
		public IActionResult Profile()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login");

			var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value);
			if (user == null)
				return RedirectToAction("Login");

			return View(user);
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
			}
			return RedirectToAction("Profile");
		}
	}
}