using TSLog.Common.BO;
using TSLog.Products.BO;

namespace TSLog.WarehouseActions.BO
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
        /// Количество в единицах измерения (плановое)
        /// </summary>
        public double PlanQuantity { get; set; }
        /// <summary>
        /// Количество в единицах измерения (фактическое)
        /// </summary>
        public double FactQuantity { get; set; }
        /// <summary>
        /// Единица измерения
        /// </summary>
        public MeasureUnit QuantityMeasureUnit { get; set; }
        private double _planVolume = 0.0;
        private double _factVolume = 0.0;
        private void CalculetaePlanVolume()
        {
            _planVolume = Product.Volume * PlanQuantity; ;
        }
        private void CalculetaeFactVolume()
        {
            _planVolume = Product.Volume * FactQuantity; ;
        }
        /// <summary>
        /// ПЛАНОВЫЙ геометрический объём товара в товарной позиции
        /// </summary>
        public double PlanVolume
        {
            get
            {
                if (_planVolume <= 0.0)
                    CalculetaePlanVolume();
                return _planVolume;
            }
            set
            {
                _planVolume = value;
            }
        }
        /// <summary>
        /// ФАКТИЧЕСКИЙ геометрический объём товара в товарной позиции
        /// </summary>
        public double FactVolume
        {
            get
            {
                if (_factVolume <= 0.0)
                    CalculetaeFactVolume();
                return _factVolume;
            }
            set
            {
                _factVolume = value;
            }
        }

        /// <summary>
        /// Единица измерения объёма
        /// </summary>
        public MeasureUnit VolumeMeasureUnit { get; set; } = MeasureUnit.Null;
        private double _planNetWeight = 0.0;
        private double _factNetWeight = 0.0;
        private double _planGrossWeight = 0.0;
        private double _factGrossWeight = 0.0;
        private void CalculatePlanNetWeight()
        {
            _factNetWeight = Product.NetWeight * PlanQuantity;
        }
        private void CalculateFactNetWeight()
        {
            _factNetWeight = Product.NetWeight * FactQuantity;
        }
        private void CalculatePlanGrossWeight()
        {
            _planGrossWeight = Product.GrossWeight * PlanQuantity;
        }
        private void CalculateFactGrossWeight()
        {
            _factGrossWeight = Product.GrossWeight * FactQuantity;
        }
        /// <summary>
        /// ПЛАНОВЫЙ вес нетто товара в товарной позиции
        /// </summary>
        public double PlanNetWeight
        {
            get
            {
                if (_planNetWeight <= 0)
                    CalculatePlanNetWeight();
                return _planNetWeight;
            }
            set
            {
                _planNetWeight = value;
            }
        }
        /// <summary>
        /// ФАКТИЧЕСКИЙ вес нетто товара в товарной позиции
        /// </summary>
        public double FactNetWeight
        {
            get
            {
                if (_factNetWeight <= 0)
                    CalculateFactNetWeight();
                return _factNetWeight;
            }
            set
            {
                _factNetWeight = value;
            }
        }
        /// <summary>
        /// ПЛАНОВЫЙ вес брутто товара в товарной позиции
        /// </summary>
        public double PlanGrossWeight
        {
            get
            {
                if (_planGrossWeight <= 0)
                    CalculatePlanGrossWeight();
                return _planGrossWeight;
            }
            set
            {
                _planGrossWeight = value;
            }
        }
        /// <summary>
        /// ФАКТИЧЕСКИЙ вес брутто товара в товарной позиции
        /// </summary>
        public double FactGrossWeight
        {
            get 
            {
                if (_factGrossWeight <= 0)
                    CalculateFactGrossWeight();
                return _factGrossWeight;
            }
            set 
            {
                _factGrossWeight = value;
            }
        }
        /// <summary>
        /// Единица измерения веса
        /// </summary>
        public MeasureUnit WeightMeasureUnit { get; set; } = MeasureUnit.Null;
        /// <summary>
        /// Количество единиц измерения на паллете
        /// </summary>
        public double PalletStandard { get; set; } = 0.0;
        /// <summary>
        /// Количество единиц измерения в коробке
        /// </summary>
        public double BoxStandard { get; set; } = 0.0;
        public LineBase(int number, Product product, double planQuantity, double factQuantity, MeasureUnit measureUnit)
        {
            Number = number > 0 ? number : -1;
            Product = product ?? throw new ArgumentNullException(nameof(product));
            PlanQuantity = planQuantity > 0.0 ? planQuantity : 0.0;
            FactQuantity = factQuantity > 0.0 ? factQuantity : 0.0;
            QuantityMeasureUnit = measureUnit ?? throw new ArgumentNullException(nameof(measureUnit));
        }
    }
}