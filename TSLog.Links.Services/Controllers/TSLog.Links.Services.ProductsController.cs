using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
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
        public string PostProducts([NakedBody] string raw)
        {
            var a = Request;

            return "fuck you " + raw;

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