using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class DispatcherHomeModel : PageModel
    {
        private readonly ILogger<DispatcherHomeModel> _logger;
        public readonly IYardStorage yardStorage;

        public DispatcherHomeModel(ILogger<DispatcherHomeModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        { }
    }
}