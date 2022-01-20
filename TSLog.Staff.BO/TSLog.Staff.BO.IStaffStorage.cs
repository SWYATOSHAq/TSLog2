using TSLog.Partners.BO;

namespace TSLog.Staff.BO
{
    public interface IStaffStorage
    {
        public Employee CreateEmployee(Employee employee);
        public Employee SetEmployeeDepartment(Employee employee);
        public Employee AddEmployeePosition(Employee employee, Position position);
        public Employee RemoveEmployeePosition(Employee employee, Position position);
        public Employee AddPartnerManager(Employee employee, Partner partner);
        public Employee RemovePartnerManager(Employee employee, Partner partner);
        public Employee EditEmployee(Employee employee);
        public void DeleteEmployee(Employee employee);
    }
}
