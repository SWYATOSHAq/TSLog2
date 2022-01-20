using TSLog.Common.Data;
using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.Staff.Data
{
    /// <summary>
    ///  Класс для работ с даннми в БД Oracle
    /// </summary>
    public class StaffOracleStorage : OracleStorage, IStaffStorage
    {
        public StaffOracleStorage(string dataSource) : base(dataSource)
        { }
        public Employee AddEmployeePosition(Employee employee, Position position)
        {
            throw new NotImplementedException();
        }
        public Employee AddPartnerManager(Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }
        public Employee CreateEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }
        public void DeleteEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }
        public Employee EditEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }
        public Employee RemoveEmployeePosition(Employee employee, Position position)
        {
            throw new NotImplementedException();
        }
        public Employee RemovePartnerManager(Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }
        public Employee SetEmployeeDepartment(Employee employee)
        {
            throw new NotImplementedException();
        }
    }
}