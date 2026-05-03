using Microsoft.AspNetCore.Mvc;
using Project_Hail_Mary.Models;

namespace Project_Hail_Mary.Controllers
{
	public class ShopController : Controller
	{
		private readonly AppDbContext _context;

		public ShopController(AppDbContext context)
		{
			_context = context;
		}

		public IActionResult Shop()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AddToCart([FromBody] CartRequest request)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return Unauthorized();

			var item = new Cart
			{
				UserId = userId.Value,
				ProductSlug = request.Slug,
				ProductName = request.Name,
				Quantity = request.Quantity,
				Size = request.Size
			};

			_context.Cart.Add(item);
			_context.SaveChanges();

			return Ok();
		}

		public IActionResult ProductDetail(string slug, string name, string category)
		{
			ViewBag.ProductSlug = slug ?? "unknown";
			ViewBag.ProductName = name ?? "Product";
			ViewBag.ProductCategory = category ?? "all";

			var productData = new Dictionary<string, (string Price, int Rating, int Reviews, string Desc1, string Desc2)>
			{
				["aphroe"] = ("₱389.00", 4, 243,
					"Aphrora is a soft, romantic fragrance inspired by Bath &amp; Body Works Sweet Pea, designed for the woman who loves gentle, fresh, and effortlessly feminine scents.",
					"This fragrance captures a light floral sweetness with a fresh, airy feel. It opens with a bright floral freshness, unfolds into a soft bouquet of delicate petals, and settles into a clean, subtle warmth that feels comforting and easy to wear."),
				["crest"] = ("₱389.00", 4, 118,
					"Crest is a crisp, elegant fragrance crafted for the modern woman. Inspired by timeless florals with a contemporary edge.",
					"Opens with a bright citrus top note, transitions to a lush floral heart, and dries down to a warm, powdery base that lingers beautifully throughout the day."),
				["florine"] = ("₱389.00", 5, 87,
					"Florine is a radiant, sun-kissed scent that evokes the feeling of a warm afternoon in a blooming garden.",
					"A harmonious blend of white flowers and soft musk, Florine is light yet lasting — effortless for everyday wear."),
				["afro"] = ("₱389.00", 4, 95,
					"Afro is a bold, confident fragrance for the modern man. Woody and warm with an undeniable presence.",
					"Starts strong with fresh green notes, evolves into a rich oud-touched heart, and settles on a dark, smoky base that commands attention."),
				["quiff"] = ("₱389.00", 3, 62,
					"Quiff is a sharp, clean scent built for the man who values precision and style.",
					"A classic fougère blend with bergamot, lavender, and cedarwood — timeless, refined, and unmistakably masculine."),
				["enchante"] = ("₱389.00", 5, 201,
					"Enchantè is a captivating, bewitching fragrance for the woman who loves to leave a lasting impression.",
					"A rich floral-oriental fusion that unfolds like a secret — warm amber and soft jasmine intertwined with a hint of vanilla musk."),
				["baco"] = ("₱389.00", 4, 74,
					"Baco is a smooth, grounding fragrance that speaks to quiet confidence and understated style.",
					"Built around warm tobacco and sandalwood, Baco wraps the wearer in depth and sophistication."),
				["antheia"] = ("₱389.00", 4, 133,
					"Antheia is a fresh floral scent inspired by Greek goddesses of flowers — blooming, alive, and radiant.",
					"Peony and rose at its heart, brightened by citrus and anchored by a light mossy base. Timeless and naturally beautiful."),
				["hypnos"] = ("₱389.00", 4, 58,
					"Hypnos is a deep, meditative scent for the man who moves through the world with quiet power.",
					"Dark woods and vetiver with subtle lavender — complex, layered, and undeniably magnetic."),
				["temple"] = ("₱389.00", 3, 44,
					"Temple is a sacred, incense-inspired fragrance with a modern masculine edge.",
					"Smoky resin and cedarwood, lifted by a breath of eucalyptus — evocative and deeply calming."),
				["athena"] = ("₱389.00", 5, 177,
					"Athena is a powerful, elegant fragrance for the woman who leads with grace and intelligence.",
					"Cool neroli and iris open into a bold rose-oud heart, closing with a confident woody-amber base."),
				["muller"] = ("₱389.00", 4, 89,
					"Muller is a classic, clean fragrance for everyday wear — fresh, reliable, and effortlessly put-together.",
					"A go-to fougère with aquatic top notes, a lavender heart, and a dry musk finish. Dependable and refined."),
				["slick"] = ("₱389.00", 4, 66,
					"Slick is a smooth, sleek fragrance for the confident, style-conscious man.",
					"Citrus and neroli up top, a leather-tinged heart, and a base of sandalwood and white musk. Urban. Polished."),
				["toper"] = ("₱389.00", 3, 38,
					"Toper is a rugged, adventurous scent inspired by open skies and untamed landscapes.",
					"Pine, juniper, and oakmoss form a raw, earthy foundation — bold, unpretentious, and naturally commanding."),
			};

			if (productData.TryGetValue(slug ?? "", out var data))
			{
				ViewBag.ProductPrice = data.Price;
				ViewBag.ProductRating = data.Rating;
				ViewBag.ProductReviews = data.Reviews;
				ViewBag.ProductDescription = data.Desc1;
				ViewBag.ProductDescription2 = data.Desc2;
			}
			else
			{
				ViewBag.ProductPrice = "₱389.00";
				ViewBag.ProductRating = 4;
				ViewBag.ProductReviews = 0;
				ViewBag.ProductDescription = $"{name} is a distinctive fragrance from bəzár.";
				ViewBag.ProductDescription2 = "A unique blend crafted with care.";
			}

			return View();
		}
	}

	// DTO for the AddToCart POST body
	public class CartRequest
	{
		public string Slug { get; set; }
		public string Name { get; set; }
		public int Quantity { get; set; }
		public string Size { get; set; }
	}
}