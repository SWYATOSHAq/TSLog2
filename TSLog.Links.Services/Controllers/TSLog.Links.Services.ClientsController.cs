using Microsoft.AspNetCore.Mvc;

namespace TSLog.Links.Services.Controllers
{
    public class TSLog : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
