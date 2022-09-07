using Microsoft.AspNetCore.Mvc;

namespace TSLog.Reports.WebUI.Controllers.v1
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        [HttpGet("WarehouseFillingReport")]
        public string GetWarehouseFillingData()
        {
            string stringDate = Request.Query["date"];
            if (DateTime.TryParse(stringDate, out var date))
            { }
            else
            { }

            return string.Empty;
        }
    }
}
