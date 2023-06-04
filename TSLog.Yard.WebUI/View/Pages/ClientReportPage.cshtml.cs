using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class ClientReportPageModel : PageModel
    {
        private readonly ILogger<ClientReportPageModel> _logger;
        /// <summary>
        /// Хранилище объектов.
        /// </summary>
        public IYardStorage Storage { get; private set; }
        /// <summary>
        /// Объект для проверки прав.
        /// </summary>
        public IRightsChecker RightsChecker { get; private set; }
        public ClientReportPageModel(ILogger<ClientReportPageModel> logger, IYardStorage storage, IRightsChecker rightsChecker)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
        }
        public void OnGet()
        { }
    }
}
