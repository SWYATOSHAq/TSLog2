using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    //[Authorize]
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