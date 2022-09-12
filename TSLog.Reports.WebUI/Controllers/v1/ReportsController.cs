using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using TSLog.Authorization;
using TSLog.Reports.BO;

namespace TSLog.Reports.WebUI.Controllers.v1
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IReportsStorage _storage;
        private readonly RightsChecker _rightsChecker;
        private readonly bool _useAuthentication;
        public ReportsController(IConfiguration configuration, IReportsStorage storage, RightsChecker rightsChecker)
        {
            _configuration = configuration;
            _storage = storage;
            _rightsChecker = rightsChecker;
            _useAuthentication = _configuration.GetValue<bool>("UseAuthentication");
        }

        [HttpGet("WarehouseFillingReport")]
        public string GetWarehouseFillingData()
        {
            try
            {
                Response.StatusCode = 500;
                var useReports = _rightsChecker.CheckRight("USE_REPORTS", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_WAREHOUSE_FILLING_REPORT", User);
                if (_useAuthentication && (!useReports || !getOperationDuration))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var elements = _storage.GetFillingReportData();
                dynamic data = new ExpandoObject();
                foreach (var element in elements)
                {
                    switch (element.area)
                    {
                        case "HBW":
                            data.HBW_busy = element.busy;
                            data.HBW_total = element.total;
                            data.HBW_percent = (double)element.busy / element.total;                            
                            break;
                        case "LBW":
                            data.LBW_busy = element.busy;
                            data.LBW_total = element.total;
                            data.LBW_percent = (double)element.busy / element.total;
                            break;
                        case "LB1_1_2":
                            data.LB1_1_2_busy = element.busy;
                            data.LB1_1_2_total = element.total;
                            data.LB1_1_2_percent = (double)element.busy / element.total;
                            break;
                        case "P01":
                            data.P01_busy = element.busy;
                            data.P01_total = element.total;
                            data.P01_percent = (double)element.busy / element.total;
                            break;
                        case "VKU":
                            data.VKU_busy = element.busy;
                            data.VKU_total = element.total;
                            data.VKU_percent = (double)element.busy / element.total;
                            break;
                        case "LM2":
                            data.LM2_busy = element.busy;
                            data.LM2_total = element.total;
                            data.LM2_percent = (double)element.busy / element.total;
                            break;
                        case "LB1_3_E":
                            data.LB1_3_E_busy = element.busy;
                            data.LB1_3_E_total = element.total;
                            data.LB1_3_E_percent = (double)element.busy / element.total;
                            break;
                        case "P02":
                            data.P02_busy = element.busy;
                            data.P02_total = element.total;
                            data.P02_percent = (double)element.busy / element.total;
                            break;
                        case "LF3":
                            data.LF3_busy = element.busy;
                            data.LF3_total = element.total;
                            data.LF3_percent = (double)element.busy / element.total;
                            break;
                        case "P03":
                            data.P03_busy = element.busy;
                            data.P03_total = element.total;
                            data.P03_percent = (double)element.busy / element.total;
                            break;
                    }
                }

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
