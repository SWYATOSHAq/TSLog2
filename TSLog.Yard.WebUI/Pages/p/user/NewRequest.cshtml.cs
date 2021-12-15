using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Links.YardManagement.Pages
{
    public class NewRequestModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public NewRequestModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}