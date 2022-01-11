using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class NewRequestModel : PageModel
    {
        private readonly ILogger<NewRequestModel> _logger;
        public readonly IYardStorage yardStorage;

        public NewRequestModel(ILogger<NewRequestModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        { }
    }
}