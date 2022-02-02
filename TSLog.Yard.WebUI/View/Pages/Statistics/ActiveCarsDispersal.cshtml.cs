using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class ActiveCarsDispersalModel : PageModel
    {
        private readonly ILogger<ActiveCarsDispersalModel> _logger;
        public readonly IYardStorage yardStorage;

        public ActiveCarsDispersalModel(ILogger<ActiveCarsDispersalModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        { }
    }
}