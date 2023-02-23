using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Ворота для ПРР
    /// </summary>
    public class Gate : Entity
    {
        public enum GateState { Free, Reserved, Busy}
        /// <summary>
        /// Код участка из БоссКонтроль
        /// </summary>
        public string SiteCode { get; set; } = string.Empty;
        /// <summary>
        /// Признак наличия "Кальяна" на воротах.
        /// </summary>
        public bool IsHookan { get; private set; } = false;
        /// <summary>
        /// Состояние ворот: свободны, зарезервированы, заняты (может быть как на момент времени так и на какой то временной интервал).
        /// </summary>
        public GateState State { get; set; } = GateState.Free;
        public Gate(long id, string code,
            string name = "", string shortName = "", string altName = "", string displayName = "",
            bool isActive = true, short displayOrder = -1,
            bool isHookan = false, GateState state = GateState.Free,
            string note = "") 
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            IsHookan = isHookan;
            State = state;
        }
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
                    _null = new Gate(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, false,  GateState.Free, "NULL");
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