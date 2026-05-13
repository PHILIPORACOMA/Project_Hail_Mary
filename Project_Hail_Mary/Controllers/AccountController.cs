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
            HttpContext.Session.SetString("UserPhoneNumber", user.PhoneNumber ?? "");
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

			var user = _db.Users
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.Id == userId.Value);
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

		// POST /account/update-profile
		[HttpPost("update-profile")]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateProfile(string firstName, string lastName, string email, DateTime? dateOfBirth)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return RedirectToAction("Login");

			var user = _db.Users.FirstOrDefault(u => u.Id == userId.Value);
			if (user == null) return RedirectToAction("Login");

			user.FirstName = firstName;
			user.LastName = lastName;
			user.Email = email;
            user.DateOfBirth = dateOfBirth;
			_db.SaveChanges();

			// Update session
			HttpContext.Session.SetString("UserEmail", user.Email);
			HttpContext.Session.SetString("UserFirstName", user.FirstName);
			HttpContext.Session.SetString("UserLastName", user.LastName);

			return RedirectToAction("Profile");
		}

        // POST /account/add-address
        [HttpPost("add-address")]
        [ValidateAntiForgeryToken]
        public IActionResult AddAddress(string label, string addressLine, string city, string zipCode, string phoneNumber, bool isDefault)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            if (isDefault)
            {
                var existingAddresses = _db.UserAddresses.Where(a => a.UserId == userId.Value);
                foreach (var addr in existingAddresses) addr.IsDefault = false;
            }

            var newAddress = new UserAddress
            {
                UserId = userId.Value,
                Label = label,
                AddressLine = addressLine,
                City = city,
                ZipCode = zipCode,
                PhoneNumber = phoneNumber,
                IsDefault = isDefault
            };

            _db.UserAddresses.Add(newAddress);
            _db.SaveChanges();

            return RedirectToAction("Profile");
        }

        // POST /account/delete-address
        [HttpPost("delete-address")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAddress(int addressId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var address = _db.UserAddresses.FirstOrDefault(a => a.Id == addressId && a.UserId == userId.Value);
            if (address != null)
            {
                _db.UserAddresses.Remove(address);
                _db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        // POST /account/set-default-address
        [HttpPost("set-default-address")]
        [ValidateAntiForgeryToken]
        public IActionResult SetDefaultAddress(int addressId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var addresses = _db.UserAddresses.Where(a => a.UserId == userId.Value).ToList();
            foreach (var addr in addresses)
            {
                addr.IsDefault = (addr.Id == addressId);
            }
            _db.SaveChanges();

            return RedirectToAction("Profile");
        }

        // GET /account/google-login
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            // Simulate Google Login callback
            // In a real app, this would use Microsoft.AspNetCore.Authentication.Google
            
            var mockUser = _db.Users.FirstOrDefault(u => u.Email == "googleuser@example.com");
            if (mockUser == null)
            {
                mockUser = new Users
                {
                    FirstName = "Google",
                    LastName = "User",
                    Email = "googleuser@example.com",
                    Password = "SocialLoginPassword123!", // Dummy password
                    ConfirmPassword = "SocialLoginPassword123!"
                };
                _db.Users.Add(mockUser);
                _db.SaveChanges();
            }

            HttpContext.Session.SetInt32("UserId", mockUser.Id);
            HttpContext.Session.SetString("UserEmail", mockUser.Email);
            HttpContext.Session.SetString("UserFirstName", mockUser.FirstName);
            HttpContext.Session.SetString("UserLastName", mockUser.LastName);

            return RedirectToAction("Index", "Home");
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