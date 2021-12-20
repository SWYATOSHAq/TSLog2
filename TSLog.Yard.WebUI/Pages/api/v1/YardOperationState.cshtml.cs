using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class YardOperationStateModel : PageModel
    {
        private readonly ILogger<YardOperationStateModel> _logger;
        private readonly IYardStorage yardStorage;

        public string Text = "";

        public YardOperationStateModel(ILogger<YardOperationStateModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        {
            // default
            Text = "this is an API, to see more check out [http://example.com]";
            //
        }


        public void OnPost()
        {
            // default
            Text = "this is an API, to see more check out [http://example.com]";
            //
            return;

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

        }
    }
}