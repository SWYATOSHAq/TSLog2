using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationsPage2Model : PageModel
    {
        private readonly ILogger<OperationsPageModel> _logger;
        public IYardStorage Storage { get; private set; }
        public IRightsChecker RightsChecker { get; private set; }
        public List<Endpoint> Endpoints { get; private set; }
        public OperationsPage2Model(ILogger<OperationsPageModel> logger, 
            IYardStorage storage, 
            IRightsChecker rightsChecker,
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