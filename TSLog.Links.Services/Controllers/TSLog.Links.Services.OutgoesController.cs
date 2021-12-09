using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.Common;
using TSLog.LegalEntities.BO;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OutgoesController : ControllerBase
    {
        private readonly ILinksOutgoesStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;
        private readonly bool DebugMode = true;

        public OutgoesController(IConfiguration configuration, ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksOutgoesStorage)storage;
            _xmlConverter = xmlConverter;
            DebugMode = configuration.GetValue<bool>("DebugMode");
        }

        [HttpPost]
        public async Task<string> PostOutgoRequest()
        {

            var user = (LegalEntity)HttpContext.Items["User"];

            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var outgoRequest = _xmlConverter.OutgoRequestFromXml(reader);

            if (user == outgoRequest.ServiceBuyer)
            {
                var outgoesConverter = new EINWMSOutboundsConverter();

                var transactionId = _storage.GenerateTransactionId();
                var fileContent = outgoesConverter.Convert(transactionId, outgoRequest);
                UtilsIO.WriteFileToWMSINFolder(fileContent, transactionId);

                if (!DebugMode)
                    return "<status code=\"0\">Запрос на расход отправлен на загрузку</status>";
                else
                    return "" + fileContent;
            }
            else
                return "<status code=\"1\">ошибка входных данных: неверный logisticServicesBuyer</status>";

        }

        [HttpGet("OutgoResposes/{outgoId}")]
        public string GetOutgoResponse(string outgoId)
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            // TODO: realize
            return string.Empty;
        }

        [HttpGet("OutgoResposes")]
        public string GetOutgoResponses()
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            // TODO: realize
            return string.Empty;
        }

        [HttpGet("PickingBeginNotices/{outgoId}")]
        public string GetPickingBeginNotice(string outgoId)
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

            var pickingBeginNotice = _storage.GetPickingBeginNotice(outgoId);
            var xml = _xmlConverter.PickingBeginNoticeToXml(pickingBeginNotice);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("PickingBeginNotices")]
        public string GetPickingBeginNotices()
        {
            var user = (LegalEntity)HttpContext.Items["User"];
            var partners = new LegalEntity[] { user };

            var pickingBeginNotices = _storage.GetPickingBeginNotices(partners);
            var xml = _xmlConverter.PickingBeginNoticesToXml(pickingBeginNotices);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("PickingEndNotices/{outgoId}")]
        public string GetPickingEndNotice(string outgoId)
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
            var user = (LegalEntity)HttpContext.Items["User"];

            var pickingEndNotice = _storage.GetPickingEndNotice(user, outgoId);
            var xml = _xmlConverter.PickingEndNoticeToXml(pickingEndNotice);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("PickingEndNotices")]
        public string GetPickingEndNotices()
        {
            var user = (LegalEntity)HttpContext.Items["User"];
            var partners = new LegalEntity[] { user };

            var pickingEndNotices = _storage.GetPickingEndNotices(partners);
            var xml = _xmlConverter.PickingEndNoticesToXml(pickingEndNotices);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("ShippingNotices/{outgoId}")]
        public string GetShippingNotice(string outgoId)
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
            var user = (LegalEntity)HttpContext.Items["User"];

            var shippingNotice = _storage.GetShippingNotice(user, outgoId);
            var xml = _xmlConverter.ShippingNoticeToXml(shippingNotice);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("ShippingNotices")]
        public string GetShippingNotices()
        {
            var user = (LegalEntity)HttpContext.Items["User"];
            var partners = new LegalEntity[] { user };

            var shippingNotices = _storage.GetShippingNotices(partners);
            var xml = _xmlConverter.ShippingNoticesToXml(shippingNotices);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }
    }
}