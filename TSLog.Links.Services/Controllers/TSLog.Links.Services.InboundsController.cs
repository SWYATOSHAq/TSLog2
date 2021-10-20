using Microsoft.AspNetCore.Mvc;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundsController : ControllerBase
    {
        [HttpPost]
        public string PostInbound(string data)
        {
            return string.Empty;
        }
        
        [HttpGet("{inboundId}")]
        public string GetInboundNotification(string inboundId) 
        {
            return string.Empty;
        }
    }
}
