using System;

namespace TSLog.BO
{
    /// <summary>
    /// Базовый класс для товарной позиции (прихода, расхода, ...)
    /// </summary>
    public class LineBase
    {
        /// <summary>
        /// номер строки
        /// </summary>
        public int Number { get; private set; }
        /// <summary>
        /// Товар
        /// </summary>
        public Product Product { get; private set; }
        /// <summary>
        /// Количество в единицах измерения
        /// </summary>
        public double Quantity { get; set; }
        /// <summary>
        /// Единица измерения
        /// </summary>
        public MeasureUnit MeasureUnit { get; private set; }

        public LineBase(int number, Product product, double quantity, MeasureUnit measureUnit)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (measureUnit == null)
                throw new ArgumentNullException(nameof(measureUnit));
            Number = number > 0 ? number : -1;
            Product = product;
            Quantity = quantity > 0.0 ? quantity : 0.0;
            MeasureUnit = measureUnit;
        }
    }
}
