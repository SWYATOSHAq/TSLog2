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
        /// Количество в единицах измерения (ВСЕГДА фактическое)
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Единица измерения
        /// </summary>
        public MeasureUnit MeasureUnit { get; set; }

        public LineBase(int number, Product product, double quantity, MeasureUnit measureUnit)
        {
            Number = number > 0 ? number : -1;
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = quantity > 0.0 ? quantity : 0.0;
            MeasureUnit = measureUnit ?? throw new ArgumentNullException(nameof(measureUnit));
        }
    }
}