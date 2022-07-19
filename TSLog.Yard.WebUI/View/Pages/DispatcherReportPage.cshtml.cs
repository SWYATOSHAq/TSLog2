using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class DispatcherReportPageModel : PageModel
    {
        private readonly ILogger<DispatcherReportPageModel> _logger;
        public IYardStorage Storage { get; private set; }
        public RightsChecker RightsChecker { get; private set; }
        public List<Endpoint> Endpoints { get; private set; }
        public DispatcherReportPageModel(ILogger<DispatcherReportPageModel> logger, 
            IYardStorage storage, 
            RightsChecker rightsChecker,
            IEnumerable<EndpointDataSource> endpointSources)
        {
            _logger = logger;
            Storage = storage;
            RightsChecker = rightsChecker;
            Endpoints = new List<Endpoint>();
            foreach (var es in endpointSources)
                Endpoints.AddRange(es.Endpoints);
        }
        public void OnGet()
        { }
    }
}