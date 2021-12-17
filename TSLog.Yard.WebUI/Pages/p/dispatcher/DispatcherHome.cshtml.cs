using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Yard.WebUI.Pages
{
    public class DispatcherHomeModel : PageModel
    {
        private readonly ILogger<DispatcherHomeModel> _logger;

        public DispatcherHomeModel(ILogger<DispatcherHomeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}