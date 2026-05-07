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
                    item.Quantity--;
                    if (item.Quantity <= 0)
                        _context.Cart.Remove(item);
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
            var model = new CheckoutViewModel
            {
                FirstName = HttpContext.Session.GetString("UserFirstName") ?? "",
                LastName = HttpContext.Session.GetString("UserLastName") ?? ""
            };

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
                var user = _context.Users.Find(userId.Value);
                if (user != null)
                {
                    user.Address = $"{model.Address}, {model.City}, {model.ZipCode}";
                    _context.SaveChanges();
                }
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
                TotalAmount = items.Sum(i => i.Price * i.Quantity)
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