using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Pages
{
    public class DispatcherHomeModel : PageModel
    {
        private readonly ILogger<DispatcherHomeModel> _logger;
        public readonly IYardStorage yardStorage;
        public readonly List<Endpoint> Endpoints;

        public DispatcherHomeModel(ILogger<DispatcherHomeModel> logger, 
            IYardStorage storage, 
            IEnumerable<EndpointDataSource> endpointSources)
        {
            _logger = logger;
            yardStorage = storage;
            Endpoints = new List<Endpoint>();
            foreach (var es in endpointSources)
                Endpoints.AddRange(es.Endpoints);
        }
        public void OnGet()
        { }
    }
}