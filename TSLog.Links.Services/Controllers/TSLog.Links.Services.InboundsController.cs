using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundsController : ControllerBase
    {
        private readonly ILinksInboundsStorage _storage = null;
        public InboundsController(ILinksStorage storage)
        {
            _storage = (ILinksInboundsStorage) storage;
        }

        [HttpPost]
        public string PostInbound(string data)
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
        
        [HttpGet("{inboundId}")]
        public string GetInboundNotification(string inboundId) 
        {
            return string.Empty;
        }
    }
}
