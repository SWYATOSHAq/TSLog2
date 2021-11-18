using Microsoft.AspNetCore.Mvc;
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
    public class ProductsController : ControllerBase
    {
        private readonly ILinksProductsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;

        public ProductsController(ILinksStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = storage as ILinksProductsStorage;
            _xmlConverter = xmlConverter;
        }

        [HttpPost]
        public async Task<string> PostProducts()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content,XmlNodeType.Document, null);
            var itemDataNotification = _xmlConverter.ProductsListFromXml(reader);
            var converterProducts = new EINWMSProductsConverter();

            var transactionNumber = _storage.GenerateTransactionId();
            var fileContent = converterProducts.Convert(transactionNumber, itemDataNotification);

            UtilsIO.WriteFileToWMSINFolder(fileContent, transactionNumber);

            return "" + fileContent; // todo retrun smth userfull
        }

        [HttpGet("balances/{productId}")]
        public string GetBalance(string productId)
        {
            // TODO: realize
            return string.Empty;
        }

        [HttpGet("balances")]
        public string GetBalances()
        {
            // TODO: realize
            return string.Empty;
        }
    }
}