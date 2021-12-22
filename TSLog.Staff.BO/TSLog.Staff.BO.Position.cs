using TSLog.Common.BO;

namespace TSLog.Staff.BO
{
    /// <summary>
    /// Должность
    /// </summary>
    public class Position : Entity
    {
        public Position(short id, string code, string name = "", string shortName = "", string altName = "", string displayName = "",
            string note = "") : base(id, code, name, shortName, altName, displayName, note)
        { }
        private Position() : base()
        { }

        private static Position? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Position Null
        {
            get
            {
                if (_null == null)
                    _null = new Position(-1, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }
        private static Position? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Position Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Position();
                return _empty;
            }
        }
    }
}
