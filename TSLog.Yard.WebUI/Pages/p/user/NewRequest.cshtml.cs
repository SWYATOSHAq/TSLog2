using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Yard.WebUI.Pages
{
    public class NewRequestModel : PageModel
    {
        private readonly ILogger<NewRequestModel> _logger;

        public NewRequestModel(ILogger<NewRequestModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}