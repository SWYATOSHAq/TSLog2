using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class HumanDispersalModel : PageModel
    {
        private readonly ILogger<HumanDispersalModel> _logger;
        public RightsChecker RightsChecker { get; private set; }
        public IYardStorage Storage { get; private set; }        
        public HumanDispersalModel(ILogger<HumanDispersalModel> logger, RightsChecker rightsChecker, IYardStorage storage)
        {
            _logger = logger;
            RightsChecker = rightsChecker;
            Storage = storage;            
        }
        public void OnGet()
        { }
    }
}