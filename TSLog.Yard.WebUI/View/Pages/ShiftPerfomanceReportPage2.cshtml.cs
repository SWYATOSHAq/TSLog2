using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class ShiftPerfomanceReportPage2Model : PageModel
    {
        private readonly ILogger<ShiftPerfomanceReportPage2Model> _logger;
        /// <summary>
        /// Хранилище объектов
        /// </summary>
        public IYardStorage Storage { get; private set; }
        /// <summary>
        /// Объект для проверки прав
        /// </summary>
        public RightsChecker RightsChecker { get; private set; }
        public ShiftPerfomanceReportPage2Model(ILogger<ShiftPerfomanceReportPage2Model> logger, 
            IYardStorage storage, RightsChecker rightsChecker)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
        }
        public void OnGet()
        { }
    }
}