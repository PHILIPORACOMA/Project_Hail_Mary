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

        // GET: /Cart/Index
        public async Task<IActionResult> Cart()
        {
            // Placeholder: Replace with actual logged-in User ID logic
            int currentUserId = 1;

            var cartItems = await _context.Cart
                .Where(c => c.UserId == currentUserId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, string action)
        {
            var item = await _context.Cart.FindAsync(cartItemId);

            if (item != null)
            {
                if (action == "inc")
                {
                    item.Quantity++; 
                }
                else if (action == "dec" && item.Quantity > 1)
                {
                    item.Quantity--; 
                }
                else if (action == "dec" && item.Quantity == 1)
                {
                    _context.Cart.Remove(item);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout
        public IActionResult Checkout()
        {
            return View(new CheckoutViewModel()); 
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (ModelState.IsValid) 
            {
                // TODO: Logic to process the order using model properties:
                // model.FirstName, model.LastName, model.Address, etc.

                return RedirectToAction("OrderConfirmation");
            }

            return View(model);
        }
    }
}