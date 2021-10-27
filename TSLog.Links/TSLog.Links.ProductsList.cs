using System;
using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Сообщение справочник товаров, которые упоминаются при обмене
    /// </summary>
    public class ProductsList : Message
    {
        public List<Product> _products { get; private set; }
        public LegalEntity DataSource { get; set; }
        public LegalEntity DataRecipient { get; set; }
        public ProductsList(long id, byte version, string body = null)
            : base(id, 'I', "PRODUCTS_LIST", version, body)
        {
            _products = new List<Product>();
        }
        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (product != Product.Null)
                _products.Add(product);
        }
        public void AddProducts(Product[] products)
        {
            if (products == null)
                throw new ArgumentNullException("products");
            foreach (var product in products)
                AddProduct(product);
        }
        public Product[] Products
        {
            get
            {
                return _products.ToArray();
            }
        }
    }
}
