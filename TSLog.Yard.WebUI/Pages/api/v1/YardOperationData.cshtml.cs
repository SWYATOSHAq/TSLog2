using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class YardOperationDataModel : PageModel
    {
        private readonly ILogger<YardOperationDataModel> _logger;
        private readonly IYardStorage yardStorage;

        public string Text = "";

        public YardOperationDataModel(ILogger<YardOperationDataModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        {
            // default
            Text = "this is an API, to see more check out [http://example.com]";
            //
            string operationID = Request.Query["operation_id"];

            long tmpID = 0;
            if (long.TryParse(operationID, out tmpID))
            {
                var loadedOperation = yardStorage.GetOperation(tmpID);
                Text = JsonSerializer.Serialize(loadedOperation);
            }

        }

        public void OnPost()
        {
            string operationID = Request.Form["operation_id"];

            Text = "fuuuuuuuuuuuuuuuuuck";

            if (operationID != null || operationID != "")
            {
                // update 
                // TODO
            }
            else
            {
                // create
                var type = yardStorage.GetOperationType(Request.Query["type"]);

                var workStartDate = DateTime.Parse(Request.Query["work_start_date"]);
                var workFinishDate = DateTime.Parse(Request.Query["work_finish_date"]);

                var vehicleKind = yardStorage.GetVehicleKind(Request.Query["vehicle_kind"]);

                var vehicleBrand = Request.Query["vehicle_kind"];
                var vehicleModel = Request.Query["vehicle_model"];
                var vehicleNumber = Request.Query["vehicle_number"];
                var vehicleDriverName = Request.Query["vehicle_driver_name"];

                var state = yardStorage.GetOperationState(Request.Query["state"]);

                short priority = 1;


                var newOperation = new Operation(-1, null, type, workStartDate, workFinishDate, vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, vehicleDriverName, state, priority, null);

                yardStorage.AddOperation(newOperation);
            }
        }

    }
}