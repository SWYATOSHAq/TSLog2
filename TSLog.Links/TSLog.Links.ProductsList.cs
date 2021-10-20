using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    public class ProductsList : Message
    {
        public ProductsList(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            ProductsData = new List<Product>();
        }
        //
        public List<Product> ProductsData { get; set; }
        //
    }
}
