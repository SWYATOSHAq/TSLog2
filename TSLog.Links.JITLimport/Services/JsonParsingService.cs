using AngleSharp.Css;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TSLog.Common.BO;
using TSLog.Links.JITLimport.Helpers;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport.Services;

internal class JsonParsingService : IJsonParsingService
{
    private readonly IYardDbConnectService _yardDbConnectService;
    private readonly ILogger<JsonParsingService> _logger;
    public JsonParsingService(IYardDbConnectService yardDbConnectService, ILogger<JsonParsingService> logger)
    {
        _yardDbConnectService = yardDbConnectService;
        _logger = logger;
    }

    public void ParseJsonFiles(string directoryPath)
    {
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json");
        foreach (var filePath in jsonFiles)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = doc.RootElement;
                    string? direction = root.GetProperty("direction").GetString().ToLower();
                    string startDate = root.GetProperty("shipping_date").GetString();
                    var startTimeTemp = root.GetProperty("shipping_time").GetString();
                    string startTime = string.IsNullOrEmpty(startTimeTemp) ? "09:00" : startTimeTemp;
                    string driverName = root.GetProperty("driver").GetString();
                    string partnerManagerName = String.Empty;
                    string user = "auto_jitloper";

                    string fileName = Path.GetFileName(filePath);
                    string[] parts = fileName.Split('_');
                    string operationTypePart = parts[3].ToLower();
                    string operationTypeCode = String.Empty;
                    string vehicleType = String.Empty;
                    string? partnerActionId = root.GetProperty("num").GetString().Trim();
                    List<WarehouseAction> actionsToAdd = new();
                    dynamic[] actions;

                    if (operationTypePart == "out" && direction == "out")
                    {
                        operationTypeCode = "O_BOX_MIX";
                        vehicleType = "1.5_TONS";
                    }
                    else
                    {
                        operationTypeCode = "I_BOX_MIX";
                        vehicleType = "20_TONS";
                    }

                    var partner = _yardDbConnectService.GetYardPartner();
                    WarehouseAction warehouseAction = _yardDbConnectService.GetYardWarehouseAction(direction == "out" ? ActionKind.Outgo : ActionKind.Income, partner, partnerActionId);                    
                    var wareHouseActionsAvailiable = _yardDbConnectService.FindWarehouseActions(direction == "out" ? ActionKind.Outgo : ActionKind.Income, partner, string.Empty, false);
                    if (wareHouseActionsAvailiable.Contains(warehouseAction))
                    {
                        actionsToAdd.Add(warehouseAction);

                        var operationType = _yardDbConnectService.GetYardOperationType(operationTypeCode);

                        var dynamicActions = YardUtils.WarehouseActionsToDynamic(actionsToAdd.ToArray());
                        DateTime date = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
                        DateTime time = DateTime.ParseExact(startTime, "HH:mm", null);
                        DateTime planWorkStartDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
                        var vehicleKind = _yardDbConnectService.GetYardVehicleKind(vehicleType);
                        var planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, YardUtils._GetFillingPlanOperationDuration(operationType, vehicleKind, dynamicActions)));
                        var vehicleNumberTemp = root.GetProperty("auto_num").GetString();
                        var vehicleNumber = string.IsNullOrEmpty(vehicleNumberTemp) ? "<Не указано>" : vehicleNumberTemp;
                        var autoModelTemp = root.GetProperty("auto_model").GetString();
                        var vehicleBrand = string.IsNullOrEmpty(autoModelTemp) ? "<Не указано>" : autoModelTemp;
                        var vehicleModel = "<Не указано>";
                        var trailerNumber = String.Empty;
                        var driverPhoneNumber = String.Empty;
                        var state = _yardDbConnectService.GetYardOperationState();
                        var priority = Operation.DefaultPriority;
                        var manager = _yardDbConnectService.GetYardPartnerManagers(partner);
                        var partnerManagerPhoneNumber = manager.PhoneNumber;
                        var partnerManagerEmail = manager.Email;

                        var operation = new Operation(-1, null, operationType, planWorkStartDate, planWorkFinishDate,
                        vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
                        driverName, driverPhoneNumber,
                        state,
                        priority,
                        partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                        manager);
                        operation.Note = String.Empty;
                        operation.Parking = false;

                        _yardDbConnectService.CreateYardOperation(operation, user, actionsToAdd.ToArray());

                    } else
                    {
                        _logger.LogWarning("PartnerAction {partnerActionId} not found in DB.", partnerActionId);
                    }                       
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing json file: {FilePath}", filePath);
            }
        }
    }
}
