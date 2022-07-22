using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Models
{
    public class DailyReportPageModel : PageModel
    {
        private readonly ILogger<DailyReportPageModel> _logger;
        public IBossControlStorage Storage { get; private set; }
        public List<Endpoint> Endpoints { get; private set; }
        public DailyReportPageModel(ILogger<DailyReportPageModel> logger,
            IBossControlStorage storage, 
            IEnumerable<EndpointDataSource> endpointSources)
        {
            _logger = logger;
            Storage = storage;
            Endpoints = new List<Endpoint>();
            foreach (var es in endpointSources)
                Endpoints.AddRange(es.Endpoints);
        }
        public void OnGet()
        { }
    }
}