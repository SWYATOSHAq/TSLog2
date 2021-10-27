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
        private readonly List<Product> _products = null;
        public LegalEntity DataSource { get; set; }
        public LegalEntity DataRecipient { get; set; }
        public ProductsList(long id, byte version)
            : base(id, 'I', "PRODUCTS_LIST", version)
        {
            _products = new List<Product>();
        }
        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
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
        public Product[] Products => _products.ToArray();
    }
}
