using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
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
        private const string DefaultStaffSchema = "staff";
        public string StaffSchema { private get; set; } = DefaultStaffSchema;
        public StaffOracleStorage(string dataSource) : base(dataSource)
        { }


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


        private readonly Catalog<Position> _positions = new("POSITIONS", "Должности");
        private Position CreatePosition(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt16(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var isLinear = Utils.DbValueToShort(reader["is_linear"]) == 1;
            var note = Utils.DbValueToString(reader["note"]);
            var position = new Position(id,
                code,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                isLinear,
                note)
            {
                LaborHourCost = Utils.DbValueToDouble(reader["cost"])
            };
            var costCurrencyCode = Utils.DbValueToString(reader["cost_currency_code"]);
            position.LaborHourCostCurrency = GetMeasureUnit(costCurrencyCode);
            return position;
        }
        public Position GetPosition(short id)
        {
            id = id > 0 ? id : (short)-1;
            var position = _positions[id];
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;

            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectPositionByIdSql =
                    $"select p.id, p.code," +
                    $"       p.name, p.short_name, p.alt_name, p.display_name," +
                    $"       p.is_active, p.display_order, " +
                    $"       p.is_linear," +
                    $"       p.cost, p.cost_currency_code," +
                    $"       p.note " +
                    $"  from {StaffSchema}.stf_position#v p " +
                    $" where p.id = :id";
                command.CommandText = SelectPositionByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    position = CreatePosition(reader);
                    _positions.Add(position);
                }
                reader.Close();
            }
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;
            throw new PositionNotFoundException(id);
        }
        public Position GetPosition(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var position = _positions[code];
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;

            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectPositionByCodeSql =
                    $"select p.id, p.code, " +
                    $"       p.name, p.short_name, p.alt_name, p.display_name," +
                    $"       p.is_active, p.display_order," +
                    $"       p.is_linear," +
                    $"       p.cost, p.cost_currency_code," +
                    $"       p.note " +
                    $"  from {StaffSchema}.stf_position#v p " +
                    $" where p.code = :code";
                command.CommandText = SelectPositionByCodeSql;
                command.Parameters.Add(new OracleParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    position = CreatePosition(reader);
                    _positions.Add(position);
                }
                reader.Close();
            }
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;
            throw new PositionNotFoundException(code);
        }
        public Position[] GetPositions()
        {
            var positions = new List<Position>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectPositionsSql =
                    $"select p.id, p.code, " +
                    $"       p.name, p.short_name, p.alt_name, p.display_name, " +
                    $"       p.is_active, p.display_order, " +
                    $"       p.is_linear, " +
                    $"       p.cost, p.cost_currency_code," +
                    $"       p.note" +
                    $"  from {StaffSchema}.stf_position#v p";
                command.CommandText = SelectPositionsSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var position = CreatePosition(reader);
                    positions.Add(position);
                }
                reader.Close();
            }
            positions.Sort();
            return positions.ToArray();
        }
        public Position AddPosition(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var AddPositionSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.create_position(:p_code," +
                    $"        :p_name, :p_short_name, :p_alt_name, :p_display_name," +
                    $"        :p_is_active, :p_display_order," +
                    $"        :p_cost, :p_cost_currency," +
                    $"        :p_note);" +
                    $"end;";
                command.CommandText = AddPositionSql;
                command.Parameters.Add(new OracleParameter("p_code", position.Code));
                command.Parameters.Add(new OracleParameter("p_name", position.Name));
                command.Parameters.Add(new OracleParameter("p_short_name", position.ShortName));
                command.Parameters.Add(new OracleParameter("p_alt_name", position.AltName));
                command.Parameters.Add(new OracleParameter("p_display_name", position.DisplayName));
                command.Parameters.Add(new OracleParameter("p_is_active", position.IsActive ? 1 : 0));
                command.Parameters.Add(new OracleParameter("p_display_order", position.DisplayOrder > 0 ? position.DisplayOrder : -1));
                command.Parameters.Add(new OracleParameter("p_cost", position.LaborHourCost));
                command.Parameters.Add(new OracleParameter("p_cost_currency", position.LaborHourCostCurrency.Code));
                command.Parameters.Add(new OracleParameter("p_note", position.Note));
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int16, ParameterDirection.Output));

                command.ExecuteNonQuery();
                var id = (OracleDecimal)command.Parameters["l_id"].Value;
                position.Id = (short)id;
            }
            return position;
        }
        public Position EditPosition(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var EditPositionSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.edit_position(:p_id, :p_code," +
                    $"        :p_name, :p_short_name, :p_alt_name, :p_display_name," +
                    $"        :p_is_active, :p_display_order," +
                    $"        :p_cost, :p_cost_currency," +
                    $"        :p_note);" +
                    $"end;";
                command.CommandText = EditPositionSql;
                command.Parameters.Add(new OracleParameter("p_id", position.Id));
                command.Parameters.Add(new OracleParameter("p_code", position.Code));
                command.Parameters.Add(new OracleParameter("p_name", position.Name));
                command.Parameters.Add(new OracleParameter("p_short_name", position.ShortName));
                command.Parameters.Add(new OracleParameter("p_alt_name", position.AltName));
                command.Parameters.Add(new OracleParameter("p_display_name", position.DisplayName));
                command.Parameters.Add(new OracleParameter("p_is_active", position.IsActive ? 1 : 0));
                command.Parameters.Add(new OracleParameter("p_display_order", position.DisplayOrder > 0 ? position.DisplayOrder : -1));
                command.Parameters.Add(new OracleParameter("p_cost", position.LaborHourCost));
                command.Parameters.Add(new OracleParameter("p_cost_currency", position.LaborHourCostCurrency.Code));
                command.Parameters.Add(new OracleParameter("p_note", position.Note));
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int16, ParameterDirection.Output));
                command.ExecuteScalar();                
            }
            return position;
        }
        public void DeletePosition(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var DeletePositionSql =                
                $"begin" +
                $"    :l_state := {StaffSchema}.api_staff.delete_position(:p_id);" +
                $"end;";
            command.CommandText = DeletePositionSql;
            command.Parameters.Add(new OracleParameter("p_id", position.Id));
            command.Parameters.Add(new OracleParameter("l_state", OracleDbType.Int16, ParameterDirection.Output));
            command.ExecuteScalar();
        }


        private readonly Catalog<Department> _departments = new("DEPARTMENTS", "Подразделения");
        private static Department CreateDepartment(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt16(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var position = new Department(id, code,
                name, shortName, altName, displayName,
                isActive, displayOrder,
                LegalEntity.Null, Employee.Null,
                note);
            return position;
        }
        public Department GetDepartment(short id)
        {
            id = id > 0 ? id : (short)-1;
            var department = _departments[id];
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;

            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectDepartmentByIdSql =
                    $"select d.id, d.code, " +
                    $"       d.name, d.short_name, d.alt_name, d.display_name," +
                    $"       d.is_active, d.display_order," +
                    $"       d.parent_id, d.company_id, d.head_id," +
                    $"       d.note " +
                    $"  from {StaffSchema}.stf_department#v d " +
                    $" where d.id = :id";
                command.CommandText = SelectDepartmentByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    department = CreateDepartment(reader);
                    _departments.Add(department);
                }
                reader.Close();
            }
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;
            throw new DepartmentNotFoundException(id);
        }
        public Department GetDepartment(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var department = _departments[code];
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;

            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectDepartmentByCodeSql =
                    $"select d.id, d.code, " +
                    $"       d.name, d.short_name, d.alt_name, d.display_name," +
                    $"       d.is_active, d.display_order," +
                    $"       d.parent_id, d.company_id, d.gead_id," +
                    $"       d.note " +
                    $"  from {StaffSchema}.stf_department#v d " +
                    $" where d.code = :code";
                command.CommandText = SelectDepartmentByCodeSql;
                command.Parameters.Add(new OracleParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    department = CreateDepartment(reader);
                    _departments.Add(department);
                }
                reader.Close();
            }
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;
            throw new DepartmentNotFoundException(code);
        }
        public Department[] GetDepartments()
        {
            var departments = new List<Department>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectDepartmentsSql =
                   $"select d.id, d.code, " +
                    $"       d.name, d.short_name, d.alt_name, d.display_name," +
                    $"       d.is_active, d.display_order," +
                    $"       d.parent_id, d.company_id, d.gead_id," +
                    $"       d.note " +
                    $"  from {StaffSchema}.stf_department#v d";
                command.CommandText = SelectDepartmentsSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var department = CreateDepartment(reader);
                    departments.Add(department);
                }
                reader.Close();
            }
            departments.Sort();
            return departments.ToArray();
        }
        public Department AddDepartment(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var AddDepartmentSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.create_department(:p_code," +
                    $"        :p_name, :p_short_name, :p_alt_name, :p_display_name," +
                    $"        :p_is_active, :p_display_order," +
                    $"        :p_parent_id," +
                    $"        :p_company_id," +
                    $"        :p_head_id," +
                    $"        :p_note);" +
                    $"end;";
                command.CommandText = AddDepartmentSql;
                command.Parameters.Add(new OracleParameter("p_code", department.Code));
                command.Parameters.Add(new OracleParameter("p_name", department.Name));
                command.Parameters.Add(new OracleParameter("p_short_name", department.ShortName));
                command.Parameters.Add(new OracleParameter("p_alt_name", department.AltName));
                command.Parameters.Add(new OracleParameter("p_display_name", department.DisplayName));
                command.Parameters.Add(new OracleParameter("p_is_active", department.IsActive ? 1 : 0));
                command.Parameters.Add(new OracleParameter("p_display_order", department.DisplayOrder));
                command.Parameters.Add(new OracleParameter("p_parent_id",
                    department.Parent != null && department.Parent != Department.Null && department.Parent != Department.Empty
                    ? department.Parent.Id
                    : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_company_id", department.Company.Id));
                command.Parameters.Add(new OracleParameter("p_head_id", department.Head.Id));
                command.Parameters.Add(new OracleParameter("p_note", department.Note));
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int16, ParameterDirection.Output));

                command.ExecuteNonQuery();
                var id = (OracleDecimal)command.Parameters["l_id"].Value;
                department.Id = (short)id;
            }
            return department;
        }
        public Department EditDepartment(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var EditDepartmentSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.edit_department(:p_id, :p_code," +
                    $"        :p_name, :p_short_name, :p_alt_name, :p_display_name," +
                    $"        :p_is_active, :p_display_order," +
                    $"        :p_parent_id," +
                    $"        :p_company_id," +
                    $"        :p_head_id," +
                    $"        :p_note);" +
                    $"end;";
                command.CommandText = EditDepartmentSql;
                command.Parameters.Add(new OracleParameter("p_id", department.Id));
                command.Parameters.Add(new OracleParameter("p_code", department.Code));
                command.Parameters.Add(new OracleParameter("p_name", department.Name));
                command.Parameters.Add(new OracleParameter("p_short_name", department.ShortName));
                command.Parameters.Add(new OracleParameter("p_alt_name", department.AltName));
                command.Parameters.Add(new OracleParameter("p_display_name", department.DisplayName));
                command.Parameters.Add(new OracleParameter("p_is_active", department.IsActive ? 1 : 0));
                command.Parameters.Add(new OracleParameter("p_display_order", department.DisplayOrder));
                command.Parameters.Add(new OracleParameter("p_parent_id",
                    department.Parent != null && department.Parent != Department.Null && department.Parent != Department.Empty
                    ? department.Parent.Id
                    : DBNull.Value));
                command.Parameters.Add(new OracleParameter("p_company_id", department.Company.Id));
                command.Parameters.Add(new OracleParameter("p_head_id", department.Head.Id));
                command.Parameters.Add(new OracleParameter("p_note", department.Note));
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int16, ParameterDirection.Output));
                command.ExecuteScalar();
            }
            return department;
        }
        public Department SetDepartmentHead(Department department, Employee head)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));
            if (head == null)
                throw new ArgumentNullException(nameof(head));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SetDepartmentHeadSqlSql =
                    $"begin" +
                    $"    :l_department_id := {StaffSchema}.api_staff.set_department_head(:p_department_id, :p_head_id);" +
                    $"end;";
                command.CommandText = SetDepartmentHeadSqlSql;
                command.Parameters.Add(new OracleParameter("p_department_id", department.Id));
                command.Parameters.Add(new OracleParameter("p_head_id", department.Head.Id));
                command.Parameters.Add(new OracleParameter("l_department_id", OracleDbType.Int16, ParameterDirection.Output));
                command.ExecuteNonQuery();
            }
            return department;
        }
        public void DeleteDepartment(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var DeleteDepartmentSql =                
                $"begin" +
                $"    :l_state := {StaffSchema}.api_staff.delete_department(:p_id);" +
                $"end;";
            command.CommandText = DeleteDepartmentSql;
            command.Parameters.Add(new OracleParameter("p_id", department.Id));
            command.Parameters.Add(new OracleParameter("l_state", OracleDbType.Int16, ParameterDirection.Output));
            command.ExecuteScalar();
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
        public Employee GetEmployee(int id)
        {
            id = id > 0 ? id : 0;
            var employee = _employees[id];
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
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
        public Employee AddEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var AddEmployeeSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.create_employee(:p_sex," +
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
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int32, ParameterDirection.Output));

                command.ExecuteNonQuery();
                var id = (OracleDecimal)command.Parameters["l_id"].Value;
                employee.Id = (int)id;
            }
            return employee;
        }
        public Employee EditEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var EditEmployeeSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.edit_employee(:p_id," +
                    $"        :p_sex," +
                    $"        :p_surname, :p_first_name, :p_patronymic," +
                    $"        :p_birth_date," +
                    $"        :p_nationality," +
                    $"        :p_phone_number, :p_email," +
                    $"        :p_document_name, :p_document_series, :p_document_number, :p_document_issue_date, :p_document_issue_organization," +
                    $"        :p_employment_date, :p_dismissal_date," +
                    $"        :p_company_id," +
                    $"        :p_department_id," +
                    $"        :p_cost, :p_cost_currency," +
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
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int32, ParameterDirection.Output));
                command.ExecuteScalar();
            }
            return employee;
        }
        public Employee SetEmployeeDepartment(Employee employee, Department deparment)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (deparment == null)
                throw new ArgumentNullException(nameof(deparment));

            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SetEmployeeDepartmentSql =
                    $"begin" +
                    $"    :l_employee_id := {StaffSchema}.api_staff.set_employee_department(:p_employee_id, :p_department_id);" +
                    $"end;";
                command.CommandText = SetEmployeeDepartmentSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_department_id", deparment.Id));
                command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
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
                command.BindByName = true;
                var SetEmployeeDepartmentSql =
                    $"begin" +
                    $"    :l_employee_id := {StaffSchema}.api_staff.set_employee_department(:p_id, :p_department_id);" +
                    $"end;";
                command.CommandText = SetEmployeeDepartmentSql;
                command.Parameters.Add(new OracleParameter("p_id", employee.Id));
                command.Parameters.Add(new OracleParameter("id",
                    employee.Department != null && employee.Department != Department.Null && employee.Department != Department.Empty
                    ? employee.Department.Id : DBNull.Value));
                command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
                command.ExecuteScalar();
            }
            return employee;
        }
        public Employee AddEmployeePosition(Employee employee, Position position)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var AddEmployeePositionSql =
                    $"begin" +
                    $"    :l_employee_id := {StaffSchema}.api_staff.add_employee_position(:p_employee_id, :p_position_id);" +
                    $"end;";
                command.CommandText = AddEmployeePositionSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_position_id", position.Id));
                command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
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
                command.BindByName = true;
                var RemoveEmployeePositionSql =
                    $"begin" +
                    $"    :l_employee_id := {StaffSchema}.api_staff.remove_employee_position(:p_employee_id, :p_position_id);" +
                    $"end;";
                command.CommandText = RemoveEmployeePositionSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_position_id", position.Id));
                command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
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
                command.BindByName = true;
                var AddPartnerManagerSql =
                    $"begin" +
                    $"    :l_employee_id := {StaffSchema}.api_staff.add_partner_manager(:p_employee_id, :p_partner_id);" +
                    $"end;";
                command.CommandText = AddPartnerManagerSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_partner_id", partner.Id));
                command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
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
                command.BindByName = true;
                var RemovePartnerManagerSql =
                    $"begin" +
                    $"    :l_employee_id := {StaffSchema}.api_staff.remove_partner_manager(:p_employee_id, :p_partner_id);" +
                    $"end;";
                command.CommandText = RemovePartnerManagerSql;
                command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
                command.Parameters.Add(new OracleParameter("p_partner_id", partner.Id));
                command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
                command.ExecuteScalar();
            }
            return employee;
        }
        public void DeleteEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var DeleteEmployeeSql =
                $"begin" +
                $"    :l_state := {StaffSchema}.api_staff.delete_employee(:p_id);" +
                $"end;";
            command.CommandText = DeleteEmployeeSql;
            command.Parameters.Add(new OracleParameter("id", employee.Id));
            command.Parameters.Add(new OracleParameter("l_state", OracleDbType.Int16, ParameterDirection.Output));
            command.ExecuteScalar();
        }

        private static Shift CreateShift(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToInt(reader["id"]);
            var from = Utils.DbValueToDateTime(reader["start_date"]);
            var to = Utils.DbValueToDateTime(reader["finish_date"]);
            var workMode = Utils.DbValueToString(reader["work_mode"]);
            var note = Utils.DbValueToString(reader["note"]);

            var shift = new Shift(id, from, to, workMode, note);
            return shift;
        }
        public Shift GetShift(int id)
        {
            id = id > 0 ? id : 0;
            Shift? shift = null;
            using (var command = _connection.CreateCommand())
            {
                var SelectShiftById =
                    $"select s.id," +
                    $"       s.from_date, s.to_date," +
                    $"       s.work_mode," +
                    $"       s.note" +
                    $"  from {StaffSchema}.stf_shift#v s" +
                    $" where s.id = :id";
                command.CommandText = SelectShiftById;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    shift = CreateShift(reader);
                reader.Close();
            }
            if (shift != null)
                return shift;
            throw new ShiftNotFoundException(id);
        }
        public Shift[] GetShifts(DateTime from, DateTime to)
        {
            var shifts = new List<Shift>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectShiftsSql =
                    $"select s.id," +
                    $"       s.start_date, s.finish_date," +
                    $"       s.work_mode," +
                    $"       s.note" +
                    $"  from {StaffSchema}.stf_shift#v s" +
                    $" where not (s.start_date > :finish_date or s.finish_date < :start_date)";
                command.CommandText = SelectShiftsSql;
                command.Parameters.Add(new OracleParameter(":start_date", from));
                command.Parameters.Add(new OracleParameter(":finish_date", to));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var shift = CreateShift(reader);
                    shifts.Add(shift);
                }
                reader.Close();
            }
            return shifts.ToArray();
        }
        public Shift AddShift(Shift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var AddShiftSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.create_shift(:p_from_date, :p_to_date, :p_work_mode, :p_note);" +
                    $"end;";
                command.CommandText = AddShiftSql;
                command.Parameters.Add(new OracleParameter("p_from_date", shift.From));
                command.Parameters.Add(new OracleParameter("p_to_date", shift.To));
                command.Parameters.Add(new OracleParameter("p_work_mode", shift.WorkMode));
                command.Parameters.Add(new OracleParameter("p_note", shift.Note));
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int32, ParameterDirection.Output));

                command.ExecuteNonQuery();
                var id = (OracleDecimal)command.Parameters["l_id"].Value;
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
                command.BindByName = true;
                var EditShiftSql =
                    $"begin" +
                    $"    :l_id := {StaffSchema}.api_staff.edit_shift(:p_id, :p_from_date, :p_to_date, :p_work_mode, :p_note);" +
                    $"end;";
                command.CommandText = EditShiftSql;
                command.Parameters.Add(new OracleParameter("p_id", shift.Id));
                command.Parameters.Add(new OracleParameter("p_from_date", shift.From));
                command.Parameters.Add(new OracleParameter("p_to_date", shift.To));
                command.Parameters.Add(new OracleParameter("p_work_mode", shift.WorkMode));
                command.Parameters.Add(new OracleParameter("p_note", shift.Note));
                command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int32, ParameterDirection.Output));
                command.ExecuteScalar();

                // TODO: синхронизировать сотрудников
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
            command.BindByName = true;
            var AddEmployeeToShiftSql =
                $"begin" +
                $"    :l_employee_id := {StaffSchema}.api_staff.add_employee_to_shift(:p_employee_id, :p_shift_id);" +
                $"end;";
            command.CommandText = AddEmployeeToShiftSql;
            command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
            command.Parameters.Add(new OracleParameter("p_shift_id", shift.Id));
            command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
            command.ExecuteScalar();
        }
        public void RemoveEmployeeFromShift(Shift shift, Employee employee)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var RemoveEmployeeFromShiftSql =
                $"begin" +
                $"    :l_employee_id := {StaffSchema}.api_staff.remove_employee_from_shift(:p_employee_id, :p_shift_id);" +
                $"end;";
            command.CommandText = RemoveEmployeeFromShiftSql;
            command.Parameters.Add(new OracleParameter("p_employee_id", employee.Id));
            command.Parameters.Add(new OracleParameter("p_shift_id", shift.Id));
            command.Parameters.Add(new OracleParameter("l_employee_id", OracleDbType.Int32, ParameterDirection.Output));
            command.ExecuteScalar();
        }
        public void DeleteShift(Shift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var DeleteShiftSql =
                $"begin" +
                $"    :l_state := {StaffSchema}.api_staff.delete_shift(:p_id);" +
                $"end;";
            command.CommandText = DeleteShiftSql;
            command.Parameters.Add(new OracleParameter("p_id", shift.Id));
            command.Parameters.Add(new OracleParameter("l_state", OracleDbType.Int32, ParameterDirection.Output));
            command.ExecuteScalar();
        }
    }
}