using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // ─── Cart Page ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Unauthorized();
                return RedirectToAction("Login", "Account");
            }

            var items = _context.Cart
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Id)
                .ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CartPartial", items);

            return View(items);
        }

        // ─── Update Quantity (+/−) ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int cartItemId, string action)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var item = _context.Cart.FirstOrDefault(c => c.Id == cartItemId && c.UserId == userId);
            if (item != null)
            {
                if (action == "inc")
                    item.Quantity++;
                else if (action == "dec")
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;
                    }
                }
                _context.SaveChanges();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return RedirectToAction("Cart");

            return RedirectToAction("Cart");
        }

        // ─── Remove Item ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var item = _context.Cart.FirstOrDefault(c => c.Id == cartItemId && c.UserId == userId);
            if (item != null)
            {
                _context.Cart.Remove(item);
                _context.SaveChanges();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return RedirectToAction("Cart");

            return RedirectToAction("Cart");
        }

        // ─── Checkout GET ────────────────────────────────────────
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var items = _context.Cart
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Id)
                .ToList();

            if (!items.Any())
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return RedirectToAction("Cart");
                return RedirectToAction("Cart");
            }

            ViewBag.CartItems = items;

            var user = _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.Id == userId.Value);

            ViewBag.UserAddresses = user?.Addresses?.ToList() ?? new List<UserAddress>();

            var model = new CheckoutViewModel
            {
                FirstName = HttpContext.Session.GetString("UserFirstName") ?? "",
                LastName = HttpContext.Session.GetString("UserLastName") ?? "",
                Phone = user?.PhoneNumber ?? ""
            };

            var defaultAddress = user?.Addresses?.FirstOrDefault(a => a.IsDefault) ?? user?.Addresses?.FirstOrDefault();

            if (defaultAddress != null)
            {
                model.Address = defaultAddress.AddressLine;
                model.City = defaultAddress.City;
                model.ZipCode = defaultAddress.ZipCode;
                model.Phone = defaultAddress.PhoneNumber;
            }
            else if (user != null && !string.IsNullOrEmpty(user.Address))
            {
                var parts = user.Address.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length >= 3)
                {
                    model.Address = parts[0];
                    model.City = parts[1];
                    model.ZipCode = parts[2];
                }
                else if (parts.Length == 2)
                {
                    model.Address = parts[0];
                    // Handle "Cebu 2000" or similar
                    var lastPart = parts[1];
                    var lastSpaceIndex = lastPart.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        model.City = lastPart.Substring(0, lastSpaceIndex).Trim();
                        model.ZipCode = lastPart.Substring(lastSpaceIndex + 1).Trim();
                    }
                    else
                    {
                        model.City = lastPart;
                    }
                }
                else
                {
                    model.Address = user.Address;
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CheckoutPartial", model);

            return View(model);
        }

        // ─── Checkout POST ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var items = _context.Cart
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Id)
                .ToList();

            ViewBag.CartItems = items;

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_CheckoutPartial", model);
                return View(model);
            }

            // Store checkout info in session for PlaceOrder
            HttpContext.Session.SetString("Checkout_FullName", $"{model.FirstName} {model.LastName}");
            HttpContext.Session.SetString("Checkout_Address", $"{model.Address}, {model.City}, {model.ZipCode}");
            HttpContext.Session.SetString("Checkout_Phone", model.Phone);

            if (model.IsDefaultAddress)
            {
                var userAddresses = _context.UserAddresses.Where(a => a.UserId == userId.Value).ToList();
                foreach (var a in userAddresses)
                {
                    a.IsDefault = false;
                }

                _context.UserAddresses.Add(new UserAddress
                {
                    UserId = userId.Value,
                    Label = "Checkout Default",
                    AddressLine = model.Address,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    PhoneNumber = model.Phone,
                    IsDefault = true
                });
                
                _context.SaveChanges();
            }

            ViewBag.AddressSubmitted = true;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CheckoutPartial", model);

            return View(model);
        }

        // ─── Place Order ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var items = _context.Cart.Where(c => c.UserId == userId).ToList();
            if (!items.Any())
                return RedirectToAction("Shop", "Shop");

            var fullName = HttpContext.Session.GetString("Checkout_FullName") ?? "Unknown";
            var address = HttpContext.Session.GetString("Checkout_Address") ?? "No Address";
            var phone = HttpContext.Session.GetString("Checkout_Phone") ?? "0000000000";

            var order = new Order
            {
                UserId = userId.Value,
                FullName = fullName,
                Address = address,
                PhoneNumber = phone,
                OrderDate = DateTime.UtcNow,
                Status = "Processing",
                TotalAmount = items.Sum(i => i.Price * i.Quantity),
                TrackingNumber = "BZ-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            foreach (var item in items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductSlug = item.ProductSlug ?? "",
                    ProductName = item.ProductName ?? "",
                    Size = item.Size ?? "",
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            _context.Orders.Add(order);
            _context.Cart.RemoveRange(items);
            _context.SaveChanges();

            TempData["OrderSuccess"] = "Your order has been placed!";
            return RedirectToAction("Orders", "Account");
        }
    }
}