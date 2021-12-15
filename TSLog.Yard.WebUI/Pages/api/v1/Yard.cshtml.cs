using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Links.YardManagement.Pages
{
    public class YardModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public YardModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}