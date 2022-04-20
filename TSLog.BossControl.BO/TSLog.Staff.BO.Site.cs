using TSLog.Common.BO;

namespace TSLog.Staff.BO
{
    public class Site: Entity
    {
        public string Kind { get; set; } = string.Empty;
        public Site? Parent { get; set; } = null;
        public Site(short id, string code, string kind, string name, string shortName, string altName, string displayName, 
            bool isActive, short displayOrder, string note = "")
            :base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            Kind = kind;
        }
        private Site() : base()
        { }

        private static Site? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Site Null
        {
            get
            {
                if (_null == null)
                    _null = new Site(-1, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, "NULL");
                return _null;
            }
        }
        private static Site? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Site Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Site();
                return _empty;
            }
        }
    }
}
