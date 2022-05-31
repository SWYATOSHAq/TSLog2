using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using System.Dynamic;
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
        public string LegalEntitiesSchema { protected get; set; } = DefaultLegalEntitiesSchema;
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
            var number = string.Empty;
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var partnerActionId = Utils.DbValueToString(reader["partner_number"]);
            var isActive = true;
            var note = string.Empty;
            var action = new WarehouseAction(kind,
                id,
                number,
                partner,
                partnerActionId,
                isActive,
                note);
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
                    $"select a.kind, a.id, a.partner_id, a.partner_number\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where a.id = :id\n" +
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
                    $"select a.kind, a.id, a.partner_id, a.partner_number\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where a.partner_number = :partner_number\n" +
                    $"   and a.partner_id = :partner_id\n" +
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
        public WarehouseAction[] FindActions(ActionKind kind, Partner? partner, string pattern)
        {
            var actions = new List<WarehouseAction>();

            pattern = !string.IsNullOrWhiteSpace(pattern) ? $"%{pattern.Trim().ToUpper()}%" : "%";
            using (var command = _connection.CreateCommand())
            {
                var SelectWarehouseActionByPartnerActionIdSql =
                    $"select a.kind,\n" +
                    $" a.id, a.action_number,\n" +
                    $" a.partner_id,\n" +
                    $" a.partner_number\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where (upper(trim(a.id)) like :pattern\n" +
                    $"     or upper(trim(a.action_number)) like :pattern)\n" +
                    $"     or upper(trim(a.partner_number)) like :pattern)\n" +
                    $"   and (:partner_id is null or a.partner_id = :partner_id)\n" +
                    $"   and a.kind = :kind\n" +
                    $"   and rownum < 16";
                command.CommandText = SelectWarehouseActionByPartnerActionIdSql;
                command.Parameters.Add(new OracleParameter("pattern", pattern));
                command.Parameters.Add(new OracleParameter("partner_id",
                    partner != null && partner != Partner.Null && partner != Partner.Empty ? partner.Id : DBNull.Value));
                command.Parameters.Add(new OracleParameter("kind", kind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var action = CreateAction(reader);
                    actions.Add(action);
                }
                reader.Close();
            }
            return actions.ToArray();
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
                    $"select le.id,\n" +
                    $"       le.name, le.short_name, le.alt_name, le.display_name,\n" +
                    $"       le.is_active, le.display_order,\n" +
                    $"       le.note\n" +
                    $"  from {LegalEntitiesSchema}.le_legal_entity#v le\n" +
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
        private readonly IDictionary<string, string> _siteCodes = new Dictionary<string, string>
        {
            { "EIN01", "GATE_1" },
            { "EIN02", "GATE_2" },
            { "EIN03", "GATE_3" },
            { "EIN04", "GATE_4" },
            { "EIN05", "GATE_5" },
            { "EIN06", "GATE_6" },
            { "EIN07", "GATE_7" },
            { "EIN08", "GATE_8" },
            { "EIN09", "GATE_9" },
            { "EIN10", "GATE_10" },
            { "EIN11", "GATE_11" },
            { "EIN12", "GATE_12" },
            { "EIN13", "GATE_13" },
            { "EIN14", "GATE_14" },
            { "EIN15", "GATE_15" },
            { "EIN16", "GATE_16" }
        };
        public string GetSiteCodeByGateCode(string gateCode) 
        {
            return !string.IsNullOrWhiteSpace(gateCode) && _siteCodes.ContainsKey(gateCode) ? _siteCodes[gateCode] : string.Empty;
        }

        #region Partners
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
                    $"select p.id, p.name, p.display_order\n" +
                    $"  from {PartnersSchema}.par_partner#v p\n" +
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
                    $"select pm.manager_id\n" +
                    $"  from {StaffSchema}.stf_partner_manager#v pm\n" +
                    $" where pm.partner_id = :partner_id";
                command.CommandText = SelectPartnerManagersSql;
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var managerId = Utils.DbValueToInt(reader["manager_id"]);
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
                    $"select p.id, p.name, p.display_order\n" +
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
        #endregion

        #region Shifts
        private const string DefaultShift = "5/2-40";
        private static Shift CreateShift(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            return null;
        }
        private string? GetShiftKind(DateTime? date = null)
        {
            var shift = DefaultShift;
            using (var command = _connection.CreateCommand())
            {
                var SelectShiftSql =
                    $"select {StaffSchema}.api_staff.get_shift(:p_date) shift from dual";
                command.CommandText = SelectShiftSql;
                command.Parameters.Add(new OracleParameter("p_date", date != null && date != DateTime.MinValue
                    ? date : DBNull.Value));
                var result = command.ExecuteScalar();
                shift = result != null ? Convert.ToString(result) : DefaultShift;
            }
            return shift;
        }
        public Shift GetShift(int id)
        {
            return null;
        }
        public Shift GetShift(DateTime date)
        {
            return null;
        }
        #endregion

        #region Employees
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
            employee.WmsLogin = Utils.DbValueToString(reader["wms_login"]);
            employee.AdLogin = Utils.DbValueToString(reader["ad_login"]);
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

            var positionId = Utils.DbValueToShort(reader["position_id"]);
            employee.MajorPosition = positionId > 0 ? GetPosition(positionId) : Position.Null;

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
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where e.id = :id";
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
        public Employee GetEmployeeByAdLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login is empty!");
            login = login.Trim().ToLower();
            var employees = _employees.Entities.ToList();
            var employee = employees.Find(e => e.AdLogin.Trim().ToLower() == login);
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectEmployeeById =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where e.ad_login = :ad_login";
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("ad_login", login));
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
            throw new EmployeeNotFoundException("AD login", login);
        }
        public Employee[] FindEmployees(string pattern)
        {
            pattern = string.IsNullOrWhiteSpace(pattern) ? "%" : $"%{pattern.ToUpper()}%";
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var FindEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee_ext#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where upper(trim(e.surname)) like :pattern\n" +
                    $"       or upper(trim(e.first_name)) like :pattern\n" +
                    $"       or upper(trim(e.patronymic)) like :pattern\n" +
                    $"       or upper(trim(e.short_name)) like :pattern\n" +
                    $"       or upper(trim(e.short_name2)) like :pattern\n" +
                    $"       or upper(trim(e.short_name3)) like :pattern\n" +
                    $"       or upper(trim(e.short_name4)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name2)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name3)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name4)) like :pattern\n" +
                    $"       or upper(trim(e.full_name)) like :pattern\n" +
                    $"       or upper(trim(e.full_name2)) like :pattern";
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
                    $"select ep.employee_id\n" +
                    $"  from {StaffSchema}.stf_employee_position_ext#v ep\n" +
                    $" where ep.position_id = :position_id\n" +
                    $"   and ep.dismissal_date is null\n" +
                    $"   and (:work_mode is null or upper(trim(ep.work_mode)) = :work_mode)\n" +
                    $"   and (upper(trim(ep.employee_surname)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_first_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_patronymic)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name2)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name3)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name4)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name2)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name3)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name4)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_full_name)) like :pattern\n" +
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
        public Employee[] GetLinearEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id" +
                    $"    where emp.is_linear = 1";
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
        public Employee[] GetEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id";
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
        public Employee[] GetPlanShiftEmployees(DateTime? date = null)
        {
            var employees = new List<Employee>();
            var shift = GetShiftKind(date);
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where upper(trim(e.work_mode)) = :shift";
                command.CommandText = SelectEmployeesSql;
                command.Parameters.Add(new OracleParameter("shift", shift));
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
        public Employee[] GetShiftEmployees(Shift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectShiftEmployeesSql =
                    $"select se.id employee_id\n" +
                    $"  from {StaffSchema}.stf_shift_employee#v se\n" +
                    $" where se.shift_id = :shift_id";
                command.CommandText = SelectShiftEmployeesSql;
                command.Parameters.Add(new OracleParameter("shift_id", shift.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employeeId = Utils.DbValueToInt(reader["employee_id"]);
                    var employee = GetEmployee(employeeId);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        public Employee[] GetShiftEmployees(DateTime date)
        {
            var shift = GetShift(date);
            return GetShiftEmployees(shift);
        }
        public dynamic[] GetBossControlWorks(DateTime startDate, DateTime finishDate, string siteCode)
        {
            var works = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"with dates as (\n" +
                    $"   select :start_date fact_work_start_date,\n" +
                    $"          :finish_date fact_work_finish_date\n" +
                    $"     from dual\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"    select emp.position_id, emp.position_name,\n" +
                    $"           d.worker_id, e.full_name worker_full_name,\n" +
                    $"           greatest(d.start_date, dt.fact_work_start_date) start_date,\n" +
                    $"           least(d.finish_date, dt.fact_work_finish_date) finish_date,\n" +
                    $"           least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date) duration, -- Продолжительность\n" +
                    $"           d.site_id, s.name site_name\n" +
                    $"      from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $" left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $" left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $" left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"     where d.finish_date - d.start_date > 0\n" +
                    $"       and s.code <> 'AUXILIARY'\n" +
                    $"       and s.code = :site_code\n" +
                    $"       and s.kind = 'GATE' -- Только ворота\n" +
                    $"       and e.id is not null\n" +
                    $" )\n" +
                    $"    select d.position_id, d.position_name,\n" +
                    $"           d.worker_id, d.worker_full_name, \n" +
                    $"           d.site_id, d.site_name,\n" +
                    $"           d.start_date, d.finish_date,\n" +
                    $"           round(d.duration * 24 * 60 * 60) duration\n" +
                    $"      from data2 d\n" +
                    $"  order by d.position_id, d.worker_id, d.worker_full_name";

                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", startDate));
                command.Parameters.Add(new OracleParameter("finish_date", finishDate));
                command.Parameters.Add(new OracleParameter("site_code", siteCode));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dynamic work = new ExpandoObject();
                    work.workerId = Utils.DbValueToInt(reader["worker_id"]);
                    work.workerFullName = Utils.DbValueToString(reader["worker_full_name"]);
                    work.positionName = Utils.DbValueToString(reader["position_name"]);
                    work.startDate = Utils.DbValueToDateTime(reader["start_date"]);
                    work.finishDate = Utils.DbValueToDateTime(reader["finish_date"]);
                    work.duration = Utils.DbValueToDouble(reader["duration"]);
                    works.Add(work);
                }
                reader.Close();
            }
            return works.ToArray();
        }
        #endregion

        #region Departments
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
                    $"select d.id, d.code,\n" +
                    $"       d.name, d.short_name, d.alt_name, d.display_name,\n" +
                    $"       d.is_active, d.display_order,\n" +
                    $"       d.parent_id, d.company_id, d.gead_id,\n" +
                    $"       d.note\n" +
                    $"  from {StaffSchema}.stf_department#v d\n" +
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
                    $"select d.id, d.code,\n" +
                    $"       d.name, d.short_name, d.alt_name, d.display_name,\n" +
                    $"       d.is_active, d.display_order,\n" +
                    $"       d.parent_id, d.company_id, d.gead_id,\n" +
                    $"       d.note\n" +
                    $"  from {StaffSchema}.stf_department#v d\n" +
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
                    $"select d.id, d.code,\n" +
                    $"       d.name, d.short_name, d.alt_name, d.display_name,\n" +
                    $"       d.is_active, d.display_order,\n" +
                    $"       d.parent_id, d.company_id, d.gead_id,\n" +
                    $"       d.note\n" +
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
        #endregion

        #region Positions
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
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_position#v p\n" +
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
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_position#v p\n" +
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
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.note\n" +
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
        public Position[] GetLinePositions()
        {
            var positions = new List<Position>();
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_line_staff#v p";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var position = CreatePosition(reader);
                    positions.Add(position);
                }
                reader.Close();
            }
            return positions.ToArray();
        }
        #endregion

        #region Reporting
        public dynamic[] GetBcLabocCosts(DateTime fromDate, DateTime toDate)
        {
            var lines = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"   select--d.worker_id employee_id,\n" +
                    $"          --emp.employee_surname, emp.employee_first_name, emp.employee_patronymic,\n" +
                    $"          d.partner_id, p.name partner_name,\n" +
                    $"          emp.position_id, emp.position_name,\n" +
                    $"          --Количество работников(различных)\n" +
                    $"          count(distinct d.worker_id) workers_count,\n" +
                    $"          --d.start_date, d.finish_date,\n" +
                    $"          --Трудозатраты в человекочасах\n" +
                    $"          round(sum((d.finish_date - d.start_date) * 24), 2) labor_cost,\n" +
                    $"          --Нормализованные трудозатраты(суммарные / количество работников) в человекочасах\n" +
                    $"          round(sum((d.finish_date - d.start_date) * 24) / count(distinct d.worker_id), 2) norm_labor_cost\n" +
                    $"          --d.site_id\n" +
                    $"     from table({StaffSchema}.stf_get_all_works(:from_date, :to_date)) d\n" +                    
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                    $"    where d.site_id in (select id from {StaffSchema}.stf_site#v connect by prior id = parent_id start with code = 'GROUND_FLOOR')\n" +
                    $"      and emp.employee_id is not null\n" +
                    $" group by emp.position_id, emp.position_name,\n" +
                    $"          d.partner_id, p.name\n" +
                    $" order by d.partner_id, emp.position_id";

                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("from_date", fromDate));
                command.Parameters.Add(new OracleParameter("to_date", toDate));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dynamic line = new ExpandoObject();
                    line.PartnerId = Utils.DbValueToString(reader["partner_id"]);
                    line.PositionId = Utils.DbValueToShort(reader["position_id"]);
                    line.WorkersCount = Utils.DbValueToInt(reader["workers_count"]);
                    line.LaborCost = Utils.DbValueToDouble(reader["labor_cost"]);
                    lines.Add(line);
                }
                reader.Close();
            }
            return lines.ToArray();
        }
        public dynamic[] GetBcShiftLabocCosts(DateTime date)
        {
            var lines = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"with data2 as (\n" +
                    $"   select {StaffSchema}.get_shift_start(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')) shift_start,\n" +
                    $"          {StaffSchema}.get_shift_finish(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')) shift_finish,\n" +
                    $"          d.worker_id, e.full_name worker_full_name,\n" +
                    $"          emp.position_id worker_position_id, emp.position_name worker_position_name,\n" +
                    $"          e.work_mode worker_work_mode,\n" +
                    $"          case \n" +
                    $"              when e.work_mode = '5/2-40' then 8.0\n" +
                    $"              when e.work_mode in ('I', 'II', 'III', 'IV') then 10.5\n" +
                    $"              else 8.0\n" +
                    $"          end worker_standard,\n" +
                    $"          sum((d.finish_date - d.start_date) * 24) worker_labor_cost\n" +
                    $"     from table({StaffSchema}.stf_get_all_works({StaffSchema}.get_shift_start(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')),\n" +
                    $"                                       {StaffSchema}.get_shift_finish(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')))) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                    $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"    where d.finish_date - d.start_date > 0\n" +
                    $"          and e.id is not null\n" +
                    $"      and d.site_id in (select id from {StaffSchema}.stf_site#v connect by prior id = parent_id start with code = 'GROUND_FLOOR')\n" +
                    $" group by d.worker_id, e.full_name, e.work_mode, emp.position_id, emp.position_name)\n" +
                    $"   select d.worker_position_id, d.worker_position_name,\n" +
                    $"          round(sum(d.worker_labor_cost / d.worker_standard), 4) labor_cost, --Человекосмены\n" +
                    $"          round(sum(d.worker_labor_cost) / sum(d.worker_standard) * 100, 2) efficiency,\n" +
                    $"          count(distinct d.worker_id) standard_labor_cost-- Человекосмены\n" +
                    $"    from data2 d\n" +
                    $"left join {StaffSchema}.stf_position#v p on d.worker_position_id = p.id\n" +
                    $"    where p.is_linear = 1\n" +
                    $" group by d.worker_position_id, d.worker_position_name";

                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("shift_date", date.ToString()));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dynamic line = new ExpandoObject();
                    line.PositionId = Utils.DbValueToShort(reader["worker_position_id"]);
                    line.StandardLaborCost = Utils.DbValueToDouble(reader["standard_labor_cost"]);
                    line.LaborCost = Utils.DbValueToDouble(reader["labor_cost"]);
                    lines.Add(line);
                }
                reader.Close();
            }
            return lines.ToArray();
        }
        public dynamic[] GetBcData(DateTime fromDate, DateTime toDate, string siteCode)
        {
            var works = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =                                         
                    $"with dates as (\n" +
                    $"  select :start_date fact_work_start_date,\n" +
                    $"         :finish_date fact_work_finish_date\n" +
                    $"    from dual\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"    select emp.position_id, emp.position_name,\n" +
                    $"           d.worker_id, e.full_name worker_full_name,\n" +
                    $"           greatest(d.start_date, dt.fact_work_start_date) start_date,\n" +
                    $"           least(d.finish_date, dt.fact_work_finish_date) finish_date,\n" +
                    $"           round((least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date)) * 24 * 60 * 60) duration, -- Продолжительность\n" +
                    $"           d.site_id, s.name site_name\n" +
                    $"      from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $" left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $" left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $" left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"     where d.finish_date - d.start_date > 0\n" +
                    $"       and s.code <> 'AUXILIARY'\n" +
                    $"       and s.code = :site_code\n" +
                    $"       and s.kind = 'GATE' -- Только ворота\n" +
                    $"       and e.id is not null\n" +
                    $" )\n" +
                    $"   select *\n" +
                    $"     from data2 d\n" +
                    $" order by d.position_id, d.worker_id, d.worker_full_name, d.start_date, d.finish_date";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", fromDate));
                command.Parameters.Add(new OracleParameter("finish_date", toDate));
                command.Parameters.Add(new OracleParameter("site_code", siteCode));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var duration = Utils.DbValueToInt(reader["duration"]);
                    if (duration <= 0)
                        continue;

                    dynamic work = new ExpandoObject();
                    work.workerId = Utils.DbValueToInt(reader["worker_id"]);
                    work.workerFullName = Utils.DbValueToString(reader["worker_full_name"]);
                    work.positionId = Utils.DbValueToShort(reader["position_id"]);
                    work.positionName = Utils.DbValueToString(reader["position_name"]);
                    work.startDate = Utils.DbValueToDateTime(reader["start_date"]);
                    work.finishDate = Utils.DbValueToDateTime(reader["finish_date"]);
                    work.duration = duration;
                    works.Add(work);
                }
                reader.Close();
            }
            return works.ToArray();
        }
        #endregion
    }
}