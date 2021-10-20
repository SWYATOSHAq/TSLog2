using Microsoft.AspNetCore.Mvc;

namespace TSLog.Links.Services.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
