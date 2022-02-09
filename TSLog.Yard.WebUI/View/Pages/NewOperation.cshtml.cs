using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class NewOperationModel : PageModel
    {
        private readonly ILogger<NewOperationModel> _logger;
        public RightsChecker RightsChecker { get; private set; }
        public IYardStorage Storage { get; private set; }        
        public NewOperationModel(ILogger<NewOperationModel> logger, RightsChecker rightsChecker, IYardStorage storage)
        {
            _logger = logger;
            RightsChecker = rightsChecker;
            Storage = storage;            
        }
        public void OnGet()
        { }
    }
}