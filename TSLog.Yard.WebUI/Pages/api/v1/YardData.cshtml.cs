using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Yard.WebUI.Pages
{
    public class YardDataModel : PageModel
    {
        private readonly ILogger<YardDataModel> _logger;

        public YardDataModel(ILogger<YardDataModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}