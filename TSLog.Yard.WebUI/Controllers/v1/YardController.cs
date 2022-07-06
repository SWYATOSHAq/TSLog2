using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text.Json;
using TSLog.Authorization;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using TSLog.Yard.Reporting;
using static TSLog.Yard.BO.OperationType;

namespace TSLog.Yard.WebUI.Pages.api.v1
{
    /// <summary>
    /// Контроллер api модуля управления двором 
    /// </summary>
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class YardController : ControllerBase
    {
        // Средее время выгрузки/погрузки паллеты/коробки/штуки
        private const int PalletUnloadTime = 93;
        private const int BoxUnloadTime = 20; 
        private const int PieceUnloadTime = 25;
        private const int PalletLoadTime = 93;
        private const int BoxLoadTime = 20;
        private const int PieceLoadTime = 25; 

        private readonly IConfiguration _configuration;
        private readonly IYardStorage _storage;
        private readonly RightsChecker _rightsChecker;
        private readonly bool _useAuthentication;
        public YardController(IConfiguration configuration, IYardStorage storage, RightsChecker rightsChecker)
        {
            _configuration = configuration;
            _storage = storage;
            _rightsChecker = rightsChecker;
            _useAuthentication = _configuration.GetValue<bool>("UseAuthentication");
        }

        private static async Task<dynamic?> GetInputData(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(body);
            string json = await reader.ReadToEndAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }
        private dynamic OperationToDynamic(Operation operation)
        {
            dynamic result = new ExpandoObject();
            result.id = operation.Id;

            result.stateId = operation.State.Id;
            result.stateCode = operation.State.Code;
            result.stateName = operation.State.Name;
            result.stateShortName = operation.State.ShortName;
            result.stateDisplayName = operation.State.DisplayName;

            result.typeId = operation.Type.Id;
            result.typeCode = operation.Type.Code;
            result.typeName = operation.Type.Name;
            result.typeShortName = operation.Type.ShortName;
            result.typeDisplayName = operation.Type.DisplayName;            
            result.directionName = operation.Type.DirectionName;

            result.vehicleKindId = operation.VehicleKind.Id;
            result.vehicleKindCode = operation.VehicleKind.Code;
            result.vehicleKindName = operation.VehicleKind.Name;
            result.vehicleKindShortName = operation.VehicleKind.ShortName;
            result.vehicleKindDisplayName = operation.VehicleKind.DisplayName;

            result.startDate = operation.StartDate;
            result.planWorkStartDate = operation.PlanWorkStartDate;
            result.factWorkStartDate = operation.FactWorkStartDate;
            result.planWorkFinishDate = operation.PlanWorkFinishDate;
            result.factWorkFinishDate = operation.FactWorkFinishDate;
            result.finishDate = operation.FinishDate;
            result.planDuration = (operation.PlanWorkFinishDate - operation.PlanWorkStartDate).TotalSeconds;
            result.factDuration = (operation.FactWorkFinishDate - operation.FactWorkStartDate).TotalSeconds;
            result.planLaborCost = operation.PlanLaborCost;
            result.planLinearLaborCost = operation.PlanLinearLaborCost;
            result.factLaborCost = operation.FactLaborCost;

            result.gateId = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
                operation.Gate.Code :
                string.Empty;
            result.gateCode = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ? 
                operation.Gate.Code : 
                string.Empty;
            result.gateShortName = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ? 
                operation.Gate.ShortName : 
                string.Empty;
            result.gateShortName = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
                operation.Gate.ShortName :
                string.Empty;
            result.gateDiaplayName = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
                operation.Gate.DisplayName :
                string.Empty;

            result.vehicleBrand = operation.VehicleBrand;
            result.lowerVehicleBrand = operation.VehicleBrand.ToLower();
            result.vehicleModel = operation.VehicleModel;
            result.lowerVehicleModel = operation.VehicleModel.ToLower();
            result.vehicleNumber = operation.VehicleNumber;
            result.lowerVehicleNumber = operation.VehicleNumber.ToLower();
            result.trailerNumber = operation.TrailerNumber;
            result.lowerTrailerNumber = operation.TrailerNumber.ToLower();

            result.driverName = operation.DriverName;
            result.lowerDriverName = operation.DriverName.ToLower();
            
            result.driverPhoneNumber = operation.DriverPhoneNumber;
            result.normalizedDriverPhoneNumber = operation.DriverPhoneNumber;
            result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace(" ", "");
            result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace("-", "");
            result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace("+", "");
            result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace("(", "");
            result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace(")", "");

            result.partnerManagerName = operation.PartnerManagerName;
            result.partnerManagerPhoneNumber = operation.PartnerManagerPhoneNumber;
            result.partnerManagerEmail = operation.PartnerManagerEmail;
            result.responsibleOfficer = operation.ResponsibleOfficerFullName;
            result.note = operation.Note;

            result.partnerId = operation.AnyPartnerId;
            result.partnerActionId = operation.PartnerActionsIds;
            result.lowerPartnerActionId = result.partnerActionId.ToLower();
            result.partnersNames = Partner.Serialize(operation.GetPartners());

            result.createDate = operation.CreateDate;
            try
            {
                result.creator = _storage.GetEmployeeByAdLogin(operation.Creator).FullName;
            }
            catch
            {
                result.creator = operation.Creator;
            }
            
            result.editDate = operation.EditDate;
            try
            {
                result.editor = _storage.GetEmployeeByAdLogin(operation.Editor).FullName;
            }
            catch
            {
                result.editor = operation.Editor;
            }            

            result.displayColor = operation.DisplayColor;

            return result;
        }
        private dynamic[] OperationsToDynamic(Operation[] operations)
        {
            var result = new List<dynamic>();
            foreach (Operation operation in operations)
                result.Add(OperationToDynamic(operation));
            return result.ToArray();
        }
        private string OperationToJson(Operation operation)
        {
            var temp = OperationToDynamic(operation);
            return JsonSerializer.Serialize(temp);
        }
        private string OperationsToJson(Operation[] operations)
        {
            var temp = OperationsToDynamic(operations);
            return JsonSerializer.Serialize(temp);
        }
        private HashSet<string> GetPartnersIds()
        {
            var userName = _rightsChecker.GetUserName(User);
            return _rightsChecker.GetPartnersIdsByUser(userName);
        }

        #region Reference data
        private bool CheckPartner(string partnerId)
        {
            var userName = _rightsChecker.GetUserName(User);
            var partnerIds = _rightsChecker.GetPartnersIdsByUser(userName);
            return partnerIds.Contains(partnerId);
        }
        [HttpGet("IsPartnerUser")]
        public string GetIsPartnerIser() 
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                if (_useAuthentication && !useYard)
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var isPartnerUser = _rightsChecker.CheckRight("IS_PARTNER_USER", User);
                var text = JsonSerializer.Serialize(isPartnerUser);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("OperationDuration")]
        public string GetOperationDuration()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
                if (_useAuthentication && (!useYard || !getOperationDuration)) 
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }
                
                string stringOperationType = Request.Query["operation_type"];
                string stringVehicleKind = Request.Query["vehicle_kind"];

                if (string.IsNullOrWhiteSpace(stringOperationType))
                    return "Operation type is empty";
                if (string.IsNullOrWhiteSpace(stringVehicleKind))
                    return "Vehicle kind is empty";

                var operationType = _storage.GetOperationType(stringOperationType);
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
                var timeSpan = _storage.GetDuration(operationType, vehicleKind, out int constantDuration, out int VariableDuration);
                var text = JsonSerializer.Serialize(timeSpan);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        private int GetDuration2(BO.OperationType operationType, VehicleKind vehicleKind, dynamic[] actions)
        {
            var standardTotalDuration = _storage.GetDuration(operationType, vehicleKind,
                   out int standardConstantDuration, out int standardVariableDuration);
            var totalTime = 0.0;
            foreach (var action in actions)
            {
                var kind = action.kind == "O" ? ActionKind.Outgo : ActionKind.Income;
                var palletTime = action.kind == "O" ? PalletLoadTime : PalletUnloadTime;
                var boxTime = action.kind == "O" ? BoxLoadTime : BoxUnloadTime;
                var pieceTime = action.kind == "O" ? PieceLoadTime : PieceUnloadTime;
                string id = action.id;
                LineBase[] lines = _storage.GetWarehouseActionLines(kind, id);
                var actionTime = 0.0;
                foreach (var line in lines)
                {
                    if (line.PlanPalletsCount > 0.0 || line.PlanBoxesCount > 0.0)
                        actionTime += line.PlanPalletsCount * palletTime + line.PlanBoxesCount * boxTime;
                    else
                    {
                        if (line.PlanVolume > 0.0)
                            actionTime += line.PlanVolume / (vehicleKind.CargoVolume * 1000 * 1000 * 1000) * standardTotalDuration;
                        else if (line.PlanGrossWeight > 0.0)
                            // Килограммы переводим в милиграммы
                            actionTime += line.PlanGrossWeight / (vehicleKind.Capacity * 1000 * 1000) * standardTotalDuration;
                    }
                }
                if (actionTime <= 0.0 && action.kind == "O")
                {
                    _storage.GetAssembliedOutgo(id, out double palletsCount, out double eachesCount);
                    actionTime = palletsCount * palletTime + eachesCount * pieceTime;
                }
                totalTime += actionTime;
            }

            totalTime = Math.Round(totalTime);
            if (totalTime <= 0.0 || totalTime > standardTotalDuration)
                totalTime = standardTotalDuration;
            // 2022.06.30 убрал учёт постоянных микроопераций по указанию Алексея Охотникова
            //else
            //    totalTime += standardConstantDuration;
            return (int)totalTime;
        }
        [HttpPost("OperationDuration2")]
        async public Task<string> GetOperationDuration2()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
                if (_useAuthentication && (!useYard || !getOperationDuration))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationType = data.operationTypeCode;
                if (string.IsNullOrWhiteSpace(stringOperationType))
                    return "Operation type is empty";
                var operationType = _storage.GetOperationType(stringOperationType);
                string stringVehicleKind = data.vehicleKindCode;
                if (string.IsNullOrWhiteSpace(stringVehicleKind))
                    return "Vehicle kind is empty";
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
                dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();
               
                var timeSpan = GetDuration2(operationType, vehicleKind, actions);
                var text = JsonSerializer.Serialize(timeSpan);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("OperationLaborCost")]
        public string GetOperationLaborCost()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
                if (_useAuthentication && (!useYard || !getOperationDuration))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringOperationType = Request.Query["operation_type"];
                string stringVehicleKind = Request.Query["vehicle_kind"];

                if (string.IsNullOrWhiteSpace(stringOperationType))
                    return "Operation type is empty";
                if (string.IsNullOrWhiteSpace(stringVehicleKind))
                    return "Vehicle kind is empty";

                var operationType = _storage.GetOperationType(stringOperationType);
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
                var laborCost = _storage.GetLaborCost(operationType, vehicleKind);
                var linearLaborCost = _storage.GetLinearLaborCost(operationType, vehicleKind);

                dynamic result = new ExpandoObject();
                result.laborCost = laborCost;
                result.linearLaborCost = linearLaborCost;

                var text = JsonSerializer.Serialize(result);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        private double GetLaborCostK(BO.OperationType operationType, VehicleKind vehicleKind, dynamic[] actions)
        {
            var standardTotalDuration = _storage.GetDuration(operationType, vehicleKind,
                   out int standardConstantDuration, out int standardVariableDuration);
            var duration = GetDuration2(operationType, vehicleKind, actions);

            return (double)duration / (double)standardTotalDuration;
        }
        [HttpPost("OperationLaborCost2")]
        async public Task<string> GetOperationLaborCost2()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
                if (_useAuthentication && (!useYard || !getOperationDuration))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationType = data.operationTypeCode;
                if (string.IsNullOrWhiteSpace(stringOperationType))
                    return "Operation type is empty";
                var operationType = _storage.GetOperationType(stringOperationType);
                string stringVehicleKind = data.vehicleKindCode;
                if (string.IsNullOrWhiteSpace(stringVehicleKind))
                    return "Vehicle kind is empty";
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
                dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

                var k = GetLaborCostK(operationType, vehicleKind, actions);
                var laborCost = _storage.GetLaborCost(operationType, vehicleKind);
                var linearLaborCost = _storage.GetLinearLaborCost(operationType, vehicleKind);
               
                dynamic result = new ExpandoObject();
                result.laborCost = Math.Round(laborCost * k);
                result.linearLaborCost = Math.Round(linearLaborCost * k);

                var text = JsonSerializer.Serialize(result);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("AvalibleIntervals")]
        public string GetAvalibleIntervals()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getBusyIntervals = _rightsChecker.CheckRight("GET_BUSY_INTERVALS", User);
                if (_useAuthentication && (!useYard || !getBusyIntervals))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringDate = Request.Query["date"];
                string stringOperationType = Request.Query["operation_type"];
                string stringVehicleKind = Request.Query["vehicle_kind"];

                if (string.IsNullOrWhiteSpace(stringOperationType))
                    return "Operation type is empty";
                if (string.IsNullOrWhiteSpace(stringVehicleKind))
                    return "Vehicle kind is empty";

                var date = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(stringDate) && !DateTime.TryParse(stringDate.Trim(), out date))
                    return $"Wrong date {stringDate}";

                var operationType = _storage.GetOperationType(stringOperationType);
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);

                var span = 2;
                var periods = _storage.GetAvalibleIntervals(date, date.AddDays(span), operationType, vehicleKind);
                var intervals = Interval.Union(periods);
                var text = JsonSerializer.Serialize(intervals);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("PartnerManager")]
        public string GetPartnerManager()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getPartnermanager = _rightsChecker.CheckRight("GET_PARTNER_MANAGER", User);
                if (_useAuthentication && (!useYard || !getPartnermanager))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string partnerId = Request.Query["partner_id"];
                if (string.IsNullOrWhiteSpace(partnerId))
                    return "Partner code is empty!";

                var partner = _storage.GetPartner(partnerId);
                var managers =_storage.GetPartnerManagers(partner);
                var text = JsonSerializer.Serialize(managers[0]);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("ManagerByPartner")]
        public string GetManagerByPartner()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getPartnermanager = _rightsChecker.CheckRight("GET_MANAGER_BY_PARTNER", User);
                if (_useAuthentication && (!useYard || !getPartnermanager))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }                
                var text = "";
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion

        #region Operations
        [HttpGet("Operation")]
        public string GetOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperation = _rightsChecker.CheckRight("GET_OPERATION", User);
                var getOperationByPartner = _rightsChecker.CheckRight("GET_OPERATION_BY_PARTNER", User);
                if (_useAuthentication && (!useYard || !(getOperation || getOperationByPartner)))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["id"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty";

                temp = temp.Trim();
                if (long.TryParse(temp, out long id))
                {
                    var operation = _storage.GetOperation(id);

                    // TODO: check partner;

                    var dynamicOperation = OperationToDynamic(operation);
                    var dynamicActions = new List<dynamic>();
                    foreach (var action in operation.GetActions()) 
                    {
                        dynamic dynamicAction = new ExpandoObject();
                        dynamicAction.kind = action.Kind == ActionKind.Outgo ? "O" : "I";
                        dynamicAction.id = action.Id;
                        dynamicActions.Add(dynamicAction);
                    }
                    var k = GetLaborCostK(operation.Type, operation.VehicleKind, dynamicActions.ToArray());
                    dynamicOperation.planLaborCost = Math.Round(dynamicOperation.planLaborCost * k);
                    dynamicOperation.planLinearLaborCost = Math.Round(dynamicOperation.planLinearLaborCost * k);
                    
                    var text = JsonSerializer.Serialize(dynamicOperation);
                    Response.StatusCode = 200;
                    return text;
                }
                else
                    return $"Wrong operation id {temp}~";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("Operations")]
        public string GetOperations()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperations = _rightsChecker.CheckRight("GET_OPERATIONS", User);
                if (_useAuthentication && (!useYard || !getOperations))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringFromDate = Request.Query["from_date"];
                string stringToDate = Request.Query["to_date"];
                string stringGateCode = Request.Query["gate_code"];
                string stringPartnerId = Request.Query["partner_id"];
                string stringOperationTypeCode = Request.Query["operation_type_code"];
                string stringVehicleKindCode = Request.Query["vehicle_kind_code"];
                string stringStateCode = Request.Query["state_code"];
                string stringId = Request.Query["id"];
                string stringVehicleNumber = Request.Query["vehicle_number"];
                string stringDriverName = Request.Query["driver_name"];
                string stringDriverPhoneNumber = Request.Query["driver_phone_number"];
                string stringPartnerActionId = Request.Query["partner_action_id"];
                string stringWorker = Request.Query["worker"];

                var partner = !string.IsNullOrWhiteSpace(stringPartnerId) ? _storage.GetPartner(stringPartnerId) : Partner.Null;
                var gate = !string.IsNullOrWhiteSpace(stringGateCode) ? _storage.GetGate(stringGateCode) : Gate.Null;
                var operationType = !string.IsNullOrWhiteSpace(stringOperationTypeCode) ? _storage.GetOperationType(stringOperationTypeCode) : Null;
                var vehicleKind = !string.IsNullOrWhiteSpace(stringVehicleKindCode) ? _storage.GetVehicleKind(stringVehicleKindCode) : VehicleKind.Null;
                var state = !string.IsNullOrWhiteSpace(stringStateCode) ? _storage.GetOperationState(stringStateCode) : Entity.Null;

                var fromDate = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(stringFromDate))
                {
                    stringFromDate = stringFromDate.Trim();
                    if (!DateTime.TryParse(stringFromDate, out fromDate))
                        return $"Wrong from date {stringFromDate}";
                }
                var toDate = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(stringToDate))
                {
                    stringToDate = stringToDate.Trim();
                    if (!DateTime.TryParse(stringToDate, out toDate))
                        return $"Wrong to date {stringToDate}";
                }

                var workers = Array.Empty<Employee>();
                if (!string.IsNullOrWhiteSpace(stringWorker))
                    workers = _storage.FindEmployees(stringWorker.Trim());
                
                var span = 1;
                var operations = _storage.GetOperations(stringId, 
                    fromDate.Date, toDate.AddDays(span).AddTicks(-1),
                    operationType,
                    vehicleKind,
                    state,
                    gate,
                    partner,
                    string.Empty,
                    stringVehicleNumber,
                    stringDriverName,
                    stringDriverPhoneNumber,
                    stringPartnerActionId,
                    workers);
                operations = Operation.Filter(operations, partner, operationType, vehicleKind, gate, state,
                    stringVehicleNumber, stringDriverName, stringPartnerActionId);

                var list = new List<Operation>();
                var partnersIds = GetPartnersIds();
                foreach (var operation in operations)
                    if (operation.Partners.Length > 0 && partnersIds.Contains(operation.Partners[0].Id) || partnersIds.Count <= 0)
                        list.Add(operation);

                var text = OperationsToJson(list.ToArray());
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {                
                return exception.Message;
            }
        }
        [HttpGet("ActiveOperations")]
        public string GetActiveOperations()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperations = _rightsChecker.CheckRight("GET_ACTIVE_OPERATIONS", User);
                if (_useAuthentication && (!useYard || !getOperations))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var operations = _storage.GetActiveOperations();
                var text = OperationsToJson(operations);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("GuardOperations")]
        public string GetGuardOperations()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperations = _rightsChecker.CheckRight("GET_GUARD_OPERATIONS", User);
                if (_useAuthentication && (!useYard || !getOperations))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var operations = _storage.GetGuardOperations();
                var text = OperationsToJson(operations);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPut("CreateOperation")]
        async public Task<string> CreateOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var createOperation = _rightsChecker.CheckRight("CREATE_OPERATION", User);
                if (_useAuthentication && (!useYard || !createOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.operationTypeCode;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation type code is empty!";
                var operationType = _storage.GetOperationType(temp);

                temp = data.vehicleKindCode;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Vehicle kind code is empty!";
                var vehicleKind = _storage.GetVehicleKind(temp);

                temp = data.startDate;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Start date is empty!";
                if (!DateTime.TryParse(temp, out DateTime planWorkStartDate))
                    return $"Wrong start date {temp}!";

                dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

                temp = data.finishDate;
                var planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, GetDuration2(operationType, vehicleKind, actions)));
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > planWorkFinishDate)
                        planWorkFinishDate = tempFinishDate;
                }

                string vehicleBrand = data.vehicleBrand;
                if (string.IsNullOrWhiteSpace(vehicleBrand))
                    vehicleBrand = "<Не указано>";
                string vehicleModel = data.vehicleModel;
                if (string.IsNullOrWhiteSpace(vehicleModel))
                    vehicleModel = "<Не указано>";
                string vehicleNumber = data.vehicleNumber;
                if (string.IsNullOrWhiteSpace(vehicleNumber))
                    vehicleNumber = "<Не указано>";
                string trailerNumber = data.trailerNumber;

                string driverName = data.driverName;
                if (string.IsNullOrWhiteSpace(driverName))
                    driverName = "<Не указано>";
                string driverPhoneNumber = data.driverPhoneNumber;

                var state = _storage.GetOperationState("CREATED");

                temp = data.priority;
                var priority = (short)1;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!short.TryParse(temp, out priority))
                        return $"Wrong priority {temp}";
                }

                string partnerManagerName = data.partnerManagerName;
                string partnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
                string partnerManagerEmail = data.partnerManagerEmail;
                string note = data.note;

                string partnerId = data.partnerId;
                if (string.IsNullOrWhiteSpace(partnerId))
                    return "Partner id is empty!";

                var partner = _storage.GetPartner(partnerId);
                var manager = _storage.GetPartnerManagers(partner)[0];
                var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;

                var operation = new Operation(-1, null, operationType, planWorkStartDate, planWorkFinishDate,
                    vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
                    driverName, driverPhoneNumber,
                    state,
                    priority,
                    partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                    manager)
                {
                    Note = note
                };

                var user = _rightsChecker.GetUserName(User);
                _storage.AddOperation(operation, user);

                var text = OperationToJson(operation);
                Response.StatusCode = 201;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPut("CreateConfirmOperation")]
        async public Task<string> CreateConfirmOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var createOperation = _rightsChecker.CheckRight("CREATE_CONFIRM_OPERATION", User);
                if (_useAuthentication && (!useYard || !createOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.operationTypeCode;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation type code is empty!";
                var operationType = _storage.GetOperationType(temp);

                temp = data.vehicleKindCode;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Vehicle kind code is empty!";
                var vehicleKind = _storage.GetVehicleKind(temp);

                temp = data.startDate;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Start date is empty!";
                if (!DateTime.TryParse(temp, out DateTime startDate))
                    return $"Wrong start date {temp}!";

                temp = data.finishDate;
                var finishDate = startDate.Add(new TimeSpan(0, 0, _storage.GetDuration(operationType, vehicleKind, 
                    out int constantDuration, out int variableDuration)));
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > finishDate)
                        finishDate = tempFinishDate;
                }
                
                string vehicleBrand = data.vehicleBrand;
                if (string.IsNullOrWhiteSpace(vehicleBrand))
                    vehicleBrand = "<Не указано>";
                string vehicleModel = data.vehicleModel;
                if (string.IsNullOrWhiteSpace(vehicleModel))
                    vehicleModel = "<Не указано>";
                string vehicleNumber = data.vehicleNumber;
                if (string.IsNullOrWhiteSpace(vehicleNumber))
                    vehicleNumber = "<Не указано>";
                string trailerNumber = data.trailerNumber;                

                string driverName = data.driverName;
                if (string.IsNullOrWhiteSpace(driverName))
                    driverName = "<Не указано>";
                string driverPhoneNumber = data.driverPhoneNumber;

                var state = _storage.GetOperationState("CREATED");

                temp = data.priority;
                var priority = (short)1;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!short.TryParse(temp, out priority))
                        return $"Wrong priority {temp}";
                }

                string partnerManagerName = data.partnerManagerName;
                string partnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
                string partnerManagerEmail = data.partnerManagerEmail;
                string note = data.note;

                string partnerId = data.partnerId;
                if (string.IsNullOrWhiteSpace(partnerId))
                    return "Partner id is empty!";

                var partner = _storage.GetPartner(partnerId);
                var manager = _storage.GetPartnerManagers(partner)[0];
                var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;

                var operation = new Operation(-1, null, operationType, startDate, finishDate,
                    vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
                    driverName, driverPhoneNumber,
                    state,
                    priority,
                    partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                    manager)
                {
                    Note = note
                };

                var user = _rightsChecker.GetUserName(User);
                operation = _storage.AddOperation(operation, user);
                
                // Эту строку (по поиску сотрудника по пользователю AD) перенёс сюда, что бы не найдя сотрудника (ответственного)
                // нельзя было присвоить ворота
                var employee = _storage.GetEmployeeByAdLogin(user);
                operation.Gate = _storage.SetOperationGate(operation, null, user);                
                _storage.SetResponsibleOfficer(operation, employee, user);

                var text = OperationToJson(operation);
                Response.StatusCode = 201;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message + "\n" + exception.StackTrace + "\n" + exception.TargetSite;
            }
        }
        [HttpPut("CreateOperationByPartner")]
        async public Task<string> CreateOperationByPartner()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var createOperation = _rightsChecker.CheckRight("CREATE_OPERATION_BY_PARTNER", User);
                if (_useAuthentication && (!useYard || !createOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.operationTypeCode;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation type code is empty!";
                var operationType = _storage.GetOperationType(temp);

                temp = data.vehicleKindCode;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Vehicle kind code is empty!";
                var vehicleKind = _storage.GetVehicleKind(temp);

                temp = data.startDate;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Start date is empty!";
                if (!DateTime.TryParse(temp, out DateTime startDate))
                    return $"Wrong start date {temp}!";

                dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

                temp = data.finishDate;
                var finishDate = startDate.Add(new TimeSpan(0, 0, GetDuration2(operationType, vehicleKind, actions)));
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > finishDate)
                        finishDate = tempFinishDate;
                }

                string vehicleBrand = data.vehicleBrand;
                if (string.IsNullOrWhiteSpace(vehicleBrand))
                    vehicleBrand = "<Не указано>";
                string vehicleModel = data.vehicleModel;
                if (string.IsNullOrWhiteSpace(vehicleModel))
                    vehicleModel = "<Не указано>";
                string vehicleNumber = data.vehicleNumber;
                if (string.IsNullOrWhiteSpace(vehicleNumber))
                    vehicleNumber = "<Не указано>";
                string trailerNumber = data.trailerNumber;

                string driverName = data.driverName;
                if (string.IsNullOrWhiteSpace(driverName))
                    driverName = "<Не указано>";
                string driverPhoneNumber = data.driverPhoneNumber;

                var state = _storage.GetOperationState("CREATED");

                temp = data.priority;
                var priority = (short)1;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!short.TryParse(temp, out priority))
                        return $"Wrong priority {temp}";
                }

                string partnerManagerName = data.partnerManagerName;
                string partnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
                string partnerManagerEmail = data.partnerManagerEmail;
                string note = data.note;

                var user = _rightsChecker.GetUserName(User);
                var partnerId = _rightsChecker.GetPartnerIdByUser(user);
                var partner = _storage.GetPartner(partnerId);
                var manager = _storage.GetPartnerManagers(partner)[0];

                var operation = new Operation(-1, null, operationType, startDate, finishDate,
                    vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
                    driverName, driverPhoneNumber,
                    state,
                    priority,
                    partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                    manager)
                {
                    Note = note
                };
                operation = _storage.AddOperation(operation, _rightsChecker.GetUserName(User));

                var text = OperationToJson(operation);
                Response.StatusCode = 201;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPost("EditOperation")]
        async public Task<string> EditOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var editOperation = _rightsChecker.CheckRight("EDIT_OPERATION", User);
                if (_useAuthentication && (!useYard || !editOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.id;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty";
                if (!long.TryParse(temp, out long id))
                    return $"Wrong operation id {temp}!";
                var operation = _storage.GetOperation(id);

                var operationType = operation.Type;
                temp = data.operationTypeCode;
                if (!string.IsNullOrWhiteSpace(temp))
                    operationType = _storage.GetOperationType(temp);
                else
                    return $"Operation type is empty!";

                var vehicleKind = operation.VehicleKind;
                temp = data.vehicleKindCode;
                if (!string.IsNullOrWhiteSpace(temp))
                    vehicleKind = _storage.GetVehicleKind(temp);
                else
                    return $"Vehicle kind is empty!";

                var planWorkStartDate = operation.PlanWorkStartDate;
                var planWorkFinishDate = operation.PlanWorkFinishDate;
                dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();
                temp = data.startDate;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out planWorkStartDate))
                        return $"Wrong start date {temp}!";
                    planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, GetDuration2(operation.Type, vehicleKind, actions)));
                }
                
                temp = data.finishDate;                
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > planWorkFinishDate)
                        planWorkFinishDate = tempFinishDate;
                }

                operation.VehicleBrand = data.vehicleBrand;
                operation.VehicleModel = data.vehicleModel;
                operation.VehicleNumber = data.vehicleNumber;
                operation.TrailerNumber = data.trailerNumber;
                operation.DriverName = data.driverName;
                operation.DriverPhoneNumber = data.driverPhoneNumber;

                var gate = operation.Gate;
                temp = data.gateCode;                
                if (!string.IsNullOrWhiteSpace(temp))
                    gate = _storage.GetGate(temp);

                var priority = operation.Priority;
                temp = data.priority;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!short.TryParse(temp, out priority))
                        return $"Wrong priority {temp}";
                    operation.Priority = priority;
                }
                else
                    operation.Priority = 1;

                operation.PartnerManagerName = data.partnerManagerName;
                operation.PartnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
                operation.PartnerManagerEmail = data.partnerManagerEmail;
                operation.Note = data.note;
                temp = data.partnerId;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Partner id is empty";
                                
                var manager = operation.Manager;
                temp = data.partnerId;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    var partner = _storage.GetPartner(temp);
                    var managers = _storage.GetPartnerManagers(partner);
                    if (managers.Length <= 0)
                        return "Partner has not managers!";
                    operation.Manager = managers[0];
                }
                else
                    return "Partner id is empty!";

                // Признак того, что изменились ключевые параметры, влияющие на выбор свободного окна и ворот
                var changeGate = operation.Type.Code != operation.Type.Code 
                    || operation.VehicleKind.Code != vehicleKind.Code
                    || operation.PlanWorkStartDate != planWorkStartDate
                    || operation.PlanWorkFinishDate != planWorkFinishDate;

                operation.Type = operationType;
                operation.VehicleKind = vehicleKind;
                operation.PlanWorkStartDate = planWorkStartDate;
                operation.PlanWorkFinishDate = planWorkFinishDate;
                var userName = _rightsChecker.GetUserName(User);
                var employee = _storage.GetEmployeeByAdLogin(userName);
                _storage.EditOperation(operation, userName);
                if (operation.Gate.Code != gate.Code)
                {
                    try
                    {
                        _storage.SetOperationGate(operation, gate, userName);
                    }
                    catch
                    {
                        _storage.SetOperationGate(operation, null, userName);
                    }
                    _storage.SetResponsibleOfficer(operation, employee);
                }
                else if (changeGate) 
                {
                    _storage.SetOperationGate(operation, null, userName);
                    _storage.SetResponsibleOfficer(operation, employee);
                }

                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPost("EditOperationByPartner")]
        async public Task<string> EditOperationByPartner()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var editOperation = _rightsChecker.CheckRight("EDIT_OPERATION_BY_PARTNER", User);
                if (_useAuthentication && (!useYard || !editOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.id;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty";
                if (!long.TryParse(temp, out long id))
                    return $"Wrong operation id {temp}!";
                var operation = _storage.GetOperation(id);

                var operationType = operation.Type;
                temp = data.operationTypeCode;
                if (!string.IsNullOrWhiteSpace(temp))
                    operationType = _storage.GetOperationType(temp);
                else
                    return $"Operation type is empty!";

                var vehicleKind = operation.VehicleKind;
                temp = data.vehicleKindCode;
                if (!string.IsNullOrWhiteSpace(temp))
                    vehicleKind = _storage.GetVehicleKind(temp);
                else
                    return $"Vehicle kind is empty!";

                var planWorkStartDate = operation.StartDate;
                var planWorkFinishDate = operation.FinishDate;
                dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();
                temp = data.startDate;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out planWorkStartDate))
                        return $"Wrong start date {temp}!";
                    planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, GetDuration2(operation.Type, vehicleKind, actions)));
                }

                temp = data.finishDate;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > planWorkFinishDate)
                        planWorkFinishDate = tempFinishDate;
                }

                operation.VehicleBrand = data.vehicleBrand;
                operation.VehicleModel = data.vehicleModel;
                operation.VehicleNumber = data.vehicleNumber;
                operation.TrailerNumber = data.trailerNumber;

                operation.DriverName = data.driverName;
                operation.DriverPhoneNumber = data.driverPhoneNumber;

                var priority = operation.Priority;
                temp = data.priority;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!short.TryParse(temp, out priority))
                        return $"Wrong priority {temp}";
                    operation.Priority = priority;
                }
                else
                    operation.Priority = 1;

                operation.PartnerManagerName = data.partnerManagerName;
                operation.PartnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
                operation.PartnerManagerEmail = data.partnerManagerEmail;
                operation.Note = data.note;

                var user = _rightsChecker.GetUserName(User);
                var partnerId = _rightsChecker.GetPartnerIdByUser(user);
                var partner = _storage.GetPartner(partnerId);
                var managers = _storage.GetPartnerManagers(partner);
                if (managers.Length <= 0)
                    return "Partner has not managers!";
                operation.Manager = managers[0];

                operation.Type = operationType;
                operation.VehicleKind = vehicleKind;
                operation.PlanWorkStartDate = planWorkStartDate;
                operation.PlanWorkFinishDate = planWorkFinishDate;
                _storage.EditOperation(operation, user);
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("SetOperationNote")]
        async public Task<string> SetOperationNote()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var startOperationWork = _rightsChecker.CheckRight("EDIT_OPERATION_NOTE", User);
                if (_useAuthentication && (!useYard || !startOperationWork))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.id;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty";

                if (!long.TryParse(temp, out long id))
                    return $"Wrong operation id {temp}";

                var operation = _storage.GetOperation(id);
                operation.Note = data.note;
                
                _storage.SetOperationNote(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("SetOperationGate")]
        async public Task<string> SetOperationGate()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var setOperationGate = _rightsChecker.CheckRight("SET_OPERATION_GATE", User);
                if (_useAuthentication && (!useYard || !setOperationGate))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                string stringGateCode = data.gateCode;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                Gate? gate = null;
                if (!string.IsNullOrWhiteSpace(stringGateCode))
                    try
                    {
                        gate = _storage.GetGate(stringGateCode.Trim().ToUpper());
                    }
                    catch
                    {
                        gate = null;
                    }

                var operation = _storage.GetOperation(id);
                var userName = _rightsChecker.GetUserName(User);

                // Эту строку (по плиску сотрудника по пользователю AD) перенёс сюда, что бы не найся сотрудника (ответственного)
                // неьзя было присвоить ворота
                var employee = _storage.GetEmployeeByAdLogin(userName);
                _storage.SetOperationGate(operation, gate, userName);                
                _storage.SetResponsibleOfficer(operation, employee);
                
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("CancelOperationGate")]
        async public Task<string> CancelOperationGate()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var cancelOperationGate = _rightsChecker.CheckRight("CANCEL_OPERATION_GATE", User);
                if (_useAuthentication && (!useYard || !cancelOperationGate))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.CancelOperationGate(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("StartOperation")]
        async public Task<string> StartOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var startOperation = _rightsChecker.CheckRight("START_OPERATION", User);
                if (_useAuthentication && (!useYard || !startOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.StartOperation(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }        
        [HttpPatch("StartOperationWork")]
        async public Task<string> StartOperationWork()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var startOperationWork = _rightsChecker.CheckRight("START_OPERATION_WORK", User);
                if (_useAuthentication && (!useYard || !startOperationWork))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.StartOperationWork(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("FinishOperationWork")]
        async public Task<string> FinishOperationWork()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var startOperationWork = _rightsChecker.CheckRight("FINISH_OPERATION_WORK", User);
                if (_useAuthentication && (!useYard || !startOperationWork))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.FinishOperationWork(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("FinishOperation")]
        async public Task<string> FinishOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var startOperation = _rightsChecker.CheckRight("FINISH_OPERATION", User);
                if (_useAuthentication && (!useYard || !startOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.FinishOperation(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("CancelOperation")]
        async public Task<string> CancelOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var cancelOperation = _rightsChecker.CheckRight("CANCEL_OPERATION", User);
                if (_useAuthentication && (!useYard || !cancelOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.CancelOperation(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpDelete("DeleteOperation")]
        async public Task<string> DeleteOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var deleteOperation = _rightsChecker.CheckRight("DELETE_OPERATION", User);
                if (_useAuthentication && (!useYard || !deleteOperation))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.DeleteOperation(operation, _rightsChecker.GetUserName(User));
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion

        #region Warehouse actions
        private static dynamic WarehouseActionToDynamic(WarehouseAction action)
        {
            dynamic result = new ExpandoObject();
            result.kind = action.Kind == ActionKind.Outgo ? "O" : "I";
            result.id = action.Id;
            result.partnerActionId = action.PartnerActionId;
            result.partnerId = action.Partner.Id;
            result.partnerName = action.Partner.Name;
            result.planActionDate = action.PlanActionDate;

            return result;
        }
        private static dynamic[] WarehouseActionsToDynamic(WarehouseAction[] actions) 
        {
            var result = new List<dynamic>();
            foreach (var action in actions)
                result.Add(WarehouseActionToDynamic(action));
            return result.ToArray();
        }
        [HttpGet("WarehouseActions")]
        public string GetWarehouseActions()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWarehouseActions = _rightsChecker.CheckRight("GET_WAREHOUSE_ACTIONS", User);
                if (_useAuthentication && (!useYard || !getWarehouseActions))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }                                
                
                string temp = Request.Query["kind"];
                ActionKind actionKind;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim().ToUpper();
                    if (temp == "O")
                        actionKind = ActionKind.Outgo;
                    else
                        actionKind = ActionKind.Income;
                }
                else
                    return "Action kind id is empty!";
                temp = Request.Query["pattern"];
                var pattern = temp;

                temp = Request.Query["partner_id"];
                Partner? partner = null;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim().ToUpper();
                    if (temp == "AUTO")
                        temp = _rightsChecker.GetPartnerIdByUser(_rightsChecker.GetUserName(User));
                    partner = _storage.GetPartner(temp);
                }

                var actions = _storage.FindWarehouseActions(actionKind, partner, pattern);
                var objects = WarehouseActionsToDynamic(actions);

                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("SelectedWarehouseActions")]
        public string GetSelectedWarehouseActions()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWarehouseActions = _rightsChecker.CheckRight("GET_SELECTED_WAREHOUSE_ACTIONS", User);
                if (_useAuthentication && (!useYard || !getWarehouseActions))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["operation_id"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty";
                if (!long.TryParse(temp, out long id))
                    return $"Wrong operation id {temp}";

                var actions = _storage.GetOperation(id).GetActions();
                var objects = WarehouseActionsToDynamic(actions);

                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("AddOperationWarehouseAction")]
        async public Task<string> AddOperationWarehouseAction()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageWarehouseActions = _rightsChecker.CheckRight("MANAGE_WAREHOUSE_ACTIONS", User);
                if (_useAuthentication && (!useYard || !manageWarehouseActions))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationId = data.operationId;
                string stringActionKind = data.actionKind;
                string stringActionId = data.actionId;

                if (string.IsNullOrWhiteSpace(stringOperationId))
                    return "Operation id is empty!";
                if (stringActionKind != "I" && stringActionKind != "O")
                    return $"Wrong action kind {stringActionKind}!";
                if (string.IsNullOrWhiteSpace(stringActionId))
                    return "Action id is empty!";

                if (!long.TryParse(stringOperationId, out long operationId))
                    return $"Wrong operation id {stringOperationId}!";
                
                var operation = _storage.GetOperation(operationId);
                var action = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
                _storage.AddAction(operation, action);

                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("RemoveOperationWarehouseAction")]
        async public Task<string> RemoveOperationWarehouseAction()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageWarehouseActions = _rightsChecker.CheckRight("MANAGE_WAREHOUSE_ACTIONS", User);
                if (_useAuthentication && (!useYard || !manageWarehouseActions))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationId = data.operationId;
                string stringActionKind = data.actionKind;
                string stringActionId = data.actionId;


                if (string.IsNullOrWhiteSpace(stringOperationId))
                    return "Operation id is empty!";
                if (string.IsNullOrWhiteSpace(stringActionKind))
                    return "Action kind is empty!";
                if (string.IsNullOrWhiteSpace(stringActionId))
                    return "Action id is empty!";

                if (!long.TryParse(stringOperationId, out long operationId))
                    return $"Wrong operation id {stringOperationId}!";

                var operation = _storage.GetOperation(operationId);

                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("RemoveOperationWarehouseActions")]
        async public Task<string> RemoveOperationWarehouseActions()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageWarehouseActions = _rightsChecker.CheckRight("MANAGE_WAREHOUSE_ACTIONS", User);
                if (_useAuthentication && (!useYard || !manageWarehouseActions))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationId = data.operationId;

                if (string.IsNullOrWhiteSpace(stringOperationId))
                    return "Operation id is empty!";
                
                if (!long.TryParse(stringOperationId, out long operationId))
                    return $"Wrong operation id {stringOperationId}!";

                var operation = _storage.GetOperation(operationId);
                _storage.RemoveActions(operation);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion

        #region Operation workers
        private static dynamic EmployeeToDynamic(Employee employee, bool isBusy = false) 
        {
            dynamic result = new ExpandoObject();
            result.id = employee.Id;
            result.surname = employee.Surname;
            result.firstName = employee.FirstName;
            result.patronymic = employee.Patronymic;
            result.shortName = employee.ShortName;
            result.fullName = employee.FullName;
            result.lowerFullName = employee.FullName.ToLower();
            result.positionId = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ?
                employee.MajorPosition.Id : 
                -1;
            result.positionCode = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ? 
                employee.MajorPosition.Code : 
                string.Empty;
            result.positionName = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ? 
                employee.MajorPosition.Name : 
                string.Empty;
            result.positionShortName = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ?
                employee.MajorPosition.ShortName :
                string.Empty;
            result.isBusy = isBusy;
            return result;
        }
        private static dynamic[] EmployeesToDynamic(Employee[] employees)
        {
            var result = new List<dynamic>();
            foreach (var employee in employees)
                result.Add(EmployeeToDynamic(employee));
            return result.ToArray();
        }
        private static dynamic OperationWorkerToDynamic(Operation.OperationWorker operationWorker)
        {
            dynamic result = new ExpandoObject();            
            result.id = operationWorker.Worker.Id;
            result.surname = operationWorker.Worker.Surname;
            result.firstName = operationWorker.Worker.FirstName;
            result.patronymic = operationWorker.Worker.Patronymic;
            result.shortName = operationWorker.Worker.ShortName;
            result.fullName = operationWorker.Worker.FullName;
            result.positionId = operationWorker.Position.Id;
            result.positionCode = operationWorker.Position.Code;
            result.positionName = operationWorker.Position.Name;
            result.positionShortName = operationWorker.Position.ShortName;
            return result;
        }
        private static dynamic[] OperationWorkersToDynamic(Operation.OperationWorker[] operationWorkers)
        {
            var result = new List<dynamic>();
            foreach (var operationWorker in operationWorkers)
                result.Add(YardController.OperationWorkerToDynamic(operationWorker));
            return result.ToArray();
        }        
        [HttpGet("PositionWorkers")]
        public string GetPositionWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWorkers = _rightsChecker.CheckRight("GET_WORKERS", User);
                if (_useAuthentication && (!useYard || !getWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringNamePattern = Request.Query["name_pattern"];
                string stringPositionCode = Request.Query["position_code"];
                if (string.IsNullOrWhiteSpace(stringPositionCode))
                    return "Position code is empty";

                var position = _storage.GetPosition(stringPositionCode);
                var workers = _storage.FindEmployees(position, stringNamePattern);
                var objects = EmployeesToDynamic(workers);
                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("LinearWorkers")]
        public string GetLinearWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWorkers = _rightsChecker.CheckRight("GET_LINEAR_WORKERS", User);
                if (_useAuthentication && (!useYard || !getWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var workers = _storage.GetLinearEmployees();

                var objects = new List<dynamic>();
                foreach (var worker in workers)
                    objects.Add(EmployeeToDynamic(worker, _storage.IsWorkerBusy(worker)));

                var text = JsonSerializer.Serialize(objects.ToArray());
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("LinearFreeWorkers")]
        public string GetLinearFreeWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWorkers = _rightsChecker.CheckRight("GET_LINEAR_FREE_WORKERS", User);
                if (_useAuthentication && (!useYard || !getWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var workers = _storage.GetLinearEmployees();
                var temp = new List<Employee>();
                foreach (var worker in workers)
                    if (!_storage.IsWorkerBusy(worker))
                        temp.Add(worker);
                var objects = EmployeesToDynamic(temp.ToArray());
                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("AllWorkers")]
        public string GetAllWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWorkers = _rightsChecker.CheckRight("GET_ALL_WORKERS", User);
                if (_useAuthentication && (!useYard || !getWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var workers = _storage.GetEmployees();
                
                var objects = new List<dynamic>();
                foreach (var worker in workers)
                    objects.Add(EmployeeToDynamic(worker, _storage.IsWorkerBusy(worker)));

                var text = JsonSerializer.Serialize(objects.ToArray());
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("OperationWorkers")]
        public string GetOperationWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWorkers = _rightsChecker.CheckRight("GET_OPERATION_WORKERS", User);
                if (_useAuthentication && (!useYard || !getWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["operation_id"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty";
                if (!long.TryParse(temp, out long id))
                    return $"Wrong operation id {temp}";

                var operation = _storage.GetOperation(id);
                var operationWorkers = _storage.GetOperationWorkers(operation);
                var objects = OperationWorkersToDynamic(operationWorkers);
                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("AddOperationWorker")]
        async public Task<string> AddOperationWorker()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", User);
                if (_useAuthentication && (!useYard || !manageOperationWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationId = data.operationId;
                string stringWorkerId = data.workerId;
                string stringPositionCode = data.positionCode;

                if (string.IsNullOrWhiteSpace(stringOperationId))
                    return "Operation id is empty!";
                if (string.IsNullOrWhiteSpace(stringWorkerId))
                    return "Worker id is empty!";
                if (string.IsNullOrWhiteSpace(stringPositionCode))
                    return "Position code is empty!";

                if (!long.TryParse(stringOperationId, out long operationId))
                    return $"Wrong operation id {stringOperationId}!";
                if (!int.TryParse(stringWorkerId, out int workerId))
                    return $"Wrong worker id {stringWorkerId}!";

                var operation = _storage.GetOperation(operationId);
                var worker = _storage.GetEmployee(workerId);
                var position = _storage.GetPosition(stringPositionCode);

                _storage.AddOperationWorker(operation, worker, position, 0.0, MeasureUnit.Null);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("RemoveOperationWorker")]        
        async public Task<string> RemoveOperationWorker()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", User);
                if (_useAuthentication && (!useYard || !manageOperationWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringOperationId = data.operationId;
                string stringWorkerId = data.workerId;
                string stringPositionCode = data.positionCode;

                if (string.IsNullOrWhiteSpace(stringOperationId))
                    return "Operation id is empty!";
                if (string.IsNullOrWhiteSpace(stringWorkerId))
                    return "Worker id is empty!";
                if (string.IsNullOrWhiteSpace(stringPositionCode))
                    return "Position code is empty!";

                if (!long.TryParse(stringOperationId, out long operationId))
                    return $"Wrong operation id {stringOperationId}!";
                if (!int.TryParse(stringWorkerId, out int workerId))
                    return $"Wrong worker id {stringWorkerId}!";

                var operation = _storage.GetOperation(operationId);
                var worker = _storage.GetEmployee(workerId);
                var position = _storage.GetPosition(stringPositionCode);

                _storage.RemoveOperationWorker(operation, worker, position);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("AllocateOperationWorkers")]
        async public Task<string> AllocateOperationWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", User);
                if (_useAuthentication && (!useYard || !manageOperationWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";

                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                _storage.AllocateOperationWorkers(operation);
                var text = OperationToJson(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("BossControlWorks")]
        public string GetBossControlWorks()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageOperationWorkers = _rightsChecker.CheckRight("GET_BOSSCONTROL_WORKS", User);
                if (_useAuthentication && (!useYard || !manageOperationWorkers))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["operation_id"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "Operation id is empty!";

                if (!long.TryParse(temp, out long operationId))
                    return $"Wrong operation id {temp}!";

                var operation = _storage.GetOperation(operationId);
                var siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
                var works = _storage.GetBossControlWorks(operation.FactWorkStartDate, operation.FactWorkFinishDate, siteCode);

                var text = JsonSerializer.Serialize(works);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion

        #region Reporting
        private static dynamic LaborReportElementToDynamic(LaborReport.Element element)
        {
            dynamic result = new ExpandoObject();
            result.partnerId = element.Partner.Id;
            result.partnerName = element.Partner.Name;
            result.positionId = element.Position.Id;
            result.positionName = element.Position.Name;

            result.bcWorkersCount = element.BcWorkersCount;
            result.bcLaborCost = element.BcLaborCost;

            result.ysPlanLaborCost = element.YSPlanLaborCost;
            result.ysFactLaborCost = element.YSFactLaborCost;

            result.yaWorkersCount = element.YAWorkersCount;
            result.yaPlanLaborCost = element.YAPlanLaborCost;
            result.yaFactLaborCost = element.YAFactLaborCost;

            return result;
        }
        private static dynamic[] LaborReportElementsToDynamic(LaborReport.Element[] elements)
        {
            var objects = new List<dynamic>();
            foreach (var element in elements)
                objects.Add(YardController.LaborReportElementToDynamic(element));
            return objects.ToArray();
        }
        [HttpGet("LaborReport")]
        public string GetLaborReport()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getData = _rightsChecker.CheckRight("GET_LABOR_REPORT", User);
                if (_useAuthentication && (!useYard || !getData))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringFromDate = Request.Query["from_date"];
                string stringToDate = Request.Query["to_date"];

                var fromDate = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(stringFromDate))
                {
                    stringFromDate = stringFromDate.Trim();
                    if (!DateTime.TryParse(stringFromDate, out fromDate))
                        return $"Wrong from date {stringFromDate}";
                }
                var toDate = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(stringToDate))
                {
                    stringToDate = stringToDate.Trim();
                    if (!DateTime.TryParse(stringToDate, out toDate))
                        return $"Wrong to date {stringToDate}";
                }
                toDate = toDate.AddDays(1);

                var report = new LaborReport(fromDate, toDate);
                report.Create(_storage);
                var elements = report.Elements;
                var result = LaborReportElementsToDynamic(elements);

                var text = JsonSerializer.Serialize(result);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        private static dynamic ShiftLaborReportElementToDynamic(ShiftLaborReport.Element element)
        {
            dynamic result = new ExpandoObject();
            result.positionId = element.Position.Id;
            result.positionName = element.Position.Name;

            result.bcLaborCost = element.BcLaborCost;
            result.bcStandardLaborCost = element.BcStandardLaborCost;
            result.bcEfficiency = element.BcEfficiency;

            result.yardLaborCost = element.YLaborCost;
            result.yardG01LaborCost = element.YG01LaborCost;
            result.yardG02LaborCost = element.YG02LaborCost;
            result.yardG03LaborCost = element.YG03LaborCost;
            result.yardG04LaborCost = element.YG04LaborCost;
            result.yardG05LaborCost = element.YG05LaborCost;
            result.yardG06LaborCost = element.YG06LaborCost;
            result.yardG07LaborCost = element.YG07LaborCost;
            result.yardG08LaborCost = element.YG08LaborCost;
            result.yardG09LaborCost = element.YG09LaborCost;
            result.yardG10LaborCost = element.YG10LaborCost;
            result.yardG11LaborCost = element.YG11LaborCost;
            result.yardG12LaborCost = element.YG12LaborCost;
            result.yardG13LaborCost = element.YG13LaborCost;
            result.yardG14LaborCost = element.YG14LaborCost;
            result.yardG15LaborCost = element.YG15LaborCost;
            result.yardG16LaborCost = element.YG16LaborCost;

            return result;
        }
        private static dynamic[] ShiftLaborReportElementsToDynamic(ShiftLaborReport.Element[] elements)
        {
            var objects = new List<dynamic>();
            foreach (var element in elements)
                objects.Add(YardController.ShiftLaborReportElementToDynamic(element));
            return objects.ToArray();
        }
        [HttpGet("ShiftLaborReport2")]
        public string GetShiftLaborReport2()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getData = _rightsChecker.CheckRight("GET_SHIFT_LABOR_REPORT", User);
                if (_useAuthentication && (!useYard || !getData))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringDate = Request.Query["date"];
                var date = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(stringDate))
                {
                    stringDate = stringDate.Trim();
                    if (!DateTime.TryParse(stringDate, out date))
                        return $"Wrong to date {stringDate}";
                }
                string shift = Request.Query["shift"];
                if (string.IsNullOrWhiteSpace(shift))
                    shift = "day";
                shift = shift.Trim().ToLower();
                if (shift != "day" && shift != "night")
                    return $"Unknown shift {shift}";

                var fromDate = shift == "day" ? date.Date.AddHours(9) : date.Date.AddHours(21);
                var toDate = fromDate.AddHours(12);

                var report = new ShiftLaborReport(fromDate, toDate);
                report.Create(_storage);
                var elements = report.Elements;
                var result = ShiftLaborReportElementsToDynamic(elements);

                var text = JsonSerializer.Serialize(result);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("ShiftPerfomanceReport")]
        public string GetShiftPerfomanceReport()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getData = _rightsChecker.CheckRight("GET_SHIFT_LABOR_REPORT", User);
                if (_useAuthentication && (!useYard || !getData))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string stringDate = Request.Query["date"];
                var date = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(stringDate))
                {
                    stringDate = stringDate.Trim();
                    if (!DateTime.TryParse(stringDate, out date))
                        return $"Wrong to date {stringDate}";
                }
                string shift = Request.Query["shift"];
                if (string.IsNullOrWhiteSpace(shift))
                    shift = "day";
                shift = shift.Trim().ToLower();
                if (shift != "day" && shift != "night")
                    return $"Unknown shift {shift}";

                var fromDate = shift == "day" ? date.Date.AddHours(9) : date.Date.AddHours(21);
                var toDate = fromDate.AddHours(12);

                var operations = _storage.GetYardPerfomanceData(fromDate, toDate);
                foreach (dynamic operation in operations)
                {
                    string partnerIds = operation.partnerIds;
                    var ids = partnerIds.Split(",");
                    var partnerNames = string.Empty;
                    foreach (var id in ids)
                    {
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            var partner = _storage.GetPartner(id.Trim());
                            if (partnerNames.Length > 0)
                                partnerNames = partnerNames += ", ";
                            partnerNames += partner.Name;
                        }
                    }
                    operation.partnerNames = partnerNames;

                    var siteCode = _storage.GetSiteCodeByGateCode(operation.gateCode);
                    operation.works = _storage.GetBcPerfomanceData(operation.factWorkStartDate, operation.factWorkFinishDate, siteCode);
                    operation.factLaborCost = 0.0;
                    foreach (var work in operation.works) 
                    {
                        operation.factLaborCost += work.duration;
                    }
                    operation.planLaborCost = operation.planLaborCost;
                    operation.factLaborCost = operation.factLaborCost;
                }

                var text = JsonSerializer.Serialize(operations);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("ECReport")]
        public string GetECReport()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getData = _rightsChecker.CheckRight("GET_EC_REPORT", User);
                if (_useAuthentication && (!useYard || !getData))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["from_date"];
                var fromDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim();
                    if (!DateTime.TryParse(temp, out fromDate))
                        return $"Wrong to date {temp}";
                }
                temp = Request.Query["to_date"];
                var toDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim();
                    if (!DateTime.TryParse(temp, out toDate))
                        return $"Wrong to date {temp}";
                    toDate = toDate.AddDays(1).AddTicks(-1);
                }

                temp = Request.Query["partners_ids"];
                var ids = string.IsNullOrWhiteSpace(temp) ? Array.Empty<string>() : temp.Split(",");

                var elements = _storage.GetEcData(fromDate, toDate, ids);
                var text = JsonSerializer.Serialize(elements);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        /// <summary>
        /// Первая версия отчёта 
        /// </summary>
        /// <returns>Данные отчёта</returns>
        [HttpGet("WorkersPerfomanceReport")]
        public string GetWorkersPerfomanceReport()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getData = _rightsChecker.CheckRight("GET_WORKERS_PERFOMANCE_REPORT", User);
                if (_useAuthentication && (!useYard || !getData))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["from_date"];
                var fromDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim();
                    if (!DateTime.TryParse(temp, out fromDate))
                        return $"Wrong to date {temp}";
                }
                temp = Request.Query["to_date"];
                var toDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim();
                    if (!DateTime.TryParse(temp, out toDate))
                        return $"Wrong to date {temp}";
                    toDate = toDate.AddDays(1).AddTicks(-1);
                }
                
                var operations = _storage.GetOperationsByFactWork(fromDate, toDate);
                var data = new Dictionary<Employee, IDictionary<Operation, int>>();
                foreach (var operation in operations)
                {
                    if (operation.FactWorkStartDate == DateTime.MinValue || operation.FactWorkFinishDate == DateTime.MinValue)
                        continue;

                    var siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
                    var works = _storage.GetBossControlWorks(operation.FactWorkStartDate, operation.FactWorkFinishDate, siteCode);

                    foreach (var work in works)
                    {
                        var employee = _storage.GetEmployee(work.workerId);
                        if (!data.ContainsKey(employee))
                            data.Add(employee, new Dictionary<Operation, int>());
                        var workerOperations = data[employee];
                        if (!workerOperations.ContainsKey(operation))
                            workerOperations.Add(operation, 0);
                        workerOperations[operation] += work.duration;
                    }
                }

                var elements = new List<dynamic>();
                foreach (var employee in data.Keys)
                {
                    var workerOperations = data[employee];
                    var perfomance = 0.0;
                    var totalNormDuration = 0.0;
                    foreach (var operation in workerOperations.Keys)
                    {
                        perfomance += operation.PlanDuration * operation.PlanDuration / workerOperations[operation];
                        totalNormDuration += operation.PlanDuration;
                    }
                    dynamic element = new ExpandoObject();
                    element.workerId = employee.Id;
                    element.workerFullName = employee.FullName;
                    element.workerPositionId = employee.MajorPosition.Id;
                    element.workerPositionName = employee.MajorPosition.Name;
                    element.perfomance = totalNormDuration != 0.0 ? 100.0 * perfomance / totalNormDuration : 0.0;
                    elements.Add(element);
                }

                var text = JsonSerializer.Serialize(elements.ToArray());
                Response.StatusCode = 200;
                return text; 
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        /// <summary>
        /// Вторая версия отчёта (детали смотри в комментариях в теле процедуры)
        /// </summary>
        /// <returns>Данные отчёта</returns>
        [HttpGet("WorkersPerfomanceReport2")]        
        public string GetWorkersPerfomanceReport2()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getData = _rightsChecker.CheckRight("GET_WORKERS_PERFOMANCE_REPORT", User);
                if (_useAuthentication && (!useYard || !getData))
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                string temp = Request.Query["from_date"];
                var fromDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim();
                    if (!DateTime.TryParse(temp, out fromDate))
                        return $"Wrong to date {temp}";
                }
                temp = Request.Query["to_date"];
                var toDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    temp = temp.Trim();
                    if (!DateTime.TryParse(temp, out toDate))
                        return $"Wrong to date {temp}";
                    toDate = toDate.AddDays(1).AddTicks(-1);
                }

                var operations = _storage.GetOperationsByFactWork(fromDate, toDate);
                var data = new Dictionary<Employee, IDictionary<Operation, double>>();
                var data2 = new Dictionary<Employee, IDictionary<Operation, double>>(); // Для производительности = План. продолжительность / Факт. продолжительность
                // Смотрим все операции двора за указанный период,
                // для кадлой операции определяем список работников, которые по БоссКонтролю "задели" фактический промежуток работ по операции,
                // определяем суммарное время всех работ (на данном участке) по БоссКонтролю, которые "задели" фактический промежуток работ по операции,
                // определяем производительность каждого сотрудника на данной операции как:
                // отношение плановой продолжительности работ по операции, умноженной на количество сотрудников  
                // и суммарного времени всех работ (на данном участке) по БоссКонтролю
                foreach (var operation in operations)
                {
                    if (operation.FactWorkStartDate == DateTime.MinValue || operation.FactWorkFinishDate == DateTime.MinValue)
                        continue;

                    var siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
                    var works = _storage.GetBossControlWorks(operation.FactWorkStartDate, operation.FactWorkFinishDate, siteCode);

                    var bcDuration = 0; // Длительность всех работ по БК для конкретной операции двора
                    var workers = new HashSet<Employee>(); // Работники, зарегистрированные на данном участке в течение работ по данной операции
                    foreach (var work in works)
                    {
                        bcDuration += work.duration;
                        var employee = _storage.GetEmployee(work.workerId);
                        if (!workers.Contains(employee))
                            workers.Add(employee);
                    }
                    var workersCount = workers.Count;
                    var perfomance = bcDuration > 0 ? (operation.PlanDuration * workersCount / bcDuration) : 0.0;
                    var perfomance2 = operation.PlanDuration / operation.FactDuration; // Для производительности = План. продолжительность / Факт. продолжительность
                    foreach (var worker in workers)
                    {
                        if (!data.ContainsKey(worker))
                            data.Add(worker, new Dictionary<Operation, double>());
                        var workerOperations = data[worker];
                        workerOperations.Add(operation, perfomance);

                        // Для производительности = План. продолжительность / Факт. продолжительность
                        if (!data2.ContainsKey(worker))
                            data2.Add(worker, new Dictionary<Operation, double>());
                        var workerOperations2 = data2[worker];
                        workerOperations2.Add(operation, perfomance2);
                    }
                }                

                // Вычисляем уже средневзвешенную производительность каждого работника:
                // сумма производельностей по конкретной операции, умноженных на плановое время операции, делённая на суммарное время фактичнских продолжительностей операций,
                // Производительности работника = Сумма(Производительность работника по конкретной операции * Плановая продолжительность операции) / Сумма(Фактическая продолжительность операции)
                var elements = new List<dynamic>();
                foreach (var worker in data.Keys)
                {
                    var workerOperations = data[worker];
                    var workerOperations2 = data2[worker]; // Для производительности = План. продолжительность / Факт. продолжительность
                    var perfomance = 0.0;
                    var perfomance2 = 0.0; // Для производительности = План. продолжительность / Факт. продолжительность
                    var totalFactDuration = 0.0;
                    foreach (var operation in workerOperations.Keys)
                    {
                        perfomance += operation.PlanDuration * workerOperations[operation];
                        perfomance2 += operation.PlanDuration * workerOperations2[operation]; // Для производительности = План. продолжительность / Факт. продолжительность
                        totalFactDuration += operation.FactDuration;
                    }
                    perfomance /= totalFactDuration;
                    perfomance2 /= totalFactDuration; // Для производительности = План. продолжительность / Факт. продолжительность

                    dynamic element = new ExpandoObject();
                    element.workerId = worker.Id;
                    element.workerFullName = worker.FullName;
                    element.workerPositionId = worker.MajorPosition.Id;
                    element.workerPositionName = worker.MajorPosition.Name;
                    element.perfomance = perfomance * 100.0;
                    element.perfomance2 = perfomance2 * 100.0; // Для производительности = План. продолжительность / Факт. продолжительность
                    elements.Add(element);
                }

                var text = JsonSerializer.Serialize(elements.ToArray());
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion
    }
}