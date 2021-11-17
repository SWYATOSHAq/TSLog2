using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.Common;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboundsController : ControllerBase
    {
        private readonly ILinksOutboundsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

        public OutboundsController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksOutboundsStorage)storage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public async Task<string> PostOrder()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var outboundInstruction = _xmlConverter.OutboundInstructionFromXml(reader);
            var converterProducts = new EINWMSOutboundsConverter();

            var transactionNumber = _storage.GenerateTransactionId();
            var fileContent = converterProducts.Convert(transactionNumber, outboundInstruction);

            UtilsIO.WriteFileToWMSINFolder(fileContent, transactionNumber);

            return "" + fileContent; // todo retrun smth userfull
        }

        //[HttpGet]
        //public ActionResult<OutboundNotification[]> GetOrderResponses()
        //{
        //    // ------------------------------
        //    // DEBUG place by Valerian

        //    return _storage.GetOrderResponses();
        //    // ------------------------------
        //    //return string.Empty;
        //}

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
            /*
            * на данный момент в системе уведомления об отгрузке формируются супер сложным запросом по триггерам
            * в данный запрос вшит фрагмент бизнес процесса
            * 
            * например для котона уведомления происходят при переносе заказа с одной ТЕ на другую
            * 
            * при переводе с KMW на PC - уведомление о том что заказ подобран
            * при переводе с PC на WRS - уведомление о том что заказ упакован
            * при переводе с WRS на TOR - уведомление о том что заказ готов к отгрузке
            */

            //var notifocationInbound = _storage.GetOutboundNotification(notificationId); //  TODO pass a param with code of picking
            //var xml = _xmlConverter.OutboundNotificationToXml(notifocationInbound);
            //xml = Utils.PrepXML(xml);


            //return xml;
            return "";
        }

        [HttpGet("PickingFinishNotifications")]
        public string GetPickingFinishNotifications()
        {
            return string.Empty;
        }

        [HttpGet("PickingFinishNotifications/{notificationId}")]
        public string GetPickingFinishNotification(string notificationId)
        {
            /*
            * на данный момент в системе уведомления об отгрузке формируются супер сложным запросом по триггерам
            * в данный запрос вшит фрагмент бизнес процесса
            * 
            * например для котона уведомления происходят при переносе заказа с одной ТЕ на другую
            * 
            * при переводе с KMW на PC - уведомление о том что заказ подобран
            * при переводе с PC на WRS - уведомление о том что заказ упакован
            * при переводе с WRS на TOR - уведомление о том что заказ готов к отгрузке
            */

            //var notifocationInbound = _storage.GetOutboundNotification(notificationId); // TODO pass a param with picking finish code
            //var xml = _xmlConverter.OutboundNotificationToXml(notifocationInbound);
            //xml = Utils.PrepXML(xml);


            //return xml;
            return "";
        }

        [HttpGet("ShippingNotifications")]
        public string GetShippingNotifications()
        {
            return string.Empty;
        }

        [HttpGet("ShippingNotifications/{notificationId}")]
        public string GetShippingNotification(string notificationId)
        {
            /*
             * на данный момент в системе уведомления об отгрузке формируются супер сложным запросом по триггерам
             * в данный запрос вшит фрагмент бизнес процесса
             * 
             * например для котона уведомления происходят при переносе заказа с одной ТЕ на другую
             * 
             * при переводе с KMW на PC - уведомление о том что заказ подобран
             * при переводе с PC на WRS - уведомление о том что заказ упакован
             * при переводе с WRS на TOR - уведомление о том что заказ готов к отгрузке
             */


            //var notifocationInbound = _storage.GetShippingNotification(notificationId); 
            //var xml = _xmlConverter.OutboundNotificationToXml(notifocationInbound);
            //xml = Utils.PrepXML(xml);


            //return xml;
            return "";
        }

        // =======================

        // отгрузка ФАКТ ПО МЕСТАМ
        // отгрузка

    }

}