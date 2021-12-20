using TSLog.Common.BO;

namespace TSLog.Staff.BO
{
    public class Employee : Entity
    {
        public Employee(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", bool isActive = true) 
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        { }
        private Employee() : base()
        { }
        private static Employee? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new Employee Null
        {
            get
            {
                if (_null == null)
                    _null = new Employee(-1, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }
        private static Employee? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Employee Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Employee();
                return _empty;
            }
        }
    }
}