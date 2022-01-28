using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.Staff.Data
{
    /// <summary>
    ///  Класс для работ с даннми в БД Oracle
    /// </summary>
    public class StaffOracleStorage : OracleStorage, IStaffStorage
    {
        private const string DefaultLegalEntitiesSchema = "staff";
        public string LegalEntitiesSchema { private get; set; } = DefaultLegalEntitiesSchema;

        private readonly Catalog<LegalEntity> _legalEntities = new("LEGAL_ENTITIES", "Организации");
        private static LegalEntity CreateLegalEntity(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToString(reader["id"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var diplayName = Utils.DbValueToString(reader["display_name"]);
            var info1 = string.Empty;
            var info2 = string.Empty;
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToLong(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var legalEntity = new LegalEntity(id,
                name, shortName, altName, diplayName, info1, info2,
                isActive,
                displayOrder,
                note);
            return legalEntity;
        }
        public LegalEntity GetLegalEntity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("id is empty");
            id = id.Trim().ToUpper();
            var legalEntity = _legalEntities[id];
            if (legalEntity != null && legalEntity != Partner.Null && legalEntity != LegalEntity.Empty)
                return legalEntity;
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"select le.id," +
                    $"       le.name, le.short_name, le.alt_name, le.display_name," +
                    $"       le.is_active, le.display_order," +
                    $"       le.note" +
                    $"  from {LegalEntitiesSchema}.le_legal_entity#v le " +
                    $" where upper(trim(le.id)) = :id";
                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    legalEntity = CreateLegalEntity(reader);
                    _legalEntities.Add(legalEntity);
                }
                reader.Close();
            }
            if (legalEntity != null && legalEntity != LegalEntity.Null && legalEntity != LegalEntity.Empty)
                return legalEntity;
            throw new LegalEntityNotFoundException(id);
        }

        private const string DefaultStaffSchema = "staff";
        public string StaffSchema { private get; set; } = DefaultStaffSchema;
        public StaffOracleStorage(string dataSource) : base(dataSource)
        { }       
        public Employee AddEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var AddEmployeeSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.create_employee(:p_sex," +
                    $"        :p_surname," +
                    $"        :p_first_name," +
                    $"        :p_patronymic," +
                    $"        :p_birth_date," +
                    $"        :p_nationality," +
                    $"        :p_phone_number," +
                    $"        :p_email," +
                    $"        :p_document_name," +
                    $"        :p_document_series," +
                    $"        :p_document_number," +
                    $"        :p_document_issue_date," +
                    $"        :p_document_issue_organization," +
                    $"        :p_employment_date," +
                    $"        :p_dismissal_date," +
                    $"        :p_company_id," +
                    $"        :p_department_id," +
                    $"        :p_cost," +
                    $"        :p_cost_currency," +
                    $"        :p_work_mode," +
                    $"        :p_note);" +
                    $"end;";
                command.CommandText = AddEmployeeSql;
                command.Parameters.Add(new OracleParameter("p_sex", employee.Sex));
                command.Parameters.Add(new OracleParameter("p_surname", employee.Surname.Trim()));
                command.Parameters.Add(new OracleParameter("p_first_name", employee.FirstName.Trim()));
                command.Parameters.Add(new OracleParameter("p_patronymic", 
                    !string.IsNullOrWhiteSpace(employee.Patronymic) ? employee.Patronymic.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_birth_date", employee.BirthDate));
                command.Parameters.Add(new OracleParameter("p_nationality", employee.Nationality.Trim()));
                command.Parameters.Add(new OracleParameter("p_phone_number", 
                    !string.IsNullOrWhiteSpace(employee.PhoneNumber) ? employee.PhoneNumber.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_email", 
                    !string.IsNullOrWhiteSpace(employee.Email) ? employee.Email.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_name", 
                    !string.IsNullOrWhiteSpace(employee.DocumentName) ? employee.DocumentName.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_series", 
                    !string.IsNullOrWhiteSpace(employee.DocumentSeries) ? employee.DocumentSeries.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_number", 
                    !string.IsNullOrWhiteSpace(employee.DocumentNumber) ? employee.DocumentNumber.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_issue_date", 
                    employee.DocumentIssueDate != DateTime.MinValue ? employee.DocumentIssueDate : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_issue_organization", 
                    !string.IsNullOrWhiteSpace(employee.DocumentIssueOrganization) ? employee.DocumentIssueOrganization.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_employment_date", 
                    employee.EmploymentDate != DateTime.MinValue ? employee.EmploymentDate : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_dismissal_date", 
                    employee.DismissalDate != DateTime.MinValue ? employee.DismissalDate : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_company_id", employee.Company.Id));
                command.Parameters.Add(new OracleParameter("p_department_id", 
                    employee.Department != null && employee.Department != Department.Null && employee.Department != Department.Empty 
                    ? employee.Department.Id : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_cost", employee.Cost));
                command.Parameters.Add(new OracleParameter("p_cost_currency", employee.CostMeasureUnit.Code));
                command.Parameters.Add(new OracleParameter("p_work_mode", employee.WorkMode.Trim()));
                command.Parameters.Add(new OracleParameter("p_note", 
                    !string.IsNullOrWhiteSpace(employee.Note) ? employee.Note.Trim() : DBNull.Value));
                var id = command.ExecuteScalar();
                employee.Id = (int)id;
            }
            return employee;
        }


        private readonly Catalog<Employee> _employees = new("EMPLOYEES", "Сотрудники");
        private Employee CreateEmployee(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt64(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var surname = Utils.DbValueToString(reader["surname"]);
            var firstName = Utils.DbValueToString(reader["first_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var birthDate = Utils.DbValueToDateTime(reader["birth_date"]);
            var companyId = Utils.DbValueToString(reader["company_id"]);
            var company = GetLegalEntity(companyId);
            var note = Utils.DbValueToString(reader["note"]);

            var employee = new Employee(id, code,
                surname, firstName,
                birthDate,
                company,
                isActive,
                note);
            employee.Patronymic = Utils.DbValueToString(reader["patronymic"]);
            employee.Sex = Utils.DbValueToString(reader["sex"]);
            employee.Nationality = Utils.DbValueToString(reader["nationality"]);
            employee.PhoneNumber = Utils.DbValueToString(reader["phone_number"]);
            employee.Email = Utils.DbValueToString(reader["email"]);

            employee.DocumentName = Utils.DbValueToString(reader["document_name"]);
            employee.DocumentSeries = Utils.DbValueToString(reader["document_series"]);
            employee.DocumentNumber = Utils.DbValueToString(reader["document_number"]);
            employee.DocumentIssueDate = Utils.DbValueToDateTime(reader["document_issue_date"]);
            employee.DocumentIssueOrganization = Utils.DbValueToString(reader["document_issue_organization"]);

            employee.EmploymentDate = Utils.DbValueToDateTime(reader["employment_date"]);
            employee.DismissalDate = Utils.DbValueToDateTime(reader["dismissal_date"]);

            //var departmentId = Utils.DbValueToInt(reader["department_id"]);
            //employee.Department = GetDepartment(departmentId);

            employee.Cost = Utils.DbValueToDouble(reader["cost"]);
            var costCurrencyCode = Utils.DbValueToString(reader["cost_currency_code"]);
            employee.CostMeasureUnit = GetMeasureUnit(costCurrencyCode);

            return employee;
        }
        public Employee GetEmployee(int id)
        {
            id = id > 0 ? id : 0;
            var employee = _employees[id];
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById =
                    $"select e.id, e.code," +
                    $"       e.surname, e.first_name, e.patronymic," +
                    $"       e.sex, e.birth_date," +
                    $"       e.nationality," +
                    $"       e.phone_number, e.email," +
                    $"       e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization," +
                    $"       e.employment_date, e.dismissal_date," +
                    $"       e.company_id," +
                    $"       e.department_id," +
                    $"       e.cost," +
                    $"       e.cost_currency_code," +
                    $"       e.is_active," +
                    $"       e.display_order," +
                    $"       e.note" +
                    $"  from {StaffSchema}.stf_employee#v e " +
                    $" where e.id = :id";
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    employee = CreateEmployee(reader);
                    _employees.Add(employee);
                }
                reader.Close();
            }
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            throw new EmployeeNotFoundException(id);
        }
        public Employee[] FindEmployees(string pattern)
        {
            pattern = string.IsNullOrWhiteSpace(pattern) ? "%" : $"%{pattern.ToUpper()}%";
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var FindEmployeesSql =
                    $"select e.id, e.code," +
                    $"       e.surname, e.first_name, e.patronymic," +
                    $"       e.sex, e.birth_date," +
                    $"       e.nationality," +
                    $"       e.phone_number, e.email," +
                    $"       e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization," +
                    $"       e.employment_date, e.dismissal_date," +
                    $"       e.company_id," +
                    $"       e.department_id," +
                    $"       e.cost," +
                    $"       e.cost_currency_code," +
                    $"       e.is_active," +
                    $"       e.display_order," +
                    $"       e.note" +
                    $"  from {StaffSchema}.stf_employee_ext#v e " +
                    $" where upper(trim(e.surname)) like :pattern" +
                    $"    or upper(trim(e.first_name)) like :pattern" +
                    $"    or upper(trim(e.patronymic)) like :pattern" +
                    $"    or upper(trim(e.short_name)) like :pattern" +
                    $"    or upper(trim(e.short_name2)) like :pattern" +
                    $"    or upper(trim(e.short_name3)) like :pattern" +
                    $"    or upper(trim(e.short_name4)) like :pattern" +
                    $"    or upper(trim(e.alt_name)) like :pattern" +
                    $"    or upper(trim(e.alt_name2)) like :pattern" +
                    $"    or upper(trim(e.alt_name3)) like :pattern" +
                    $"    or upper(trim(e.alt_name4)) like :pattern" +
                    $"    or upper(trim(e.full_name)) like :pattern" +
                    $"    or upper(trim(e.full_name2)) like :pattern";
                command.CommandText = FindEmployeesSql;
                command.Parameters.Add(new OracleParameter("pattern", pattern));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        public Employee[] FindEmployees(Position position, string pattern, string workMode = "")
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            pattern = string.IsNullOrWhiteSpace(pattern) ? "%" : $"%{pattern.ToUpper()}%";
            var employees = new HashSet<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById =
                    $"select ep.employee_id" +
                    $"  from {StaffSchema}.stf_employee_position_ext#v ep " +
                    $" where ep.position_id = :position_id" +
                    $"   and (:work_mode is null or upper(trim(ep.work_mode)) = :work_mode) " +
                    $"   and (upper(trim(ep.employee_surname)) like :pattern" +
                    $"     or upper(trim(ep.employee_first_name)) like :pattern" +
                    $"     or upper(trim(ep.employee_patronymic)) like :pattern" +
                    $"     or upper(trim(ep.employee_short_name)) like :pattern" +
                    $"     or upper(trim(ep.employee_short_name2)) like :pattern" +
                    $"     or upper(trim(ep.employee_short_name3)) like :pattern" +
                    $"     or upper(trim(ep.employee_short_name4)) like :pattern" +
                    $"     or upper(trim(ep.employee_alt_name)) like :pattern" +
                    $"     or upper(trim(ep.employee_alt_name2)) like :pattern" +
                    $"     or upper(trim(ep.employee_alt_name3)) like :pattern" +
                    $"     or upper(trim(ep.employee_alt_name4)) like :pattern" +
                    $"     or upper(trim(ep.employee_full_name)) like :pattern" +
                    $"     or upper(trim(ep.employee_full_name2)) like :pattern)";
                command.BindByName = true;
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("position_id", position.Id));

                var workModeParameter = new OracleParameter
                {
                    ParameterName = "work_mode",
                    OracleDbType = OracleDbType.NVarchar2,
                    Value = !string.IsNullOrWhiteSpace(workMode) ? workMode.Trim().ToUpper() : DBNull.Value
                };

                command.Parameters.Add(new OracleParameter("pattern", pattern));
                command.Parameters.Add(workModeParameter);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employeeId = Utils.DbValueToInt(reader["employee_id"]);
                    var employee = GetEmployee(employeeId);
                    if (!employees.Contains(employee))
                        employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        public Employee[] GetEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"select e.id, e.code," +
                    $"       e.surname, e.first_name, e.patronymic," +
                    $"       e.sex, e.birth_date," +
                    $"       e.nationality," +
                    $"       e.phone_number, e.email," +
                    $"       e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization," +
                    $"       e.employment_date, e.dismissal_date," +
                    $"       e.company_id," +
                    $"       e.department_id," +
                    $"       e.cost," +
                    $"       e.cost_currency_code," +
                    $"       e.is_active," +
                    $"       e.display_order," +
                    $"       e.note" +
                    $"  from {StaffSchema}.stf_employee#v e";
                command.CommandText = SelectEmployeesSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }

        public Employee EditEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var EditEmployeeSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.edit_employee(:p_id," +
                    $"        :p_sex," +
                    $"        :p_surname," +
                    $"        :p_first_name," +
                    $"        :p_patronymic," +
                    $"        :p_birth_date," +
                    $"        :p_nationality," +
                    $"        :p_phone_number," +
                    $"        :p_email," +
                    $"        :p_document_name," +
                    $"        :p_document_series," +
                    $"        :p_document_number," +
                    $"        :p_document_issue_date," +
                    $"        :p_document_issue_organization," +
                    $"        :p_employment_date," +
                    $"        :p_dismissal_date," +
                    $"        :p_company_id," +
                    $"        :p_department_id," +
                    $"        :p_cost," +
                    $"        :p_cost_currency," +
                    $"        :p_work_mode," +
                    $"        :p_note);" +
                    $"end;";
                command.CommandText = EditEmployeeSql;
                command.Parameters.Add(new OracleParameter("p_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_sex", employee.Sex));
                command.Parameters.Add(new OracleParameter("p_surname", employee.Surname.Trim()));
                command.Parameters.Add(new OracleParameter("p_first_name", employee.FirstName.Trim()));
                command.Parameters.Add(new OracleParameter("p_patronymic",
                    !string.IsNullOrWhiteSpace(employee.Patronymic) ? employee.Patronymic.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_birth_date", employee.BirthDate));
                command.Parameters.Add(new OracleParameter("p_nationality", employee.Nationality.Trim()));
                command.Parameters.Add(new OracleParameter("p_phone_number",
                    !string.IsNullOrWhiteSpace(employee.PhoneNumber) ? employee.PhoneNumber.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_email",
                    !string.IsNullOrWhiteSpace(employee.Email) ? employee.Email.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_name",
                    !string.IsNullOrWhiteSpace(employee.DocumentName) ? employee.DocumentName.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_series",
                    !string.IsNullOrWhiteSpace(employee.DocumentSeries) ? employee.DocumentSeries.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_number",
                    !string.IsNullOrWhiteSpace(employee.DocumentNumber) ? employee.DocumentNumber.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_issue_date",
                    employee.DocumentIssueDate != DateTime.MinValue ? employee.DocumentIssueDate : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_document_issue_organization",
                    !string.IsNullOrWhiteSpace(employee.DocumentIssueOrganization) ? employee.DocumentIssueOrganization.Trim() : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_employment_date",
                    employee.EmploymentDate != DateTime.MinValue ? employee.EmploymentDate : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_dismissal_date",
                    employee.DismissalDate != DateTime.MinValue ? employee.DismissalDate : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_company_id", employee.Company.Id));
                command.Parameters.Add(new OracleParameter("p_department_id",
                    employee.Department != null && employee.Department != Department.Null && employee.Department != Department.Empty
                    ? employee.Department.Id : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_cost", employee.Cost));
                command.Parameters.Add(new OracleParameter("p_cost_currency", employee.CostMeasureUnit.Code));
                command.Parameters.Add(new OracleParameter("p_work_mode", employee.WorkMode.Trim()));
                command.Parameters.Add(new OracleParameter("p_note",
                    !string.IsNullOrWhiteSpace(employee.Note) ? employee.Note.Trim() : DBNull.Value));
                command.ExecuteScalar();
            }
            return employee;
        }
        public Employee SetEmployeeDepartment(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var SetEmployeeDepartmentSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.set_employee_department(:p_id, :p_department_id);" +
                    $"end;";
                command.CommandText = SetEmployeeDepartmentSql;
                command.Parameters.Add(new OracleParameter("p_id", employee.Id));
                command.Parameters.Add(new OracleParameter("id", 
                    employee.Department != null && employee.Department != Department.Null && employee.Department != Department.Empty 
                    ? employee.Department.Id : DBNull.Value));
                command.ExecuteScalar();
            }
            return employee;
        }
        public void DeleteEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using var command = _connection.CreateCommand();
            var DeleteEmployeeSql =
                $"begin" +
                $"    {StaffSchema}.api_staff.delete_employee(:p_id);" +
                $"end;";
            command.CommandText = DeleteEmployeeSql;
            command.Parameters.Add(new OracleParameter("id", employee.Id));
            command.ExecuteScalar();
        }
        public Employee AddEmployeePosition(Employee employee, Position position)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var AddEmployeePositionSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.add_employee_position(:p_employee_id, :p_position_id);" +
                    $"end;";
                command.CommandText = AddEmployeePositionSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_position_id", position.Id));
                command.ExecuteScalar();
            }
            return employee;
        }
        public Employee RemoveEmployeePosition(Employee employee, Position position)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var RemoveEmployeePositionSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.remove_employee_position(:p_employee_id, :p_position_id);" +
                    $"end;";
                command.CommandText = RemoveEmployeePositionSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_position_id", position.Id));
                command.ExecuteScalar();
            }
            return employee;
        }
        public Employee AddPartnerManager(Employee employee, Partner partner)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var AddPartnerManagerSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.add_partner_manager(:p_employee_id, :p_partner_id);" +
                    $"end;";
                command.CommandText = AddPartnerManagerSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_partner_id", partner.Id));
                command.ExecuteScalar();
            }
            return employee;
        }
        public Employee RemovePartnerManager(Employee employee, Partner partner)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                var RemovePartnerManagerSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.remove_partner_manager(:p_employee_id, :p_partner_id);" +
                    $"end;";
                command.CommandText = RemovePartnerManagerSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_partner_id", partner.Id));
                command.ExecuteScalar();
            }
            return employee;
        }

        public Shift AddShift(Shift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            using (var command = _connection.CreateCommand())
            {
                var AddShiftSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.create_shift(:p_from_date, :p_to_date, :p_work_mode, :p_note);";
                command.CommandText = AddShiftSql;
                command.Parameters.Add(new OracleParameter("p_from_date", shift.From));
                command.Parameters.Add(new OracleParameter("p_to_date", shift.To));
                command.Parameters.Add(new OracleParameter("p_work_mode", shift.WorkMode));
                command.Parameters.Add(new OracleParameter("p_note", shift.Note));
                var id = command.ExecuteScalar();
                shift.Id = (int)id;
            }
            return shift;
        }
        public Shift EditShift(Shift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            using (var command = _connection.CreateCommand())
            {
                var EditShiftSql =
                    $"declare" +
                    $"    l_id number(8);" +
                    $"begin" +
                    $"    l_id := {StaffSchema}.api_staff.edit_shift(:p_id, :p_from_date, :p_to_date, :p_work_mode, :p_note);" +
                    $"end;";
                command.CommandText = EditShiftSql;
                command.Parameters.Add(new OracleParameter("p_id", shift.Id));
                command.Parameters.Add(new OracleParameter("p_from_date", shift.From));
                command.Parameters.Add(new OracleParameter("p_to_date", shift.To));
                command.Parameters.Add(new OracleParameter("p_work_mode", shift.WorkMode));
                command.Parameters.Add(new OracleParameter("p_note", shift.Note));
                command.ExecuteScalar();
            }
            return shift;
        }
        public void AddEmployeeToShift(Shift shift, Employee employee)        
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using var command = _connection.CreateCommand();
            var AddEmployeeToShiftSql =
                $"declare" +
                $"    l_id number(8);" +
                $"begin" +
                $"    l_id := {StaffSchema}.api_staff.add_employee_to_shift(:p_employee_id, :p_shift_id);" +
                $"end;";
            command.CommandText = AddEmployeeToShiftSql;
            command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
            command.Parameters.Add(new OracleParameter("p_shift_id", shift.Id));
            command.ExecuteScalar();
        }
        public void RemoveEmployeeFromShift(Shift shift, Employee employee)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using var command = _connection.CreateCommand();
            var RemoveEmployeeFromShiftSql =
                $"declare" +
                $"    l_id number(8);" +
                $"begin" +
                $"    l_id := {StaffSchema}.api_staff.remove_employee_from_shift(:p_employee_id, :p_shift_id);" +
                $"end;";
            command.CommandText = RemoveEmployeeFromShiftSql;
            command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
            command.Parameters.Add(new OracleParameter("p_shift_id", shift.Id));
            command.ExecuteScalar();
        }
    }
}