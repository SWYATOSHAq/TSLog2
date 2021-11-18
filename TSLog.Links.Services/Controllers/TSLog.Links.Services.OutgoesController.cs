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
    public class OutgoesController : ControllerBase
    {
        private readonly ILinksOutboundsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

        public OutgoesController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksOutboundsStorage)storage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public async Task<string> PostOutgoRequest()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var outgoRequest = _xmlConverter.OutgoRequestFromXml(reader);
            var converterProducts = new EINWMSOutboundsConverter();

            var transactionNumber = _storage.GenerateTransactionId();
            var fileContent = converterProducts.Convert(transactionNumber, outgoRequest);

            UtilsIO.WriteFileToWMSINFolder(fileContent, transactionNumber);

            return "" + fileContent; // todo retrun smth userfull
        }


        [HttpGet("OutgoResposes/{outgoId}")]
        public string GetOutgoResponse(string outgoId)
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("OutgoResposes")]
        public string GetOutgoResponses()
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("PickingBeginNotifications/{outgoId}")]
        public string GetPickingBeginNotification(string outgoId)
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
            // TODO: realize
            return "";
        }

        [HttpGet("PickingBeginNotifications")]
        public string GetPickingBeginNotifications()
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("PickingEndNotifications/{notificationId}")]
        public string GetPickingEndNotification(string outgoId)
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
            // TODO: realize
            return "";
        }

        [HttpGet("PickingEndNotifications")]
        public string GetPickingEndNotifications()
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("ShippingNotifications/{outgoId}")]
        public string GetShippingNotification(string outgoId)
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
            // TODO: realize
            return "";
        }

        [HttpGet("ShippingNotifications")]
        public string GetShippingNotifications()
        {
            // TODO: realize
            return string.Empty;
        }
    }

}