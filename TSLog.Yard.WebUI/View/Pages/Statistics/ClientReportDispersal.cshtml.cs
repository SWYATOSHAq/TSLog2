using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class ClientReportDispersalModel : PageModel
    {
        private readonly ILogger<ClientReportDispersalModel> _logger;
        public readonly IYardStorage yardStorage;

        public ClientReportDispersalModel(ILogger<ClientReportDispersalModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        { }
    }
}