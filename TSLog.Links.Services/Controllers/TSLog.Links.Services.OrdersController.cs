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

        [HttpPost]
        public string PostOrder(OrderNotofocation order)
        {
            return string.Empty;
        }

        [HttpGet]
        public string GetOrderResponses() 
        {
            return string.Empty;
        }

        [HttpGet("orderId")]
        public string GetOrderResponse(string orderId)
        {
            return string.Empty;
        }

        [HttpGet]
        public string GetPickingStartNotifications()
        {
            return string.Empty;
        }

        [HttpGet("orderId")]
        public string GetPickingStartNotification(string orderId)
        {
            return string.Empty;
        }

        [HttpGet]
        public string GetPickingFinishNotifications()
        {
            return string.Empty;
        }

        [HttpGet("orderId")]
        public string GetPickingFinishNotification(string orderId)
        {
            return string.Empty;
        }

        [HttpGet]
        public string GetShippingNotifications()
        {
            return string.Empty;
        }

        [HttpGet("orderId")]
        public string GetShippingNotification(string orderId)
        {
            return string.Empty;
        }
    }
}
