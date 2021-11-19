using Microsoft.AspNetCore.Mvc;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundsController : ControllerBase
    {
        [HttpPost("{id}")]
        public string Index(long id)
        {
            return "inbound test - " + id.ToString();
        }
    }
}
