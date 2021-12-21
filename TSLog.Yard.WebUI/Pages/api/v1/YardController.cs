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

            var partner = yardStorage.GetPartner(partnerID);

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

        [HttpGet("createUpdateOperation")]
        public string GetInsertUpdateData()
        {
            string operationID = Request.Form["operation_id"];

            var Text = "fuuuuuuuuuuuuuuuuuck";

            if (operationID != null || operationID != "")
            {
                // update 
                // TODO
            }
            else
            {
                // create
                var type = yardStorage.GetOperationType(Request.Query["type"]);

                var startDate = DateTime.Parse(Request.Query["start_date"]);
                var finishDate = DateTime.Parse(Request.Query["finish_date"]);

                var workersCount = short.Parse(Request.Query["workers_count"]);

                var vehicleKind = yardStorage.GetVehicleKind(Request.Query["vehicle_kind"]);

                var vehicleBrand = Request.Query["vehicle_kind"];
                var vehicleModel = Request.Query["vehicle_model"];
                var vehicleNumber = Request.Query["vehicle_number"];
                var vehicleDriverName = Request.Query["vehicle_driver_name"];

                var state = yardStorage.GetOperationState(Request.Query["state"]);

                short priority = 1;


                var newOperation = new Operation(-1, null, type, startDate, finishDate, workersCount, vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, vehicleDriverName, state, priority, null);

                yardStorage.AddOperation(newOperation);
                yardStorage.SetOperationGate(newOperation); // TODO catch an erorr
            }

            return Text;
        }


        [HttpGet("changeOperationState")]
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
