using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILinksOrdersStorage _storage = null;
        public OrdersController(ILinksStorage storage)
        {
            _storage = (ILinksOrdersStorage) storage;
        }

        [HttpPost]
        public string PostOrder(OutboundInstruction order)
        {
            //return string.Empty;
            var items = _storage.GetOrderResponses();
            var sb = new StringBuilder();
            foreach (var i in items)
            {
                sb.Append(i.OrderId + "\n");
            }
            return sb.ToString();
        }

        [HttpGet]
        public ActionResult<OutboundNotification[]> GetOrderResponses()
        {
            // ------------------------------
            // DEBUG place by Valerian

            return _storage.GetOrderResponses();
            // ------------------------------
            //return string.Empty;
        }

        [HttpGet("{orderId}")]
        public string GetOrderResponse(string orderId)
        {
            return string.Empty;
        }

        [HttpGet("PickingStartNotifications")]
        public string GetPickingStartNotifications()
        {
            return string.Empty;
        }

        [HttpGet("PickingStartNotifications/{notificationId}")]
        public string GetPickingStartNotification(string notificationId)
        {
            return "req PickingStartNotifications notification: " + notificationId;
        }

        [HttpGet("PickingFinishNotifications")]
        public string GetPickingFinishNotifications()
        {
            return string.Empty;
        }

        [HttpGet("PickingFinishNotifications/{notificationId}")]
        public string GetPickingFinishNotification(string notificationId)
        {
            return "req PickingFinishNotifications notification: " + notificationId;
        }

        [HttpGet("ShippingNotifications")]
        public string GetShippingNotifications()
        {
            return string.Empty;
        }

        [HttpGet("ShippingNotifications/{notificationId}")]
        public string GetShippingNotification(string notificationId)
        {
            return "req ShippingNotifications notification: " + notificationId;
        }
    }
}
