using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;
using TSLog.Yard.WebUI.Models;

namespace TSLog.Yard.WebUI.Models
{
    public class ECReportDetailedPageModel : PageModel
    {
        private readonly ILogger<ECReportDetailedPageModel> _logger;
        /// <summary>
        /// Хранилище объектов
        /// </summary>
        public IYardStorage Storage { get; private set; }
        /// <summary>
        /// Объект для проверки прав
        /// </summary>
        public RightsChecker RightsChecker { get; private set; }
        public ECReportDetailedPageModel(ILogger<ECReportDetailedPageModel> logger,
            IYardStorage storage, RightsChecker rightsChecker)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
        }
        public void OnGet()
        {
        }
    }
}
