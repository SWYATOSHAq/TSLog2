using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Ворота для ПРР
    /// </summary>
    public class Gate : Entity
    {
        /// <summary>
        /// Код участка из БоссКонтроль
        /// </summary>
        public string SiteCode { get; set; } = string.Empty;
        /// <summary>
        /// Признак наличия "Кальяна" на поротах
        /// </summary>
        public bool IsHookan { get; set; } = false;
        public Gate(long id, string code,
            string name = "", string shortName = "", string altName = "", string displayName = "",
            bool isActive = true, short displayOrder = -1,
            string note = "") 
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        { }
        private Gate() : base()
        { }

        private static Gate? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Gate Null
        {
            get
            {
                if (_null == null)
                    _null = new Gate(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, "NULL");
                return _null;
            }
        }
        private static Gate? _empty = null;
        /// <summary>
        /// Empty object
        /// </summary>
        public static new Gate Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Gate();
                return _empty;
            }
        }
    }
}