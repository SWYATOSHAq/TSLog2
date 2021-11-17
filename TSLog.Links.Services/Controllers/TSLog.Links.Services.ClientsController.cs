using Microsoft.AspNetCore.Mvc;
using TSLog.BO;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ILinksClientsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

        public ClientsController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksClientsStorage)storage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public string PostClients(string data)
        {
            return string.Empty;
            //var items = _storage.GetOrderResponses();
            //var sb = new StringBuilder();
            //foreach (var i in items)
            //{
            //    sb.Append(i.OrderId + "\n");
            //}
            //return sb.ToString();
        }

        [HttpGet]
        public ActionResult<LegalEntity[]> GetClients()
        {
            return _storage.GetClientsList().Clients;
            //var items = _storage.GetOrderResponses();
            //var sb = new StringBuilder();
            //foreach (var i in items)
            //{
            //    sb.Append(i.OrderId + "\n");
            //}
            //return sb.ToString();
        }

        [HttpGet("{clientId}")]
        public string GetClientById(string clientId)
        {
            return string.Empty;
        }
    }
}