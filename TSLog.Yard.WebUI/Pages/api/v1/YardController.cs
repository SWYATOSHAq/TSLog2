using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
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

        [HttpGet("operationData")]
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

        [HttpGet("operationsList")]
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

        [HttpPost("createUpdateOperation")]
        public string GetInsertUpdateData()
        {
            var operationType = Request.Form["operation_type"];
            //var gate = Request.Form["gate"];
            var vehicleKindCode = Request.Form["vehicle_kind"];
            var vehicleBrand = Request.Form["vehicle_brand"];
            var vehicleModel = Request.Form["vehicle_model"];
            var vehicleNumber = Request.Form["vehicle_number"];
            var trailerNumber = string.Empty;
            var orderId = Request.Form["order_id"];
            var driverName = Request.Form["driver_name"];
            var driverPhoneNumber = string.Empty;
            string operationId = Request.Form["operation_id"];
            var requestStartDate = Request.Form["start_date"];
            var requestFinishDate = Request.Form["finish_date"];

            string Text;

            if (!string.IsNullOrWhiteSpace(operationId))
            {
                // update 
                // TODO
                Text = "upd";
            }
            else
            {
                // create
                var type = yardStorage.GetOperationType(operationType);
                var vehicleKind = yardStorage.GetVehicleKind(vehicleKindCode);

                var startDate = DateTime.Parse(requestStartDate);
                var finishDate = startDate;

                finishDate = finishDate.Add(new TimeSpan(0, 0, yardStorage.GetTime(type, vehicleKind)));

                var priority = (short)1;
                var partnerManagerName = string.Empty;
                var partnerManagerPhoneNumber = string.Empty;
                var partnerManagerEmail = string.Empty;

                var defState = yardStorage.GetOperationState("CREATED");

                var partner = yardStorage.GetPartner(Request.Form["partner_id"]);
                var defManager = yardStorage.GetPartnerManagers(partner)[0];

                var newOperation = new Operation(-1, null, type, startDate, finishDate,
                    vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber, driverName, driverPhoneNumber,
                    defState,
                    priority,
                    partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                    defManager);

                var action = yardStorage.GetAction(ActionKind.Income, partner, orderId);
                newOperation.AddAction(action);
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
            return Text;
        }

        [HttpPost("changeOperationState")]
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
    }
}