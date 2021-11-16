using Microsoft.AspNetCore.Mvc;
using TSLog.Common;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundsController : ControllerBase
    {
        private readonly ILinksInboundsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

        public InboundsController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksInboundsStorage)storage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public string PostInbound(string data)
        {
            return data;

            //return string.Empty;
            //var items = _storage.GetOrderResponses();
            //var sb = new StringBuilder();
            //foreach (var i in items)
            //{
            //    sb.Append(i.OrderId + "\n");
            //}
            //return sb.ToString();
        }

        //[HttpGet("{inboundId}")]
        //public string GetInboundNotification(string inboundId)
        //{
        //    return string.Empty;
        //}

        [HttpGet("InboundNotifications/{notificationId}")]
        public string GetPickingFinishNotification(string notificationId)
        {

            var notifocationInbound = _storage.GetInboundNotification(notificationId);
            var xml = _xmlConverter.InboundNotificationToXml(notifocationInbound);
            xml = Utils.PrepXML(xml);


            return xml;
        }
    }
}