using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class HumanDispersalModel : PageModel
    {
        private readonly ILogger<HumanDispersalModel> _logger;
        public readonly IYardStorage yardStorage;

        public HumanDispersalModel(ILogger<HumanDispersalModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        { }
    }
}