using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.BO;
using TSLog.Common;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomesController : ControllerBase
    {
        private readonly ILinksInboundsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;
        private readonly bool DebugMode = true;

        public IncomesController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = (ILinksInboundsStorage)storage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public async Task<string> PostIncomeRequest()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var incomeRequest = _xmlConverter.IncomeRequestFromXml(reader);
            var incomesConverter = new EINWMSInboundsConverter();

            var transactionId = _storage.GenerateTransactionId();
            var fileContent = incomesConverter.Convert(transactionId, incomeRequest);
            UtilsIO.WriteFileToWMSINFolder(fileContent, transactionId);

            if (!DebugMode)
                return "<status code=\"0\">Запрос на приход отправлен на загрузку</status>";
            else
                return "" + fileContent;
        }

        [HttpGet("IncomeResponses/{incomeId}")]
        public string GetIncomeResponse(string incomeId)
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("IncomeResponses")]
        public string GetIncomeResponses()
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("ReceiptEndNotices/{incomeId}")]
        public string GetReceiptEndNotice(string incomeId)
        {
            var receiptEndNotice = _storage.GetReceiptEndNotice(incomeId);
            var xml = _xmlConverter.ReceiptEndNoticeToXml(receiptEndNotice);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }

        [HttpGet("ReceiptEndNotices")]
        public string GetReceiptEndNotices()
        {
            var partners = new List<LegalEntity>().ToArray();
            var receiptEndNotices = _storage.GetReceiptEndNoticies(partners);
            var xml = _xmlConverter.ReceiptEndNoticesToXml(receiptEndNotices);
            if (DebugMode)
                xml = Utils.PrepXML(xml);
            return xml;
        }
    }
}