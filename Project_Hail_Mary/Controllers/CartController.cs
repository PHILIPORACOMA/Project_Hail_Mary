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
                return RedirectToAction("Login", "Account");

            var items = _context.Cart
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Id)
                .ToList();

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
                return RedirectToAction("Cart");

            ViewBag.CartItems = items;
            return View(new CheckoutViewModel
            {
                FirstName = HttpContext.Session.GetString("UserFirstName") ?? "",
                LastName = HttpContext.Session.GetString("UserLastName") ?? ""
            });
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
                return View(model);

            ViewBag.AddressSubmitted = true;
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
            _context.Cart.RemoveRange(items);
            _context.SaveChanges();

            TempData["OrderSuccess"] = "Your order has been placed!";
            return RedirectToAction("Shop", "Shop");
        }
    }
}