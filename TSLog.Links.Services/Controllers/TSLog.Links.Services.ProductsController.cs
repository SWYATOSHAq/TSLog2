using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Pipelines;
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

            var fileContent = converterProducts.Convert(_storage.GenerateTransactionId(), itemDataNotification);

            UtilsIO.WriteFileToWMSINFolder(fileContent, System.IO.Path.GetRandomFileName()); // TODO

            return ""+ fileContent;

            //Stream req = Request.Body;
            //req.Seek(0, System.IO.SeekOrigin.Begin);
            //string content = new StreamReader(req).ReadToEnd();
            //return $"suck on my {content}";
            //var items = _storage.GetOrderResponses();
            //var sb = new StringBuilder();
            //foreach (var i in items)
            //{
            //    sb.Append(i.OrderId + "\n");
            //}
            //return sb.ToString();
        }

        //[HttpGet]
        //public ActionResult<BO.Product[]> GetProducts()
        //{
        //    var data = _storage.GetProductsList(new LegalEntity("1234")).Products; // TODO get client from reqest context

        //    return data;

        //    //var items = _storage.GetOrderResponses();
        //    //var sb = new StringBuilder();
        //    //foreach (var i in items)
        //    //{
        //    //    sb.Append(i.OrderId + "\n");
        //    //}
        //    //return sb.ToString();
        //}

        //[HttpGet("{clientId}")]
        //public string GetProductById(string clientId)
        //{
        //    return string.Empty;
        //}
    }
}