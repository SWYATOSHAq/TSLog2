using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILinksStorage _storage = null;
        public OrdersController(ILinksStorage storage)
        {
            _storage = storage;
        }

        [HttpPost("{id}")]
        public string Index(long id)
        {
            var items = _storage.GetOrderResponses();
            var sb = new StringBuilder();
            foreach (var i in items) 
            {
                sb.Append(i.Data + "\n");
            }
            return sb.ToString();
        }
    }
}
