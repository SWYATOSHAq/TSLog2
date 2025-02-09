using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.Common;
using TSLog.LegalEntities.BO;
using TSLog.Links.Gs1EdiXml;
using TSLog.Products.BO;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ILinksProductsStorage _storage = null;
        private readonly Gs1EdiXmlConverter _xmlConverter = null;
        private readonly bool DebugMode = true;

        public ProductsController(IConfiguration configuration, ILinksCommonStorage storage, Gs1EdiXmlConverter xmlConverter)
        {
            _storage = storage as ILinksProductsStorage;
            _xmlConverter = xmlConverter;
            DebugMode = configuration.GetValue<bool>("DebugMode");
        }

        [HttpPost]
        public async Task<string> PostProducts()
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            string content = await new StreamReader(Request.Body).ReadToEndAsync(); // get string data from body

            var reader = new XmlTextReader(content, XmlNodeType.Document, null);
            var productsListRequest = _xmlConverter.ProductsListFromXml(reader);

            if (user == productsListRequest.DataSource)
            {
                var productsConverter = new EINWMSProductsConverter();

                var transactionId = _storage.GenerateTransactionId();
                var fileContent = productsConverter.Convert(transactionId, productsListRequest);
                IOUtils.WriteFileToWmsINFolder(fileContent, transactionId);

                if (!DebugMode)
                    return "<status code=\"0\">Справочник товаров отправлен на загрузку</status>";
                else
                    return "" + fileContent;
            }
            else
                return "<status code=\"1\">ошибка входных данных: неверный dataSource</status>";



        }

        [HttpGet("{productId}")]
        public Product GetClientById(string productId)
        {
            return _storage.GetProductByGtin(productId);
        }

        [HttpGet("balances/{productId}")]
        public string GetBalance(string productId)
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            // TODO: realize
            return string.Empty;
        }

        [HttpGet("balances")]
        public string GetBalances()
        {
            var user = (LegalEntity)HttpContext.Items["User"];

            // TODO: realize
            return string.Empty;
        }
    }
}