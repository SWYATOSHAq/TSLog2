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
    public class IncomesController : ControllerBase
    {
        private readonly ILinksInboundsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

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
            var converterProducts = new EINWMSInboundsConverter();

            var transactionNumber = _storage.GenerateTransactionId();
            var fileContent = converterProducts.Convert(transactionNumber, incomeRequest);

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
        public string GetReceiptEndNoticeNotification(string incomeId)
        {
            // todo dont work
            var receiptEndNotice = _storage.GetReceiptEndNotice(incomeId);
            var xml = _xmlConverter.ReceiptEndNoticeToXml(receiptEndNotice);
            xml = Utils.PrepXML(xml);

            return xml;
        }

        [HttpGet("ReceiptEndNotices")]
        public string GetReceiptEndNoticeNotifications()
        {
            // TODO: realize
            return string.Empty;
        }
    }
}