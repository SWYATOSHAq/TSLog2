using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationPageModel : PageModel
    {
        private readonly ILogger<OperationPageModel> _logger;
        public IRightsChecker RightsChecker { get; private set; }
        public IYardStorage Storage { get; private set; }        
        public OperationPageModel(ILogger<OperationPageModel> logger, IRightsChecker rightsChecker, IYardStorage storage)
        {
            _logger = logger;
            RightsChecker = rightsChecker;
            Storage = storage;            
        }
        public void OnGet()
        { }
    }
}