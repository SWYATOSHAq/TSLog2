using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationPrintPageModel : PageModel
    {
        private readonly ILogger<OperationPrintPageModel> _logger;
        public IYardStorage Storage { get; private set; }
        public RightsChecker RightsChecker { get; private set; }
        public OperationPrintPageModel(ILogger<OperationPrintPageModel> logger, IYardStorage storage, RightsChecker rightsChecker)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
        }
        public void OnGet()
        { }
    }
}