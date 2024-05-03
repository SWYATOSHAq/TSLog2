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
                    string startTime = root.GetProperty("shipping_time").GetString();
                    string driverName = root.GetProperty("driver").GetString();
                    string partnerManagerName = String.Empty;
                    string user = "auto_jitloper";

                    string fileName = Path.GetFileName(filePath);
                    string[] parts = fileName.Split('_');
                    string operationTypePart = parts[3].ToLower();
                    string operationTypeCode = String.Empty;
                    string vehicleType = String.Empty;
                    dynamic[] actions;

                    if (operationTypePart == "out" && direction == "out")
                    {
                        operationTypeCode = "O_BOX_MIX";
                        vehicleType = "1.5_TONS";
                        actions = new dynamic[]
                        {
                            new {
                                operationId = -1,
                                kind = "O",
                                id = "00420000MASTER NORMAL"
                            }
                        };
                    }
                    else
                    {
                        operationTypeCode = "I_BOX_MIX";
                        vehicleType = "20_TONS";
                        actions = new dynamic[]
                        {
                            new {
                                operationId = -1,
                                kind = "I",
                                id = "00420000MASTER NORMAL"
                            }
                        };
                    }

                    string stringActionKind = actions[0].kind;
                    string stringActionId = actions[0].id;
                    WarehouseAction warehouseAction = _yardDbConnectService.GetYardWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
                    var wareHouseActions = _yardDbConnectService.FindWarehouseActions(actionKind, partner, pattern ?? string.Empty, isPartnerUser);


                    var operationType = _yardDbConnectService.GetYardOperationType(operationTypeCode);

                    DateTime date = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
                    DateTime time = DateTime.ParseExact(startTime, "HH:mm", null);
                    DateTime planWorkStartDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
                    var vehicleKind = _yardDbConnectService.GetYardVehicleKind(vehicleType);
                    var planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, YardUtils._GetFillingPlanOperationDuration(operationType, vehicleKind, actions)));
                    //var planWorkStartDate = planWorkFinishDate.Subtract(new TimeSpan(0, 0, YardUtils._GetFillingPlanOperationDuration(operationType, vehicleKind, actions)));
                    var vehicleNumber = root.GetProperty("auto_num").GetString();
                    var vehicleBrand = root.GetProperty("auto_model").GetString();
                    var vehicleModel = "<Не указано>";
                    var trailerNumber = String.Empty;
                    var driverPhoneNumber = String.Empty;
                    var state = _yardDbConnectService.GetYardOperationState();
                    var priority = Operation.DefaultPriority;
                    var partner = _yardDbConnectService.GetYardPartner();
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

                    _yardDbConnectService.CreateYardOperation(operation, user, warehouseAction);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing json file: {FilePath}", filePath);
            }
        }
    }
}
