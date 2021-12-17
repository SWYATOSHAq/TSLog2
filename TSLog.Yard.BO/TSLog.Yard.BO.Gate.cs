using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Ворота для ПРР
    /// </summary>
    public class Gate : Entity
    {
        public Gate(long id, string code, 
            string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", 
            bool isActive = true) : base(id, code, name, shortName, altName, displayName, note, isActive)
        { }


        private static Gate _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new Gate Null
        {
            get
            {
                if (_null == null)
                    _null = new Gate(-1, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }
    }
}
