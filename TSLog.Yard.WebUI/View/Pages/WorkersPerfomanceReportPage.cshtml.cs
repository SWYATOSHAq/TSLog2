using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class WorkersPerfomanceReportPageModel : PageModel
    {
        private readonly ILogger<WorkersPerfomanceReportPageModel> _logger;
        /// <summary>
        /// Хранилище объектов
        /// </summary>
        public IYardStorage Storage { get; private set; }
        /// <summary>
        /// Объект для проверки прав
        /// </summary>
        public IRightsChecker RightsChecker { get; private set; }
        public WorkersPerfomanceReportPageModel(ILogger<WorkersPerfomanceReportPageModel> logger,
            IYardStorage storage,
            IRightsChecker rightsChecker)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
        }
        public void OnGet()
        { }
    }
}