using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class StaffPageModel : PageModel
    {
        private readonly ILogger<StaffPageModel> _logger;
        /// <summary>
        /// Объект для проверки прав
        /// </summary>
        public IRightsChecker RightsChecker { get; private set; }
        /// <summary>
        /// Хранилище объектов
        /// </summary>
        public IYardStorage Storage { get; private set; }        
        public StaffPageModel(ILogger<StaffPageModel> logger, IRightsChecker rightsChecker, IYardStorage storage)
        {
            _logger = logger;
            RightsChecker = rightsChecker;
            Storage = storage;            
        }
        public void OnGet()
        { }
    }
}