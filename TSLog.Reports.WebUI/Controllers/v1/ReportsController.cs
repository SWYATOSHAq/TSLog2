using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TSLog.Reports.BO;

namespace TSLog.Reports.WebUI.Controllers.v1
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IReportsStorage _storage;
        public ReportsController(IConfiguration configuration, IReportsStorage storage)
        {
            _configuration = configuration;
            _storage = storage;
        }

        [HttpGet("WarehouseFillingReport")]
        public string GetWarehouseFillingData()
        {
            try
            {
                Response.StatusCode = 500;
                /*
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
                if (_useAuthentication && (!useYard || !getOperationDuration))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }
                */

                var data = _storage.GetFillingReportData();

                var text = JsonSerializer.Serialize(data);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
    }
}
