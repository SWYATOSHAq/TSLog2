using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class PresentVehiclesPageModel : PageModel
    {
        private readonly ILogger<PresentVehiclesPageModel> _logger;
        public IYardStorage Storage { get; private set; }
        public RightsChecker RightsChecker { get; private set; }
        public PresentVehiclesPageModel(ILogger<PresentVehiclesPageModel> logger, IYardStorage storage, RightsChecker rightsChecker)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
        }
        public void OnGet()
        { }
    }
}