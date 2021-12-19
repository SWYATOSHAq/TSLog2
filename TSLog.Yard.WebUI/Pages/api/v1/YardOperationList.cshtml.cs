using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TSLog.Partners.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class YardOperationListModel : PageModel
    {
        private readonly ILogger<YardOperationListModel> _logger;
        private readonly IYardStorage yardStorage;

        public string Text = "";

        public YardOperationListModel(ILogger<YardOperationListModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        {
            // default
            Text = "this is an API, to see more check out [http://example.com]";
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

        }
    }
}