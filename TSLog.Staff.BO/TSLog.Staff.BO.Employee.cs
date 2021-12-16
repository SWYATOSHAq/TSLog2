using TSLog.Common.BO;

namespace TSLog.Staff.BO
{
    public class Employee : Entity
    {
        public Employee(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", bool isActive = true) 
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        { }

        public static new Employee Null
        {
            get
            {
                return null;
            }
        }
    }
}