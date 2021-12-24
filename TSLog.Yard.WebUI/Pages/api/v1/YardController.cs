using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TSLog.Partners.BO;
using TSLog.Yard.BO;

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
            var Text = "";
            //
            string strDate = Request.Query["date"];
            var date = DateTime.Parse(strDate);


            string strOperation = Request.Query["operation"];
            var operation = yardStorage.GetOperationType(strOperation);

            string strVehicleKind = Request.Query["vehicle_kind"];
            var vehicleKind = yardStorage.GetVehicleKind(strVehicleKind);

            //

            var busyPeriods = yardStorage.GetAvalibleIntervals(date, date.AddDays(2), operation, vehicleKind);
            //
            Text = JsonSerializer.Serialize(busyPeriods);
            return Text;
        }

        [HttpGet("OperationLength")]
        public string GetOperationLength()
        {
            // TODO test
            // default
            var Text = "";
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

            long tmpID = 0;
            if (long.TryParse(operationID, out tmpID))
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
            string partnerID = Request.Query["partner_id"];

            Partner partner = null;
            if (partnerID != null)
            {
                partner = yardStorage.GetPartner(partnerID);
            }

            var searchPartners = (partner == null || partner == Partner.Null) ? null : new Partner[] { partner };

            DateTime tmp;
            if (DateTime.TryParse(operationDate, out tmp))
            {
                // search for period of 1 day or 2 days?
                var daysSpan = 2;

                var loadedOperations = yardStorage.GetOperations(tmp.Date, tmp.AddDays(daysSpan).AddTicks(-1), searchPartners);
                Text = JsonSerializer.Serialize(loadedOperations);
            }

            return Text;
        }

        [HttpPost("createUpdateOperation")]
        public string GetInsertUpdateData()
        {
            var operation_type = Request.Form["operation_type"];
            //var gate = Request.Form["gate"];
            var vehicle_kind = Request.Form["vehicle_kind"];
            var vehicle_brand = Request.Form["vehicle_brand"];
            var vehicle_model = Request.Form["vehicle_model"];
            var vehicle_number = Request.Form["vehicle_number"];
            var order_id = Request.Form["order_id"];
            var driver_name = Request.Form["driver_name"];
            //
            string operation_id = Request.Form["operation_id"];
            //
            var start_date = Request.Form["start_date"];
            var finish_date = Request.Form["finish_date"];

            var Text = "";

            if (operation_id != null)
            {
                // update 
                // TODO
                Text = "upd";
            }
            else
            {
                // create
                var type = yardStorage.GetOperationType(operation_type);

                var startDate = DateTime.Parse(start_date);
                var finishDate = startDate;
                finishDate = finishDate.Add(new TimeSpan(0, type.Time, 0));

                var vehicleKind = yardStorage.GetVehicleKind(vehicle_kind);
                short priority = 1;

                var defState = yardStorage.GetOperationState("CREATED");
                var defManager = yardStorage.GetEmployee(10405); // TODO load dinamicly

                var newOperation = new Operation(-1, null, type, startDate, finishDate, vehicleKind, vehicle_brand, vehicle_model, vehicle_number, driver_name, defState, priority, defManager);

                yardStorage.AddOperation(newOperation);
                yardStorage.AddAction(newOperation, new WarehouseActions.BO.WarehouseAction(WarehouseActions.BO.ActionKind.Income, "not-set", yardStorage.GetPartner(Request.Form["partner_id"]), order_id));
                yardStorage.SetOperationGate(newOperation); // TODO catch an erorr
            }

            return Text;
        }


        [HttpPost("changeOperationState")]
        public string PostChangeOperationState()
        {
            var Text = "";

            string operationID = Request.Query["operation_id"];
            string actionWithOperation = Request.Query["operation"];

            long tmpID = 0;
            if (long.TryParse(operationID, out tmpID))
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
