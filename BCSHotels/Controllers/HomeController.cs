using System.Linq;
using System.Web.Mvc;
using BCSHotelsDomain.Persistence;
using PagedList;

namespace BCSHotels.Controllers
{
	public class HomeController : Controller
	{
		protected BCSContext ctx = new BCSContext();


		public ActionResult Index(string sortOrder, int? page)
		{
			ViewBag.CurrentSort = sortOrder;
			ViewBag.NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewBag.PriceSort = sortOrder == "Price" ? "price_desc" : "Price";
			ViewBag.RatingSort = sortOrder == "Rating" ? "rating_desc" : "Rating";

			var hotels = ctx.Hotels.Select(h => h);

			switch (sortOrder)
			{
				case "name_desc" :
					hotels = hotels.OrderByDescending(h => h.Name);
					break;
				case "Price" :
					hotels = hotels.OrderBy(h => h.Price);
					break;
				case "price_desc" :
					hotels = hotels.OrderByDescending(h => h.Price);
					break;
				case "Rating" :
					hotels = hotels.OrderBy(h => h.Rating);
					break;
				case "rating_desc" :
					hotels = hotels.OrderByDescending(h => h.Rating);
					break;
				default:
					hotels = hotels.OrderBy(h => h.Name);
					break;
			}

			var pageSize = 10;
			var pageNumber = (page ?? 1);

			return View(hotels.ToPagedList(pageNumber, pageSize));
		}

		public ActionResult List()
		{
			var model = ctx.Hotels;
			return View(model);
		}

		public ActionResult Details(int id)
		{
			var model = ctx.Hotels.FirstOrDefault(h => h.Id == id);
			return View(model);
		}


		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}