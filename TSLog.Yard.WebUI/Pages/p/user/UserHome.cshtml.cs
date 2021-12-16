using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TSLog.Yard.WebUI.Pages
{
    public class UserHomeModel : PageModel
    {
        private readonly ILogger<UserHomeModel> _logger;

        public UserHomeModel(ILogger<UserHomeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}