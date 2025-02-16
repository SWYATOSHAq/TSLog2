using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;

namespace TSLog.Staff.BO;

public interface IStaffStorage
{
    // Get list.
    public Department[] GetDepartments();
    public Employee[] GetEmployees();
    public LegalEntity[] GetEmployers();
    public Position[] GetPositions();
    public Shift[] GetShifts(DateTime from, DateTime to);
    public Employee[] FindEmployees(string pattern);

    // Get by id / code.
    public LegalEntity GetLegalEntity(string id);
    public Position GetPosition(short id);
    public Position GetPosition(string code);
    public Department GetDepartment(short id);
    public Department GetDepartment(string code);
    public Shift GetShift(int id);

    // Add.
    public Employee AddEmployee(Employee employee);
    public Position AddPosition(Position position);
    public Department AddDepartment(Department department);
    public Shift AddShift(Shift shift);

    // Edit.
    public Position EditPosition(Position position);
    public Department EditDepartment(Department department);
    public Employee EditEmployee(Employee employee);
    public Shift EditShift(Shift shift);

    // Delete.
    public void DeleteDepartment(Department department);
    public void DeleteEmployee(Employee employee);
    public void DeleteShift(Shift shift);

    // Special.
    public Employee SetEmployeeDepartment(Employee employee, Department deparment);
    public Employee SetEmployeeDepartment(Employee employee);
    public Employee RemoveEmployeePosition(Employee employee, Position position);
    public Employee AddPartnerManager(Employee employee, Partner partner);
    public Employee RemovePartnerManager(Employee employee, Partner partner);
    public void AddEmployeeToShift(Shift shift, Employee employee);
    public void RemoveEmployeeFromShift(Shift shift, Employee employee);
    public Employee AddEmployeePosition(Employee employee, Position position);
}
