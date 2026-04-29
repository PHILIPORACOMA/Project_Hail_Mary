using Microsoft.AspNetCore.Mvc;

namespace Project_Hail_Mary.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Shop()
        {
            return View();
        }
    }
}
