using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using TSLog.Authorization;
using TSLog.Reports.BO;

namespace TSLog.Reports.WebUI.Controllers.v1;

[Route("/api/v1/[controller]")]
[ApiController]
public class ReportsController : Controller
{
    /// <summary>
    /// Максимальный период для отчёта по загружености склада (в сутках).
    /// </summary>
    private const int MaxReportPeriodDays = 180;
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

    [HttpGet("WarehouseFillingReport1")]
    public string GetWarehouseFillingData1()
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

            var elements = _storage.GetFillingReportData1();
            dynamic data = new ExpandoObject();

            data.P01_busy = 0;
            data.P01_total = 0;
            data.P01_percent = 0.0;
            data.P02_busy = 0;
            data.P02_total = 0;
            data.P02_percent = 0.0;
            data.P03_busy = 0;
            data.P03_total = 0;
            data.P03_percent = 0.0;

            var areas = new string[] { "HBW", "LBW", "LB1_1_2", "P01",
                "VKU", "LM2", "LF2", "LB1_3_E", "P02",
                "LF3", "P03" };
            var dict = (IDictionary<string, object>)data;
            void initArea(string area)
            {
                dict[area + "_busy_pallets"] = 0;
                dict[area + "_busy_pieces"] = 0;
                dict[area + "_busy"] = 0;
                dict[area + "_total_pallets"] = 0;
                dict[area + "_total_pieces"] = 0;
                dict[area + "_total"] = 0;
                dict[area + "_percent"] = 0.0;
            }
            foreach (var area in areas)
                initArea(area);

            void fillArea(string area, dynamic element)
            {
                var suffix = element.placeType == "EACH" ? "pieces" : "pallets";
                var busy = Convert.ToInt32(dict[area + "_busy_" + suffix]);
                busy += Convert.ToInt32(element.busy);
                dict[area + "_busy_" + suffix] = busy;
                var total = Convert.ToInt32(dict[area + "_total_" + suffix]);
                total += Convert.ToInt32(element.total);
                dict[area + "_total_" + suffix] = total;
            }
            foreach (var element in elements)
            {
                switch (element.area)
                {
                    case "HBW":
                        element.total *= 0.95; // Убираем 5% мест в HBW (необходимо для корректной работы области).
                        fillArea("HBW", element);
                        break;
                    case "LBW":
                        fillArea("LBW", element);
                        break;
                    case "LB1_1_2":
                        fillArea("LB1_1_2", element);
                        break;
                    case "P01":
                        fillArea("P01", element);
                        break;
                    case "PC1":
                        fillArea("P01", element);
                        break;
                    case "VKU":
                        fillArea("VKU", element);
                        break;
                    case "LM2":
                        fillArea("LM2", element);
                        break;
                    case "LF2":
                        fillArea("LF2", element);
                        break;
                    case "LB1_3_E":
                        fillArea("LB1_3_E", element);
                        break;
                    case "P02":
                        fillArea("P02", element);
                        break;
                    case "PC2":
                        fillArea("P02", element);
                        break;
                    case "LF3":
                        fillArea("LF3", element);
                        break;
                    case "P03":
                        fillArea("P03", element);
                        break;
                    case "PC3":
                        fillArea("P03", element);
                        break;
                }
            }
            
            void calculateArea(string area)
            {
                dict[area + "_busy"] = Convert.ToInt32(dict[area + "_busy_pallets"]) + Convert.ToInt32(dict[area + "_busy_pieces"]);
                dict[area + "_total"] = Convert.ToInt32(dict[area + "_total_pallets"]) + Convert.ToInt32(dict[area + "_total_pieces"]);
                dict[area + "_percent"] = Convert.ToInt32(dict[area + "_total"]) > 0 
                    ? Convert.ToDouble(dict[area + "_busy"]) / Convert.ToDouble(dict[area + "_total"]) 
                    : 0.0;
            }
            foreach (var area in areas)
                calculateArea(area);

            var text = JsonSerializer.Serialize(data);
            Response.StatusCode = 200;
            return text;
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }

    [HttpGet("WarehouseFillingReport2")]
    public string GetWarehouseFillingData2()
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

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong from date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Today.AddDays(1);
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");
            if ((toDate - fromDate).TotalDays > MaxReportPeriodDays)
                toDate = fromDate.AddDays(MaxReportPeriodDays);

            var data = _storage.GetFillingReportData2(fromDate, toDate);
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
