using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Links.YardManagement.Pages
{
    public class DispetcherHomeModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public DispetcherHomeModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}