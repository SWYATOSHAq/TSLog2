using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class PrintDispersalModel : PageModel
    {
        private readonly ILogger<PrintDispersalModel> _logger;
        public readonly IYardStorage yardStorage;

        public PrintDispersalModel(ILogger<PrintDispersalModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;

        }

        public void OnGet()
        {

        }
    }
}