using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Товарная позиция
    /// </summary>
    public class LineBase
    {
        public int Number { get; private set; }
        public Product Product { get; private set; }
        public double Quantity { get; set; }
        public string measureUnitCode { get; private set; }

        public List<LogisticUnit> _containers = null;

        public LineBase(int number, Product product, double quantity = 0.0)
        {
            Number = number;
            Product = product;
            Quantity = quantity;
            _containers = new List<LogisticUnit>();
        }
    }
}
