using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class GatesPageModel : PageModel
    {
        private readonly ILogger<GatesPageModel> _logger;
        public IRightsChecker RightsChecker { get; private set; }
        public IYardStorage Storage { get; private set; }        
        public GatesPageModel(ILogger<GatesPageModel> logger, IRightsChecker rightsChecker, IYardStorage storage)
        {
            _logger = logger;
            RightsChecker = rightsChecker;
            Storage = storage;            
        }
        public void OnGet()
        { }
    }
}