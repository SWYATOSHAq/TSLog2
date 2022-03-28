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
        public RightsChecker RightsChecker { get; private set; }
        /// <summary>
        /// Хранилище объектов
        /// </summary>
        public IYardStorage Storage { get; private set; }        
        public StaffPageModel(ILogger<StaffPageModel> logger, RightsChecker rightsChecker, IYardStorage storage)
        {
            _logger = logger;
            RightsChecker = rightsChecker;
            Storage = storage;            
        }
        public void OnGet()
        { }
    }
}