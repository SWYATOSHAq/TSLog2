using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TSLog.Authorization;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
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

        private string GetUserName()
        {
            var user = string.Empty;
            if (User != null && User.Identity != null && !string.IsNullOrWhiteSpace(User.Identity.Name))
                user = User.Identity.Name.Split('\\').Last();
            return user;
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
                var timeSpan = _storage.GetTime(operationType, vehicleKind);
                var text = JsonSerializer.Serialize(timeSpan);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("BusyIntervals")]
        public string GetBusyIntervals()
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
        [HttpGet("Operation")]
        public string GetOperation()
        {
            Response.StatusCode = 500;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperation = _rightsChecker.CheckRight("GET_OPERATION", User);
            if (_useAuthentication && !useYard && !getOperation)
            {
                Response.StatusCode = 403;
                return "Forbidden";
            }

            string stringId = Request.Query["id"];
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            stringId = stringId.Trim();
            if (long.TryParse(stringId, out long id))
            {
                var operation = _storage.GetOperation(id);
                var text = JsonSerializer.Serialize(operation);
                Response.StatusCode = 200;
                return text;
            }
            else
                return $"Wrong operation id {stringId}";
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
                    stringVehicleNumber, stringDriverName);
                var text = JsonSerializer.Serialize(operations);
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

                string stringStartDate = data.startDate;
                string stringOperationTypeCode = data.operationTypeCode;
                string stringVehicleKindCode = data.vehicleKindCode;
                string stringVehicleBrand = data.vehicleBrand;
                string stringVehicleModel = data.vehicleModel;
                string stringVehicleNumber = data.vehicleNumber;
                string stringTrailerNumber = data.trailerNumber;
                string stringPartnerId = data.partnerId;
                string stringPartnerOrderId = data.orderId;
                string stringDriverName = data.driverName;
                string stringDriverPhoneNumber = data.driverPhoneNumber;
                string stringManagerName = data.managerName;
                string stringManagerPhoneNumber = data.managerPhoneNumber;
                string stringManagerEmail = data.managerEmail;
                string stringNote = data.note;                

                var operationType = _storage.GetOperationType(stringOperationTypeCode);
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKindCode);
                var startDate = DateTime.Parse(stringStartDate);
                var finishDate = startDate.Add(new TimeSpan(0, 0, _storage.GetTime(operationType, vehicleKind)));
                var priority = (short)1;
                var partnerManagerName = stringManagerName;
                var partnerManagerPhoneNumber = stringManagerPhoneNumber;
                var partnerManagerEmail = stringManagerEmail;
                var trailerNumber = stringTrailerNumber;
                var state = _storage.GetOperationState("CREATED");

                var partner = _storage.GetPartner(stringPartnerId);
                var manager = _storage.GetPartnerManagers(partner)[0];

                var operation = new Operation(-1, null, operationType, startDate, finishDate,
                    vehicleKind, stringVehicleBrand, stringVehicleModel, stringVehicleNumber, trailerNumber,
                    stringDriverName, stringDriverPhoneNumber,
                    state,
                    priority,
                    partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                    manager);

                if (!string.IsNullOrWhiteSpace(stringDriverPhoneNumber))
                    operation.DriverPhoneNumber = stringDriverPhoneNumber;
                if (!string.IsNullOrWhiteSpace(stringManagerName))
                    operation.PartnerManagerName = stringManagerName;
                if (!string.IsNullOrWhiteSpace(stringManagerPhoneNumber))
                    operation.PartnerManagerPhoneNumber = stringManagerPhoneNumber;
                if (!string.IsNullOrWhiteSpace(stringManagerEmail))
                    operation.PartnerManagerEmail = stringManagerEmail;
                if (!string.IsNullOrWhiteSpace(stringNote))
                    operation.Note = stringNote;

                var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;
                var action = _storage.GetAction(actionKind, partner, stringPartnerOrderId);
                operation.AddAction(action);

                var user = GetUserName();
                _storage.AddOperation(operation, user);
                _storage.SetOperationGate(operation, null, user);                
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
                // TODO: check user partner

                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string stringId = data.id;
                string stringStartDate = data.startDate;
                string stringGateCode = data.gateCode;
                string stringOperationTypeCode = data.operationTypeCode;
                string stringVehicleKindCode = data.vehicleKindCode;
                string stringVehicleBrand = data.vehicleBrand;
                string stringVehicleModel = data.vehicleModel;
                string stringVehicleNumber = data.vehicleNumber;
                string stringTrailerNumber = data.trailerNumber;
                string stringPartnerId = data.partnerId;
                string stringPartnerOrderId = data.orderId;
                string stringDriverName = data.driverName;
                string stringDriverPhoneNumber = data.driverPhoneNumber;
                string stringManagerName = data.managerName;
                string stringManagerPhoneNumber = data.managerPhoneNumber;
                string stringManagerEmail = data.managerEmail;
                string stringNote = data.note;
                
                if (string.IsNullOrWhiteSpace(stringId))
                    return "Operation id is empty";
                if (!long.TryParse(stringId, out long id))
                    return $"Wrong operation id {stringId}";

                var operation = _storage.GetOperation(id);
                if (operation.State.Code == "FINISHED" || operation.State.Code == "CANCELED")
                    return $"Wrong operation state {operation.State.Code}";

                var gate = _storage.GetGate(stringGateCode);
                var operationType = _storage.GetOperationType(stringOperationTypeCode);
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKindCode);

                if (operationType != Null)
                    operation.Type = operationType;
                if (vehicleKind != VehicleKind.Null)
                    operation.VehicleKind = vehicleKind;
                if (!string.IsNullOrWhiteSpace(stringVehicleBrand))
                    operation.VehicleBrand = stringVehicleBrand;
                if (!string.IsNullOrWhiteSpace(stringVehicleModel))
                    operation.VehicleModel = stringVehicleModel;
                if (!string.IsNullOrWhiteSpace(stringVehicleNumber))
                    operation.VehicleNumber = stringVehicleNumber;
                if (!string.IsNullOrWhiteSpace(stringDriverName))
                    operation.DriverName = stringDriverName;
                if (!string.IsNullOrWhiteSpace(stringDriverPhoneNumber))
                    operation.DriverPhoneNumber = stringDriverPhoneNumber;
                if (!string.IsNullOrWhiteSpace(stringManagerName))
                    operation.PartnerManagerName = stringManagerName;
                if (!string.IsNullOrWhiteSpace(stringManagerPhoneNumber))
                    operation.PartnerManagerPhoneNumber = stringManagerPhoneNumber;
                if (string.IsNullOrWhiteSpace(stringManagerEmail))
                    operation.PartnerManagerEmail = stringManagerEmail;
                if (!string.IsNullOrWhiteSpace(stringNote))
                    operation.Note = stringNote;

                if (!string.IsNullOrWhiteSpace(stringPartnerOrderId))
                {
                    var partner = Partner.Null;
                    var actions = operation.GetActions();
                    foreach (var action in actions)
                    {
                        partner = action.Partner;
                        operation.RemoveAction(action);
                    }
                    var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;
                    operation.AddAction(_storage.GetAction(actionKind, partner, stringPartnerOrderId));
                }
                var user = GetUserName();
                _storage.EditOperation(operation, user);
                if (operation.Gate != gate)
                    try
                    {
                        _storage.SetOperationGate(operation, gate, user);
                    }
                    catch
                    {
                        _storage.SetOperationGate(operation, null, user);
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
                _storage.SetOperationGate(operation, gate, GetUserName());
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
                _storage.CancelOperationGate(operation, GetUserName());
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
                _storage.StartOperation(operation, GetUserName());
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
                _storage.StartOperationWork(operation, GetUserName());
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
                _storage.FinishOperationWork(operation, GetUserName());
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
                _storage.FinishOperation(operation, GetUserName());
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
                _storage.CancelOperation(operation, GetUserName());
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
                _storage.DeleteOperation(operation, GetUserName());
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
    }
}