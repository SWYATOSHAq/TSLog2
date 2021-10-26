namespace TSLog.BO
{
    /// <summary>
    /// Единица измерения
    /// </summary>
    public class MeasureUnit : Entity
    {
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={6512FD8B-4D58-49B0-9A36-AF01F2EED2FF}
        /// <summary>
        /// Размерность единицы измерения (объем, вес, длина и т. д.)
        /// </summary>
        public string Category { get; private set; }
        /// <summary>
        /// Коэффициент преобразования: определяет номер этой единицы измерения, который соответствует базовой единице измерения.
        /// </summary>
        public double Coefficient { get; private set; }
        /// <summary>
        /// Базовая единица измерения, коэффициент преобразования которой равен 1
        /// </summary>
        public MeasureUnit BaseUnit { get; set; }

        private static MeasureUnit _null = null;
        public MeasureUnit(long id, string code, string name = "", string note = "")
            : base(id, code, name, note)
        {
            //
        }
        /// <summary>
        /// NULL object
        /// </summary>
        new public static MeasureUnit Null
        {
            get
            {
                if (_null == null)
                    _null = new MeasureUnit(0, "NULl", "Заглушка");
                return _null;
            }
        }
    }
}
