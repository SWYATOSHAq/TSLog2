using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Data
{
    /// <summary>
    /// Класс для работ с даннми в БД Oracle
    /// </summary>
    public class YardOracleStorage : OracleStorage, IYardWmsStorage
    {
        private const string DefaultStaffSchema = "staff";
        private const string DefaultLegalEntitiesSchema = "legal_entities";
        private const string DefaultPartnersSchema = "partners";
        /// <summary>
        /// Схема для объектов учёта персонала
        /// </summary>
        public string StaffSchema { protected get; set; } = DefaultStaffSchema;
        /// <summary>
        /// Схема для организаций (юр. лиц)
        /// </summary>
        public string LegalEntitieSchema { protected get; set; } = DefaultLegalEntitiesSchema;
        /// <summary>
        /// Схема для партнёров (покупателей наших складских и логистических услуг)
        /// </summary>
        public string PartnersSchema { protected get; set; } = DefaultPartnersSchema;
        public YardOracleStorage(string dataSource) : base(dataSource)
        { }

        private WarehouseAction CreateAction(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var kind = Convert.ToString(reader["kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var id = Utils.DbValueToString(reader["id"]);
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var partnerActionId = Utils.DbValueToString(reader["partner_number"]);
            var action = new WarehouseAction(kind,
                id,
                partner,
                partnerActionId);
            return action;
        }
        public WarehouseAction GetAction(ActionKind actionKind, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("action id is empty");
            id = id.Trim();
            var action = WarehouseAction.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectWarehouseActionByIdSql =
                    $"select a.kind, a.id, a.partner_id, a.partner_number " +
                    $"  from {CommonSchema}.act_warehouse_action#v a " +
                    $" where a.id = :id " +
                    $"   and a.action_kind = :action_kind";
                command.CommandText = SelectWarehouseActionByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                command.Parameters.Add(new OracleParameter("action_kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    action = CreateAction(reader);
                reader.Close();
            }
            if (action != null && action != WarehouseAction.Null && action != WarehouseAction.Empty)
                return action;
            throw new WarehouseActionNotFoundException(id);
        }
        public WarehouseAction GetAction(ActionKind actionKind, Partner partner, string partnerActionId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            partnerActionId = partnerActionId.Trim();
            var action = WarehouseAction.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectWarehouseActionByPartnerActionIdSql =
                    $"select a.kind, a.id, a.partner_id, a.partner_number " +
                    $"  from {CommonSchema}.act_warehouse_action#v a " +
                    $" where a.partner_number = :partner_number " +
                    $"   and a.partner_id = :partner_id" +
                    $"   and a.kind = :kind";
                command.CommandText = SelectWarehouseActionByPartnerActionIdSql;
                command.Parameters.Add(new OracleParameter("partner_number", partnerActionId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                command.Parameters.Add(new OracleParameter("kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    action = CreateAction(reader);
                reader.Close();
            }
            if (action != null && action != WarehouseAction.Null && action != WarehouseAction.Empty)
                return action;
            throw new WarehouseActionNotFoundException(partner.Id, partnerActionId);
        }
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
                    $"  from {LegalEntitieSchema}.le_legal_entity#v le " +
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
        private readonly Catalog<Partner> _partners = new("PARTNERS", "Партнёры");
        private static Partner CreatePartner(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToString(reader["id"]);
            var name = Utils.DbValueToString(reader["name"]);
            var isActive = Utils.DbValueToLong(reader["display_order"]) < 9999;
            var displayOrder = Utils.DbValueToLong(reader["display_order"]);
            var partner = new Partner(id,
                name,
                name, // note
                string.Empty,
                string.Empty,
                isActive,
                displayOrder);
            return partner;
        }
        public Partner GetPartner(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("id is empty");
            id = id.Trim().ToUpper();
            var partner = _partners[id];
            if (partner != null && partner != Partner.Null && partner != Partner.Empty)
                return partner;
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"select p.id, p.name, p.display_order " +
                    $"  from {PartnersSchema}.par_partner#v p " +
                    $" where upper(trim(p.id)) = :id";
                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    partner = CreatePartner(reader);
                    _partners.Add(partner);
                }
                reader.Close();
            }
            if (partner != null && partner != Partner.Null && partner != Partner.Empty)
                return partner;
            throw new PartnerNotFoundException(id);
        }
        public Employee[] GetPartnerManagers(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));

            var managers = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerManagersSql =
                    $"select pm.manager_id " +
                    $"  from {StaffSchema}.stf_partner_manager#v pm " +
                    $" where pm.partner_id = :partner_id";
                command.CommandText = SelectPartnerManagersSql;
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var managerId = Utils.DbValueToShort(reader["manager_id"]);
                    var manager = GetEmployee(managerId);
                    managers.Add(manager);
                }
                reader.Close();
            }
            return managers.ToArray();
        }
        public Partner[] GetPartners(bool getOnlyActive = true)
        {
            var partners = new List<Partner>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnersSql =
                    $"select p.id, p.name, p.display_order " +
                    $"  from {PartnersSchema}.par_partner#v p";
                command.CommandText = SelectPartnersSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var partner = CreatePartner(reader);
                    if (!getOnlyActive || partner.IsActive)
                        partners.Add(partner);
                }
                reader.Close();
            }
            partners.Sort();
            return partners.ToArray();
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

            var departmentId = Utils.DbValueToInt(reader["department_id"]);
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
        public Employee[] GetShiftEmployees(DateTime date)
        {
            var employees = new List<Employee>();
            return employees.ToArray();
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
                var SelectDepartmentByIdSql =
                    $"select d.id, d.code, " +
                    $"       d.name, d.short_name, d.alt_name, d.display_name," +
                    $"       d.is_active, d.display_order," +
                    $"       d.parent_id, d.company_id, d.gead_id," +
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
                var SelectPositionsSql =
                    $"select p.id, p.code, " +
                    $"       p.name, p.short_name, p.alt_name, p.display_name, " +
                    $"       p.is_active, p.display_order, " +
                    $"       p.is_linear, " +
                    $"       p.note " +
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
    }
}