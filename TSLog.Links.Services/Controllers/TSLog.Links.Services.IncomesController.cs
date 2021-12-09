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
    public class IncomesController : ControllerBase
    {
        private readonly ILinksIncomesStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;
        private readonly bool DebugMode = true;

        public IncomesController(IConfiguration configuration, ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksIncomesStorage)storage;
            _xmlConverter = xmlConverter;
            DebugMode = configuration.GetValue<bool>("DebugMode");
        }

        [HttpPost]
        public async Task<string> PostIncomeRequest()
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var incomeRequest = _xmlConverter.IncomeRequestFromXml(reader);

            if (user == incomeRequest.ServiceBuyer)
            {
                var incomesConverter = new EINWMSInboundsConverter();

                var transactionId = _storage.GenerateTransactionId();
                var fileContent = incomesConverter.Convert(transactionId, incomeRequest);
                UtilsIO.WriteFileToWMSINFolder(fileContent, transactionId);

                if (!DebugMode)
                    return "<status code=\"0\">Запрос на приход отправлен на загрузку</status>";
                else
                    return "" + fileContent;
            }
            else
                return "<status code=\"1\">ошибка входных данных: неверный logisticServicesBuyer</status>";
        }

        [HttpGet("IncomeResponses/{incomeId}")]
        public string GetIncomeResponse(string incomeId)
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            // TODO: realize
            return string.Empty;
        }

        [HttpGet("IncomeResponses")]
        public string GetIncomeResponses()
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            // TODO: realize
            return string.Empty;
        }

        [HttpGet("ReceiptEndNotices/{incomeId}")]
        public string GetReceiptEndNotice(string incomeId)
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            var receiptEndNotice = _storage.GetReceiptEndNotice(user, incomeId);
            var xml = _xmlConverter.ReceiptEndNoticeToXml(receiptEndNotice);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("ReceiptEndNotices")]
        public string GetReceiptEndNotices()
        {
            var user = (LegalEntity)HttpContext.Items["User"];
            var partners = new LegalEntity[] { user };

            var receiptEndNotices = _storage.GetReceiptEndNotices(partners);
            var xml = _xmlConverter.ReceiptEndNoticesToXml(receiptEndNotices);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }
    }
}