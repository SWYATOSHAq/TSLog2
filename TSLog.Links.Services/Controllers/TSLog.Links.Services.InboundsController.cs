using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
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
        public async Task<string> PostInbound()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var inboundInstruction = _xmlConverter.InboundInstructionFromXml(reader);
            var converterProducts = new EINWMSInboundsConverter();

            var transactionNumber = _storage.GenerateTransactionId();
            var fileContent = converterProducts.Convert(transactionNumber, inboundInstruction);

            UtilsIO.WriteFileToWMSINFolder(fileContent, transactionNumber);

            return "" + fileContent; // todo retrun smth userfull

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
            // todo dont work
            var notifocationInbound = _storage.GetInboundNotification(notificationId);
            var xml = _xmlConverter.InboundNotificationToXml(notifocationInbound);
            xml = Utils.PrepXML(xml);


            return xml;
        }
    }
}