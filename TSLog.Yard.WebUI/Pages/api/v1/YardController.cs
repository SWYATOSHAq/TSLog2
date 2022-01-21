using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;
using TSLog.Partners.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using static TSLog.Yard.BO.OperationType;
using OperationType = TSLog.Yard.BO.OperationType;

namespace TSLog.Yard.WebUI.Pages.api.v1
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class YardController : ControllerBase
    {
        private readonly IYardStorage yardStorage;

        public YardController(IYardStorage storage)
        {
            yardStorage = storage;
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
            // TODO test
            // default
            string Text;
            //
            string strDate = Request.Query["date"];
            var date = DateTime.Parse(strDate);


            string strOperation = Request.Query["operation"];
            var operation = yardStorage.GetOperationType(strOperation);

            string strVehicleKind = Request.Query["vehicle_kind"];
            var vehicleKind = yardStorage.GetVehicleKind(strVehicleKind);

            //
            var periods = yardStorage.GetAvalibleIntervals(date, date.AddDays(2), operation, vehicleKind);
            var busyPeriods = Interval.Union(periods);
            //
            Text = JsonSerializer.Serialize(busyPeriods);
            return Text;
        }

        [HttpGet("OperationLength")]
        public string GetOperationLength()
        {
            // TODO test
            // default
            string Text;
            //

            string strOperation = Request.Query["operation"];
            var operation = yardStorage.GetOperationType(strOperation);

            string strVehicleKind = Request.Query["vehicle_kind"];
            var vehicleKind = yardStorage.GetVehicleKind(strVehicleKind);

            //
            var timeSpan = yardStorage.GetTime(operation, vehicleKind);

            ////
            Text = JsonSerializer.Serialize(timeSpan);
            return Text;
        }

        [HttpGet("OperationData")]
        public string GetOperationData()
        {
            // TODO test
            // default
            var Text = "";
            //
            string operationID = Request.Query["operation_id"];

            if (long.TryParse(operationID, out long tmpID))
            {
                var loadedOperation = yardStorage.GetOperation(tmpID);
                Text = JsonSerializer.Serialize(loadedOperation);
            }

            return Text;
        }

        [HttpGet("OperationsList")]
        public string GetOperationsList()
        {
            var Text = "";
            //
            string operationDate = Request.Query["date"];

            string gate = Request.Query["gate"];
            string partnerID = Request.Query["partner_id"];
            string operationType = Request.Query["operation_type"];
            string vehicleKind = Request.Query["vehicle_kind"];
            string state = Request.Query["state"];

            var partner = !string.IsNullOrWhiteSpace(partnerID) ? yardStorage.GetPartner(partnerID) : Partner.Null;
            //var searchPartners = partner == Partner.Null ? Array.Empty<Partner>() : new Partner[] { partner };

            var searchGate = !string.IsNullOrWhiteSpace(gate) ? yardStorage.GetGate(gate) : Gate.Null;
            var searchOperationType = !string.IsNullOrWhiteSpace(operationType) ? yardStorage.GetOperationType(operationType) : OperationType.Null;
            var searchVehicleKind = !string.IsNullOrWhiteSpace(vehicleKind) ? yardStorage.GetVehicleKind(vehicleKind) : VehicleKind.Null;
            var searchState = !string.IsNullOrWhiteSpace(state) ? yardStorage.GetOperationState(state) : Entity.Null;

            if (DateTime.TryParse(operationDate, out DateTime tmp))
            {
                // search for period of 1 day or 2 days?
                var daysSpan = 2;
                var loadedOperations = yardStorage.GetOperations(tmp.Date, tmp.AddDays(daysSpan).AddTicks(-1), Array.Empty<Partner>());
                // additional filters
                loadedOperations = Operation.Filter(loadedOperations, partner, searchOperationType, searchVehicleKind, searchGate, searchState);
                //
                Text = JsonSerializer.Serialize(loadedOperations);
            }
            return Text;
        }

        [HttpPost("CreateUpdateOperation")]
        public string GetInsertUpdateData()
        {
            var operation_type = Request.Form["operation_type"];
            //var gate = Request.Form["gate"];
            var vehicle_kind = Request.Form["vehicle_kind"];
            string vehicle_brand = Request.Form["vehicle_brand"];
            string vehicle_model = Request.Form["vehicle_model"];
            string vehicle_number = Request.Form["vehicle_number"];
            string order_id = Request.Form["order_id"];
            string driver_name = Request.Form["driver_name"];

            string driver_phone = Request.Form["driver_phone"];
            string manager_name = Request.Form["manager_name"];
            string manager_phone = Request.Form["manager_phone"];
            string manager_email = Request.Form["manager_email"];
            string note = Request.Form["note"];

            string gate_id = Request.Form["gate"];

            //
            string operation_id = Request.Form["operation_id"];
            //
            var start_date = Request.Form["start_date"];
            var finish_date = Request.Form["finish_date"];

            string Text = "eror||[внутренная ошибка: ошибка создания/обновления записи, обратитесь к ответственному лицу]";

            if (operation_id != null)
            {
                long operationId;

                if (long.TryParse(operation_id, out operationId))
                {

                    // update 
                    var operationToUpdate = yardStorage.GetOperation(operationId);
                    if (operationToUpdate.State.Code != "CREATED" && operationToUpdate.State.Code != "CONFIRMED")
                    {
                        Text = "eror||ошибка, невозможно изменить значение заявки, которая уже в работе";
                    }
                    else
                    {
                        //set values
                        var type = yardStorage.GetOperationType(operation_type);
                        var gate = yardStorage.GetGate(gate_id);
                        var vehicleKind = yardStorage.GetVehicleKind(vehicle_kind);

                        if (type != OperationType.Null)
                            operationToUpdate.Type = type;

                        if (vehicleKind != VehicleKind.Null)
                            operationToUpdate.VehicleKind = vehicleKind;


                        if (vehicle_brand != null)
                            operationToUpdate.VehicleBrand = vehicle_brand;
                        if (vehicle_model != null)
                            operationToUpdate.VehicleModel = vehicle_model;
                        if (vehicle_number != null)
                            operationToUpdate.VehicleNumber = vehicle_number;
                        if (order_id != null)
                        {
                            var tmpPartner = Partner.Null;
                            var curActions = operationToUpdate.GetActions();
                            // remove previous
                            foreach (var curAction in curActions)
                            {
                                tmpPartner = curAction.Partner;
                                operationToUpdate.RemoveAction(curAction);
                            }
                            // add new
                            ActionKind actionKind = ActionKind.Income;

                            switch (type.Direction)
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
                            operationToUpdate.AddAction(yardStorage.GetAction(actionKind, tmpPartner, order_id));
                        }
                        if (driver_name != null)
                            operationToUpdate.DriverName = driver_name;

                        if (driver_phone != null)
                            operationToUpdate.DriverPhoneNumber = driver_phone;
                        if (manager_name != null)
                            operationToUpdate.PartnerManagerName = manager_name;
                        if (manager_phone != null)
                            operationToUpdate.PartnerManagerPhoneNumber = manager_phone;
                        if (manager_email != null)
                            operationToUpdate.PartnerManagerEmail = manager_email;
                        if (note != null)
                            operationToUpdate.Note = note;
                        //
                        yardStorage.EditOperation(operationToUpdate);
                        try
                        {
                            if (operationToUpdate.Gate != gate)
                                yardStorage.SetOperationGate(operationToUpdate, gate);
                        }
                        catch (Exception)
                        {
                            yardStorage.SetOperationGate(operationToUpdate);
                            // TODO say to user that gates has not been successfully updated
                        }


                        Text = JsonSerializer.Serialize(operationToUpdate);
                    }

                }
                else
                {
                    Text = "eror||ошибка, информация не обновлена";
                }
            }
            else
            {
                // create
                var type = yardStorage.GetOperationType(operation_type);
                var vehicleKind = yardStorage.GetVehicleKind(vehicle_kind);

                var startDate = DateTime.Parse(start_date);
                var finishDate = startDate;

                finishDate = finishDate.Add(new TimeSpan(0, 0, yardStorage.GetTime(type, vehicleKind)));

                short priority = 1;
                var partnerManagerName = string.Empty;
                var partnerManagerPhoneNumber = string.Empty;
                var partnerManagerEmail = string.Empty;

                var trailerNumber = string.Empty;

                var defState = yardStorage.GetOperationState("CREATED");
                var defManager = yardStorage.GetEmployee(100004); // TODO load dinamicly

                var newOperation = new Operation(-1, null, type, startDate, finishDate,
                    vehicleKind, vehicle_brand, vehicle_model, vehicle_number, trailerNumber, driver_name, string.Empty,
                    defState,
                    priority,
                    partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                    defManager);

                if (driver_phone != null)
                    newOperation.DriverPhoneNumber = driver_phone;
                if (manager_name != null)
                    newOperation.PartnerManagerName = manager_name;
                if (manager_phone != null)
                    newOperation.PartnerManagerPhoneNumber = manager_phone;
                if (manager_email != null)
                    newOperation.PartnerManagerEmail = manager_email;
                if (note != null)
                    newOperation.Note = note;

                //type.Direction
                ActionKind actionKind = ActionKind.Income;

                switch (type.Direction)
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

                try
                {
                    newOperation.AddAction(yardStorage.GetAction(actionKind, yardStorage.GetPartner(Request.Form["partner_id"]), order_id));
                    yardStorage.AddOperation(newOperation);
                    try
                    {
                        yardStorage.SetOperationGate(newOperation);
                    }
                    catch (Exception e)
                    {
                        Text = "eror||ошибка, ворота не были проставлены, измените время или тип машины";
                    }

                    Text = JsonSerializer.Serialize(newOperation);
                }
                //catch (WarehouseActionNotFoundException e) // bruh dont working
                catch (WarehouseActionNotFoundException)
                {
                    Text = "eror||ошибка, номер заказа партнера не соответствует типу заказа / не действителен или отсутствует в БД";
                }




            }

            return Text;
        }


        [HttpPost("ChangeOperationState")]
        public string PostChangeOperationState()
        {
            var Text = "";

            string operationID = Request.Form["operation_id"];
            string actionWithOperation = Request.Form["action"];

            if (long.TryParse(operationID, out long tmpID))
            {
                var loadedOperation = yardStorage.GetOperation(tmpID);

                switch (actionWithOperation)
                {
                    case "StartOperation":
                        yardStorage.StartOperation(loadedOperation);
                        break;
                    case "ResetOperationGate":
                        yardStorage.SetOperationGate(loadedOperation);
                        break;
                    case "StartOperationWork":
                        yardStorage.StartOperationWork(loadedOperation);
                        break;
                    case "FinishOperation":
                        yardStorage.FinishOperation(loadedOperation);
                        break;
                    case "FinishOperationWork":
                        yardStorage.FinishOperationWork(loadedOperation);
                        break;
                    case "CancelOperation":
                        yardStorage.CancelOperation(loadedOperation);
                        break;
                    case "DeleteOperation":
                        yardStorage.DeleteOperation(loadedOperation);
                        break;

                    default:
                        Text = "operation is not specified";
                        break;
                }

                //
                Text = JsonSerializer.Serialize(loadedOperation);
            }

            return Text;
        }

        [HttpGet("SearchWorkersByName")]
        public string GetSearchWorkersByName()
        {
            // default
            string Text = string.Empty;
            //

            string search_querry = Request.Query["search"];
            string position_code = Request.Query["position_code"];

            var loadedPosition = yardStorage.GetPosition(position_code);

            var workers = yardStorage.FindEmployees(loadedPosition, search_querry);
            //////
            Text = JsonSerializer.Serialize(workers);
            return Text;
        }

        [HttpPost("UpdateOperationWorkers")]
        public string PostUpdateOperationWorkers()
        {
            var Text = "";

            string operation_id = Request.Form["operation_id"];
            string action_type = Request.Form["action_type"];

            string worker_id = Request.Form["worker_id"];
            string position_code = Request.Form["position_code"];

            if (long.TryParse(operation_id, out long operationID))
                if (int.TryParse(worker_id, out int workerID))
                {
                    var loadedOperation = yardStorage.GetOperation(operationID);
                    var loadedWorker = yardStorage.GetEmployee(workerID);
                    var loadedPosition = yardStorage.GetPosition(position_code);

                    switch (action_type)
                    {
                        case "add":
                            yardStorage.AddWorker(loadedOperation, loadedWorker, loadedPosition, 0.0, MeasureUnit.Null);
                            break;
                        case "del":
                            yardStorage.RemoveWorker(loadedOperation, loadedWorker, loadedPosition);
                            break;

                        default:
                            Text = "action type is not specified";
                            break;
                    }

                    //
                    Text = JsonSerializer.Serialize(loadedOperation);
                }

            return Text;
        }



    }
}