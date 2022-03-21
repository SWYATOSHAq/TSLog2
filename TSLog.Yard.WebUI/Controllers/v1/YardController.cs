using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;
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
        private readonly IYardStorage _storage;
        public YardController(IYardStorage storage)
        {
            _storage = storage;
        }
        /*
         * get info about operation by id
         * get list of operations by filter (start time, finish time, doors, partner)
         * create / update operation
         * change state
         */

        [HttpGet("BusyPeriods")]
        public string GetBusyPeriods()
        {
            string stringDate = Request.Query["date"];
            string stringOperationType = Request.Query["operation"];
            string stringVehicleKind = Request.Query["vehicle_kind"];

            var date = DateTime.Parse(stringDate);
            var operationType = _storage.GetOperationType(stringOperationType);
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);

            var span = 2;
            var periods = _storage.GetAvalibleIntervals(date, date.AddDays(span), operationType, vehicleKind);
            var busyPeriods = Interval.Union(periods);

            var text = JsonSerializer.Serialize(busyPeriods);
            return text;
        }

        [HttpGet("OperationLength")]
        public string GetOperationLength()
        {
            string stringOperationType = Request.Query["operation"];            
            string stringVehicleKind = Request.Query["vehicle_kind"];

            var operationType = _storage.GetOperationType(stringOperationType);
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            var timeSpan = _storage.GetTime(operationType, vehicleKind);

            var text = JsonSerializer.Serialize(timeSpan);
            return text;
        }

        [HttpGet("OperationData")]
        public string GetOperationData()
        {
            var text = string.Empty;
            string stringId = Request.Query["operation_id"];
            if (long.TryParse(stringId, out long id))
            {
                var operation = _storage.GetOperation(id);
                text = JsonSerializer.Serialize(operation);
            }
            return text;
        }

        [HttpGet("OperationsList")]
        public string GetOperationsList()
        {
            var text = string.Empty;

            string stringFromDate = Request.Query["from_date"];
            string stringToDate = Request.Query["to_date"];
            string stringGate = Request.Query["gate"];
            string stringPartnerId = Request.Query["partner_id"];
            string stringOperationType = Request.Query["operation_type"];
            string stringVehicleKind = Request.Query["vehicle_kind"];
            string stringState = Request.Query["state"];
            string stringVehicleNumber = Request.Query["vehicle_number"];
            string stringDriverName = Request.Query["driver_name"];

            var partner = !string.IsNullOrWhiteSpace(stringPartnerId) ? _storage.GetPartner(stringPartnerId) : Partner.Null;
            var gate = !string.IsNullOrWhiteSpace(stringGate) ? _storage.GetGate(stringGate) : Gate.Null;
            var operationType = !string.IsNullOrWhiteSpace(stringOperationType) ? _storage.GetOperationType(stringOperationType) : Null;
            var vehicleKind = !string.IsNullOrWhiteSpace(stringVehicleKind) ? _storage.GetVehicleKind(stringVehicleKind) : VehicleKind.Null;
            var state = !string.IsNullOrWhiteSpace(stringState) ? _storage.GetOperationState(stringState) : Entity.Null;

            if (DateTime.TryParse(stringFromDate, out DateTime fromDate) && DateTime.TryParse(stringToDate, out DateTime toDate))
            {
                var span = 1;
                var operations = _storage.GetOperations(fromDate.Date, toDate.AddDays(span).AddTicks(-1), Array.Empty<Partner>());
                operations = Operation.Filter(operations, partner, operationType, vehicleKind, gate, state,
                    stringVehicleNumber, stringDriverName);
                text = JsonSerializer.Serialize(operations);
            }
            return text;
        }

        [HttpPost("CreateUpdateOperation")]
        public string GetInsertUpdateData()
        {
            var user = string.Empty;
            if (User != null && User.Identity != null && !string.IsNullOrWhiteSpace(User.Identity.Name))
                user = User.Identity.Name.Split('\\').Last();

            var text = "Error||[внутренная ошибка: ошибка создания/обновления записи, обратитесь к ответственному лицу]";

            string stringOperationId = Request.Form["operation_id"];
            string stringStartDate = Request.Form["start_date"];
            string stringFinishDate = Request.Form["finish_date"];
            string stringGateId = Request.Form["gate"];
            string stringOperationType = Request.Form["operation_type"];
            string stringVehicleKind = Request.Form["vehicle_kind"];            
            string stringVehicleBrand = Request.Form["vehicle_brand"];
            string stringVehicleModel = Request.Form["vehicle_model"];
            string stringVehicleNumber = Request.Form["vehicle_number"];
            string stringOrderId = Request.Form["order_id"];
            string stringDriverName = Request.Form["driver_name"];
            string stringDriverPhoneNumber = Request.Form["driver_phone"];
            string stringManagerName = Request.Form["manager_name"];
            string stringManagerPhoneNumber = Request.Form["manager_phone"];
            string stringManagerEmail = Request.Form["manager_email"];
            string stringNote = Request.Form["note"];
            string stringPartnerId = Request.Form["partner_id"];

            if (stringOperationId != null)
            {
                if (long.TryParse(stringOperationId, out long id))
                {
                    var operation = _storage.GetOperation(id);
                    if (operation.State.Code == "FINISHED" || operation.State.Code == "CANCELED")
                        text = "Error||ошибка, невозможно изменить значение заявки, которая уже завершена / отменена";
                    else
                    {
                        var gate = _storage.GetGate(stringGateId);
                        var operationType = _storage.GetOperationType(stringOperationType);                        
                        var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);

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

                        if (!string.IsNullOrWhiteSpace(stringOrderId))
                        {
                            var partner = Partner.Null;
                            var actions = operation.GetActions();
                            foreach (var action in actions)
                            {
                                partner = action.Partner;
                                operation.RemoveAction(action);
                            }

                            var actionKind = ActionKind.Income;
                            switch (operationType.Direction)
                            {
                                case TypeDirection.In:
                                    actionKind = ActionKind.Income;
                                    break;
                                case TypeDirection.Out:
                                    actionKind = ActionKind.Outgo;
                                    break;
                                default:
                                    break;
                            }
                            operation.AddAction(_storage.GetAction(actionKind, partner, stringOrderId));
                        }

                        _storage.EditOperation(operation, user);
                        if (operation.Gate != gate)
                        {
                            try
                            {
                                _storage.SetOperationGate(operation, gate, user);
                            }
                            catch (Exception)
                            {
                                _storage.SetOperationGate(operation, null, user);
                                // TODO: Say to user that gates has not been successfully updated
                            }
                        }
                        text = JsonSerializer.Serialize(operation);
                    }
                }
                else
                    text = "Error||ошибка, информация не обновлена";
            }
            else
            {
                var operationType = _storage.GetOperationType(stringOperationType);
                var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
                var startDate = DateTime.Parse(stringStartDate);
                var finishDate = startDate.Add(new TimeSpan(0, 0, _storage.GetTime(operationType, vehicleKind)));
                var priority = (short)1;
                var partnerManagerName = stringManagerName;
                var partnerManagerPhoneNumber = stringManagerPhoneNumber;
                var partnerManagerEmail = stringManagerEmail;
                var trailerNumber = string.Empty;
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

                var actionKind = ActionKind.Income;
                switch (operationType.Direction)
                {
                    case TypeDirection.In:
                        actionKind = ActionKind.Income;
                        break;
                    case TypeDirection.Out:
                        actionKind = ActionKind.Outgo;
                        break;
                    default:
                        break;
                }
                var action = _storage.GetAction(actionKind, partner, stringOrderId);
                operation.AddAction(action);

                try
                {                    
                    _storage.AddOperation(operation, user);
                    try
                    {
                        _storage.SetOperationGate(operation, null, user);
                    }
                    catch (Exception e)
                    {
                        text = "Error||ошибка, ворота не были проставлены, измените время или тип машины";
                    }
                    text = JsonSerializer.Serialize(operation);
                }
                catch (WarehouseActionNotFoundException)
                {
                    text = "Error||ошибка, номер заказа партнера не соответствует типу заказа / не действителен или отсутствует в БД";
                }
            }
            return text;
        }

        [HttpPost("ChangeOperationState")]
        public string PostChangeOperationState()
        {
            var user = string.Empty;
            if (User != null && User.Identity != null && !string.IsNullOrWhiteSpace(User.Identity.Name))
                user = User.Identity.Name.Split('\\').Last();

            var text = string.Empty;

            string stringId = Request.Form["operation_id"];
            string stringAction = Request.Form["action"];

            if (long.TryParse(stringId, out long id))
            {
                var operation = _storage.GetOperation(id);
                switch (stringAction)
                {
                    case "StartOperation":
                        _storage.StartOperation(operation, user);
                        break;
                    case "ResetOperationGate":
                        _storage.SetOperationGate(operation, null, user);
                        break;
                    case "StartOperationWork":
                        _storage.StartOperationWork(operation, user);
                        break;
                    case "FinishOperation":
                        _storage.FinishOperation(operation, user);
                        break;
                    case "FinishOperationWork":
                        _storage.FinishOperationWork(operation, user);
                        break;
                    case "CancelOperation":
                        _storage.CancelOperation(operation, user);
                        break;
                    case "DeleteOperation":
                        _storage.DeleteOperation(operation, user);
                        break;
                    case "AllocateEmployees":
                        try
                        {
                            _storage.AllocateEmployees(operation);
                        }
                        catch (Exception)
                        {
                            // TODO ошибка привязки персонала
                        }
                        break;

                    default:
                        text = "Operation is not specified";
                        break;
                }
                text = JsonSerializer.Serialize(operation);
            }
            return text;
        }

        [HttpGet("SearchWorkersByName")]
        public string GetSearchWorkersByName()
        {
            string stringSearchQuery = Request.Query["search"];
            string stringPositionCode = Request.Query["position_code"];

            var loadedPosition = _storage.GetPosition(stringPositionCode);
            var workers = _storage.FindEmployees(loadedPosition, stringSearchQuery);

            var text = JsonSerializer.Serialize(workers);
            return text;
        }

        [HttpPost("UpdateOperationWorkers")]
        public string PostUpdateOperationWorkers()
        {
            var text = string.Empty;

            string stringId = Request.Form["operation_id"];
            string stringActionType = Request.Form["action_type"];
            string stringWorkerId = Request.Form["worker_id"];
            string stringPositionCode = Request.Form["position_code"];

            if (long.TryParse(stringId, out long id) && int.TryParse(stringWorkerId, out int workerId))
            {
                var operation = _storage.GetOperation(id);
                var worker = _storage.GetEmployee(workerId);
                var position = _storage.GetPosition(stringPositionCode);

                switch (stringActionType)
                {
                    case "add":
                        _storage.AddEmployee(operation, worker, position, 0.0, MeasureUnit.Null);
                        break;
                    case "del":
                        _storage.RemoveEmployee(operation, worker, position);
                        break;
                    default:
                        text = "Action type is not specified";
                        break;
                }
                text = JsonSerializer.Serialize(operation);
            }
            return text;
        }
    }
}