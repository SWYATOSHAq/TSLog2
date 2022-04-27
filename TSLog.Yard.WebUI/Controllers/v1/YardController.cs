using Microsoft.AspNetCore.Mvc;
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
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class YardController : ControllerBase
    {
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
        private static dynamic OperationToDynamic(Operation operation)
        {
            dynamic result = new ExpandoObject();
            result.id = operation.Id;
            result.stateCode = operation.State.Code;
            result.stateDisplayName = operation.State.DisplayName;
            result.typeCode = operation.Type.Code;
            result.typeDisplayName = operation.Type.DisplayName;
            result.directionName = operation.Type.DirectionName;
            result.vehicleKindCode = operation.VehicleKind.Code;
            result.startDate = operation.StartDate;
            result.planWorkStartDate = operation.PlanWorkStartDate;
            result.factWorkStartDate = operation.FactWorkStartDate;
            result.planWorkFinishDate = operation.PlanWorkFinishDate;
            result.factWorkFinishDate = operation.FactWorkFinishDate;
            result.finishDate = operation.FinishDate;
            result.planDuration = (operation.PlanWorkFinishDate - operation.PlanWorkStartDate).TotalSeconds;
            result.factDuration = (operation.FactWorkFinishDate - operation.FactWorkStartDate).TotalSeconds;
            result.planLaborCost = operation.PlanLaborCost;
            result.factLaborCost = operation.FactLaborCost;
            result.gateCode = operation.Gate.Code;
            result.gateShortName = operation.Gate.ShortName;
            result.vehicleBrand = operation.VehicleBrand;
            result.vehicleModel = operation.VehicleModel;
            result.vehicleNumber = operation.VehicleNumber;
            result.trailerNumber = operation.TrailerNumber;
            result.driverName = operation.DriverName;
            result.driverPhoneNumber = operation.DriverPhoneNumber;
            result.partnerManagerName = operation.PartnerManagerName;
            result.partnerManagerPhoneNumber = operation.PartnerManagerPhoneNumber;
            result.partnerManagerEmail = operation.PartnerManagerEmail;
            result.responsibleOfficer = operation.ResponsibleOfficer != null && operation.ResponsibleOfficer != Employee.Null && operation.ResponsibleOfficer != Employee.Empty
                ? operation.ResponsibleOfficer.FullName : string.Empty;
            result.note = operation.Note;

            result.partnerId = operation.Partners.Length > 0 ? operation.Partners[0].Id : string.Empty;
            result.partnerActionId = operation.GetActions().Length > 0 ? operation.GetActions()[0].PartnerActionId : string.Empty;

            result.partnersNames = Partner.Serialize(operation.GetPartners());

            result.displayColor = operation.DisplayColor;

            return result;
        }
        private static dynamic[] OperationsToDynamic(Operation[] operations)
        {
            var result = new List<dynamic>();
            foreach (Operation operation in operations)
                result.Add(YardController.OperationToDynamic(operation));
            return result.ToArray();
        }
        private static string OperationToJson(Operation operation)
        {
            var temp = YardController.OperationToDynamic(operation);
            return JsonSerializer.Serialize(temp);
        }
        private static string OperationsToJson(Operation[] operations)
        {
            var temp = OperationsToDynamic(operations);
            return JsonSerializer.Serialize(temp);
        }
        private HashSet<string> GetPartnersIds()
        {
            var userName = _rightsChecker.GetUserName(User);
            return _rightsChecker.GetPartnersIdsByUser(userName);
        }
        private bool CheckPartner(string partnerId)
        {
            var userName = _rightsChecker.GetUserName(User);
            var partnerIds = _rightsChecker.GetPartnersIdsByUser(userName);
            return partnerIds.Contains(partnerId);
        }
        [HttpGet("OperationDuration")]
        public string GetOperationDuration()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
                if (_useAuthentication && !useYard && !getOperationDuration) 
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
                var timeSpan = _storage.GetDuration(operationType, vehicleKind);
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
                if (_useAuthentication && !useYard && !getOperationDuration)
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
                var timeSpan = _storage.GetLaborCost(operationType, vehicleKind);
                var text = JsonSerializer.Serialize(timeSpan);
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
                var getBusyIntervals = _rightsChecker.CheckRight("GET_BUSY_DURATION", User);
                if (_useAuthentication && !useYard && !getBusyIntervals)
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
                if (_useAuthentication && !useYard && !getPartnermanager)
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
                if (_useAuthentication && !useYard && !getPartnermanager)
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
        [HttpGet("Operation")]
        public string GetOperation()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperation = _rightsChecker.CheckRight("GET_OPERATION", User);
                if (_useAuthentication && !useYard && !getOperation)
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

                    var text = OperationToJson(operation);
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
                if (_useAuthentication && !useYard && !getOperations)
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
                string stringVehicleNumber = Request.Query["vehicle_number"];
                string stringDriverName = Request.Query["driver_name"];
                string stringPartnerActionId = Request.Query["partner_action_id"];

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

                var span = 1;
                var operations = _storage.GetOperations(fromDate.Date, toDate.AddDays(span).AddTicks(-1), Array.Empty<Partner>());
                operations = Operation.Filter(operations, partner, operationType, vehicleKind, gate, state,
                    stringVehicleNumber, stringDriverName, stringPartnerActionId);

                var list = new List<Operation>();
                var partnersIds = GetPartnersIds();
                foreach (var operation in operations)
                    if (operation.Partners.Length > 0 && partnersIds.Contains(operation.Partners[0].Id) || partnersIds.Count <= 0)
                        list.Add(operation);

                var text = JsonSerializer.Serialize(list.ToArray());
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
                if (_useAuthentication && !useYard && !getOperations)
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var operations = _storage.GetActiveOperations();
                var list = OperationsToDynamic(operations);
                var text = JsonSerializer.Serialize(list);
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
                if (_useAuthentication && !useYard && !getOperations)
                {
                    Response.StatusCode = 403;
                    return "Forbidden";
                }

                var operations = _storage.GetGuardOperations();
                var list = OperationsToDynamic(operations);
                var text = JsonSerializer.Serialize(list);
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
                if (_useAuthentication && !useYard && !createOperation)
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
                var finishDate = startDate.Add(new TimeSpan(0, 0, _storage.GetDuration(operationType, vehicleKind)));
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
                string partnerActionId = data.partnerActionId;
                if (string.IsNullOrWhiteSpace(partnerActionId))
                    return "Partner action id is empty!";

                var partner = _storage.GetPartner(partnerId);
                var manager = _storage.GetPartnerManagers(partner)[0];
                var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;
                var action = _storage.GetAction(actionKind, partner, partnerActionId);

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
                operation.AddAction(action);

                var user = _rightsChecker.GetUserName(User);
                _storage.AddOperation(operation, user);
                // Эту строку (по плиску сотрудника по пользователю AD) перенёс сюда, что бы не найся сотрудника (ответственного)
                // неьзя было присвоить ворота
                var employee = _storage.GetEmployeeByAdLogin(user);
                _storage.SetOperationGate(operation, null, user);                
                _storage.SetResponsibleOfficer(operation, employee, user);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
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
                if (_useAuthentication && !useYard && !createOperation)
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
                var finishDate = startDate.Add(new TimeSpan(0, 0, _storage.GetDuration(operationType, vehicleKind)));
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

                string partnerActioinId = data.partnerActionId;
                if (string.IsNullOrWhiteSpace(partnerActioinId))
                    return "Partner action id is empty!";

                var user = _rightsChecker.GetUserName(User);
                var partnerId = _rightsChecker.GetPartnerIdByUser(user);
                var partner = _storage.GetPartner(partnerId);
                var manager = _storage.GetPartnerManagers(partner)[0];
                var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;
                var action = _storage.GetAction(actionKind, partner, partnerActioinId);

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
                operation.AddAction(action);
                
                _storage.AddOperation(operation, user);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
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
                if (_useAuthentication && !useYard && !editOperation)
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

                var vehicleKind = operation.VehicleKind;
                temp = data.vehicleKindCode;
                if (!string.IsNullOrWhiteSpace(temp))
                    vehicleKind = _storage.GetVehicleKind(temp);

                var planWorkStartDate = operation.PlanWorkStartDate;
                var planWorkFinishDate = operation.PlanWorkFinishDate;
                temp = data.startDate;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out planWorkStartDate))
                        return $"Wrong start date {temp}!";
                    planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, _storage.GetDuration(operation.Type, vehicleKind)));
                }
                
                temp = data.finishDate;                
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > planWorkFinishDate)
                        planWorkFinishDate = tempFinishDate;
                }

                    temp = data.vehicleBrand;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.VehicleBrand = temp;
                temp = data.vehicleModel;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.VehicleModel = temp;
                temp = data.vehicleNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.VehicleNumber = temp;
                temp = data.trailerNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.TrailerNumber = temp;

                temp = data.driverName;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.DriverName = temp;
                temp = data.driverPhoneNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.DriverPhoneNumber = temp;

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

                temp = data.partnerManagerName;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.PartnerManagerName = temp;
                temp = data.partnerManagerPhoneNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.PartnerManagerPhoneNumber = temp;
                temp = data.partnerManagerEmail;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.PartnerManagerEmail = temp;
                temp = data.note;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.Note = temp;

                var actions = operation.GetActions();
                var partner = actions.Length > 0 ? actions[0].Partner : Partner.Null;
                var manager = operation.Manager;
                temp = data.partnerId;
                string temp2 = data.partnerActionId;
                if (!string.IsNullOrWhiteSpace(temp) && !string.IsNullOrWhiteSpace(temp2))
                {
                    operation.RemoveActions();
                    var actionKind = operation.Type.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;
                    var action = _storage.GetAction(actionKind, partner, temp2);
                    operation.AddAction(action);
                    var managers = _storage.GetPartnerManagers(partner);
                    if (managers.Length <= 0)
                        return "Partner has not managers!";
                    operation.Manager = managers[0];
                }
                else if (string.IsNullOrWhiteSpace(temp) && !string.IsNullOrWhiteSpace(temp2))
                    return "Partner id is empty, but partner order id is not empty!";
                else if (!string.IsNullOrWhiteSpace(temp) && string.IsNullOrWhiteSpace(temp2))
                    return "Partner id is not empty, but partner order id is empty!";

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

                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !editOperation)
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

                var vehicleKind = operation.VehicleKind;
                temp = data.vehicleKindCode;
                if (!string.IsNullOrWhiteSpace(temp))
                    vehicleKind = _storage.GetVehicleKind(temp);

                var planWorkStartDate = operation.StartDate;
                var planWorkFinishDate = operation.FinishDate;
                temp = data.startDate;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out planWorkStartDate))
                        return $"Wrong start date {temp}!";
                    planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, _storage.GetDuration(operation.Type, vehicleKind)));
                }

                temp = data.finishDate;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
                        return $"Wrong finish date {temp}";
                    if (tempFinishDate > planWorkFinishDate)
                        planWorkFinishDate = tempFinishDate;
                }

                temp = data.vehicleBrand;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.VehicleBrand = temp;
                temp = data.vehicleModel;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.VehicleModel = temp;
                temp = data.vehicleNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.VehicleNumber = temp;
                temp = data.trailerNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.TrailerNumber = temp;

                temp = data.driverName;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.DriverName = temp;
                temp = data.driverPhoneNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.DriverPhoneNumber = temp;

                var gate = operation.Gate;
                temp = data.getaCode;
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

                temp = data.partnerManagerName;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.PartnerManagerName = temp;
                temp = data.partnerManagerPhoneNumber;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.PartnerManagerPhoneNumber = temp;
                temp = data.partnerManagerEmail;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.PartnerManagerEmail = temp;
                temp = data.note;
                if (!string.IsNullOrWhiteSpace(temp))
                    operation.Note = temp;

                var user = _rightsChecker.GetUserName(User);
                var partnerId = _rightsChecker.GetPartnerIdByUser(user);
                var partner = _storage.GetPartner(partnerId);
                temp = data.partnerActionId;
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    var actions = operation.GetActions();
                    operation.RemoveActions();
                    var actionKind = operation.Type.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;
                    var action = _storage.GetAction(actionKind, partner, temp);
                    operation.AddAction(action);                    
                }
                var managers = _storage.GetPartnerManagers(partner);
                if (managers.Length <= 0)
                    return "Partner has not managers!";
                operation.Manager = managers[0];

                operation.Type = operationType;
                operation.VehicleKind = vehicleKind;
                operation.PlanWorkStartDate = planWorkStartDate;
                operation.PlanWorkFinishDate = planWorkFinishDate;
                _storage.EditOperation(operation, user);
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !startOperationWork)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !setOperationGate)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !cancelOperationGate)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !startOperation)
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
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("AllocateOperationEmployees")]
        async public Task<string> AllocateOperationEmployees()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", User);
                if (_useAuthentication && !useYard && !manageOperationWorkers)
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
                _storage.AllocateEmployees(operation);
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !startOperationWork)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !startOperationWork)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !startOperation)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !cancelOperation)
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
                var text = JsonSerializer.Serialize(operation);
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
                if (_useAuthentication && !useYard && !deleteOperation)
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
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("GetWorkers")]
        public string GetWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getWorkers = _rightsChecker.CheckRight("GET_WORKERS", User);
                if (_useAuthentication && !useYard && !getWorkers)
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
                var text = JsonSerializer.Serialize(workers);
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
                if (_useAuthentication && !useYard && !manageOperationWorkers)
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

                _storage.AddEmployee(operation, worker, position, 0.0, MeasureUnit.Null);
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
                if (_useAuthentication && !useYard && !manageOperationWorkers)
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

                _storage.RemoveEmployee(operation, worker, position);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }


        private dynamic LaborReportElementToDynamic(LaborReport.Element element)
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
        private dynamic[] LaborReportElementsToDynamic(LaborReport.Element[] elements)
        {
            var objects = new List<dynamic>();
            foreach (var element in elements)
                objects.Add(LaborReportElementToDynamic(element));
            return objects.ToArray();
        }
        [HttpGet("LaborReport")]
        public string GetLaborReport()
        {
            try
            {
                Response.StatusCode = 500;
                var useYard = _rightsChecker.CheckRight("USE_YARD", User);
                var getOperations = _rightsChecker.CheckRight("GET_LABOR_REPORT", User);
                if (_useAuthentication && !useYard && !getOperations)
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
    }
}