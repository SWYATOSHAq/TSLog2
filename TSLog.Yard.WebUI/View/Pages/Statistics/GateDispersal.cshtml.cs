using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class GateDispersalModel : PageModel
    {
        private readonly ILogger<GateDispersalModel> _logger;
        public readonly IYardStorage yardStorage;

        public GateDispersalModel(ILogger<GateDispersalModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }
        public void OnGet()
        { }
    }
}