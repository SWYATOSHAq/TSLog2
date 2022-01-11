using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class UserHomeModel : PageModel
    {
        private readonly ILogger<UserHomeModel> _logger;
        public readonly IYardStorage yardStorage;

        public UserHomeModel(ILogger<UserHomeModel> logger, IYardStorage storage)
        {
            _logger = logger;
            yardStorage = storage;
        }

        public void OnGet()
        { }
    }
}