using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Логист
    /// </summary>
    public class Logist : Entity
    {
        public Logist(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", bool isActive = true)
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        { }
        private Logist() : base()
        {
            
        }
        private static Logist? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new Logist Null
        {
            get
            {
                if (_null == null)
                    _null = new Logist(-1, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }
        private static Logist? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Logist Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Logist();
                return _empty;
            }
        }
    }
}
