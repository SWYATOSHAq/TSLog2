using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TSLog.Links.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ILinksProductsStorage _storage = null;
        public ProductsController(ILinksStorage storage)
        {
            _storage = (ILinksProductsStorage) storage;
        }

        [HttpPost]
        public string PostProducts(string data)
        {
            return string.Empty;
            //var items = _storage.GetOrderResponses();
            //var sb = new StringBuilder();
            //foreach (var i in items)
            //{
            //    sb.Append(i.OrderId + "\n");
            //}
            //return sb.ToString();
        }

        [HttpGet]
        public string GetProducts()
        {
            return string.Empty;
            //var items = _storage.GetOrderResponses();
            //var sb = new StringBuilder();
            //foreach (var i in items)
            //{
            //    sb.Append(i.OrderId + "\n");
            //}
            //return sb.ToString();
        }

        [HttpGet("{clientId}")]
        public string GetProductById(string clientId)
        {
            return string.Empty;
        }
    }
}
