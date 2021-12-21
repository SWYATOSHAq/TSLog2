namespace TSLog.Common.BO
{
    /// <summary>
    /// Единица измерения
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={6512FD8B-4D58-49B0-9A36-AF01F2EED2FF}
    /// </summary>
    public class MeasureUnit : Entity
    {
        /// <summary>
        /// Размерность единицы измерения (объем, вес, длина и т. д.)
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Признак базовой Е. И.
        /// </summary>
        public bool IsBasic { get; private set; }

        /// <summary>
        /// Коэффициент преобразования: определяет номер этой единицы измерения, который соответствует базовой единице измерения.
        /// </summary>
        public double Coefficient { get; private set; }

        /// <summary>
        /// Базовая единица измерения, коэффициент преобразования которой равен 1
        /// </summary>
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Базовая Е. И.
        /// </summary>
        public MeasureUnit BaseUnit { get; set; }

        public MeasureUnit(long id, string code, string name, string shortName, string note,
            string category, bool isBasic, double coefficient, bool isDefault = false)
            : base(id, code, name, shortName, string.Empty, string.Empty, note)
        {
            Category = !string.IsNullOrWhiteSpace(category) ? category.Trim() : string.Empty;
            IsBasic = isBasic;
            Coefficient = coefficient > 0.0 ? coefficient : 1.0;
            IsDefault = isDefault;
        }
        private MeasureUnit() : base()
        {
            Category = string.Empty;
            IsBasic = false;
            Coefficient = 1.0;
            IsDefault = false;
        }

        private static MeasureUnit _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public new static MeasureUnit Null
        {
            get
            {
                if (_null == null)
                    _null = new MeasureUnit(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, 1.0);
                return _null;
            }
        }
        private static MeasureUnit _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new MeasureUnit Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new MeasureUnit();
                return _empty;
            }
        }
    }
}