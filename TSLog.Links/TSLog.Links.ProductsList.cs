using System;
using System.Collections.Generic;
using TSLog.LegalEntities.BO;
using TSLog.Products.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Сообщение справочник товаров, которые упоминаются при обмене
    /// </summary>
    public class ProductsList : Message
    {
        private readonly List<Product> _products = null;

        /// <summary>
        /// Отправитель данных
        /// </summary>
        public LegalEntity DataSource { get; private set; }

        /// <summary>
        /// Получатель данных
        /// </summary>
        public LegalEntity DataRecipient { get; private set; }

        public ProductsList(long id, string number, byte version, DateTime date,
            LegalEntity sender, LegalEntity recipient, LegalEntity dataSource, LegalEntity dataRecipient, string note = "")
            : base(id, number, 'I', "PRODUCTS_LIST", version, date, sender, recipient, note)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            DataRecipient = dataRecipient ?? throw new ArgumentNullException(nameof(dataRecipient));
            _products = new List<Product>();
        }

        /// <summary>
        /// Добавить товар
        /// </summary>
        /// <param name="product">Товар</param>
        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (product != Product.Null)
                _products.Add(product);
        }

        /// <summary>
        /// Добавить товары
        /// </summary>
        /// <param name="products">Товары</param>
        public void AddProducts(Product[] products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));
            foreach (var product in products)
                AddProduct(product);
        }

        /// <summary>
        /// Товары
        /// </summary>
        public Product[] Products => _products.ToArray();
    }
}