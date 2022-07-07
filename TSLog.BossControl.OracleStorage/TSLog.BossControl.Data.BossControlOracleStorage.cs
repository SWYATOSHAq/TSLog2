using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using TSLog.BossControl.BO;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.Data
{
    /// <summary>
    /// Хранилище данных для объектов БоссКонтроль в БД Oracle
    /// </summary>
    public class BossControlOracleStorage : OracleStorage, IBossControlStorage
    {
        private const string DefaultStaffSchema = "staff";
        private const string DefaultLegalEntitiesSchema = "legal_entities";
        private const string DefaultPartnersSchema = "partners";
        /// <summary>
        /// Схема для хранения объектов БД для учёта персонала
        /// </summary>
        public string StaffSchema { get; private set; } = DefaultStaffSchema;
        /// <summary>
        /// Схема для хранения объектов БД для учёта юр. лиц
        /// </summary>
        public string LegalEntitiesSchema { get; private set; } = DefaultLegalEntitiesSchema;
        /// <summary>
        /// Схема для хранения объектов БД для учёта партнёров (мандантов)
        /// </summary>
        public string PartnersSchema { get; private set; } = DefaultPartnersSchema;
        public BossControlOracleStorage(string dataSource) 
            : base(dataSource)
        { }

        public void Connect(string userName, string password)
        {
            Connect(userName, Utils.StringToSecureString(password));
        }
        public void SetOracleCommonSchema(string commonSchema)
        {
            CommonSchema = commonSchema;
        }
        public void SetOracleStaffSchema(string staffShcema)
        {
            StaffSchema = staffShcema;
        }
        public void SetOracleLegalEntitiesSchema(string legalEntitiesSchema)
        {
            LegalEntitiesSchema = legalEntitiesSchema;
        }
        public void SetOraclePartnersSchema(string partnersSchema)
        {
            PartnersSchema = partnersSchema;
        }

        #region Sites
        private readonly Catalog<Site> _sites = new("SITES", "Участки");
        private static Site CreateSite(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToShort(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var kind = Utils.DbValueToString(reader["kind"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var site = new Site(id,
                code,
                kind,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                note);
            return site;
        }
        public Site GetSite(short id)
        {
            var site = _sites[id];
            if (site != null && site != Site.Null && site != Site.Empty)
                return site;
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"select s.id,"  +
                    $"       s.code,\n" +
                    $"       s.kind,\n" +
                    $"       s.name, s.short_name, s.alt_name, s.display_name,\n" +
                    $"       s.is_active,\n" +
                    $"       s.display_order,\n" +
                    $"       s.parent_id,\n" +
                    $"       s.note\n" +
                    $"  from {StaffSchema}.stf_site#v s\n" + 
                    $" where s.id = :id";
                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    site = CreateSite(reader);
                    _sites.Add(site);
                }
                reader.Close();
            }
            if (site != null && site != Site.Null && site != Site.Empty)
                return site;
            throw new SiteNotFoundException(id);
        }
        public Site[] GetSites()
        {
            var sites = new List<Site>();
            return sites.ToArray();
        }
        #endregion

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
        public Partner[] GetPartners()
        {
            var partners = new List<Partner>();
            return partners.ToArray();
        }
        #endregion

        #region Legal entities
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
        public LegalEntity[] GetCompanies()
        {
            var companies = new List<LegalEntity>();
            return companies.ToArray();
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
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
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

            employee.WorkMode = Utils.DbValueToString(reader["work_mode"]);

            //var departmentId = Utils.DbValueToInt(reader["department_id"]);
            //employee.Department = GetDepartment(departmentId);

            var majorPositionId = Utils.DbValueToShort(reader["major_position_id"]);
            employee.MajorPosition = majorPositionId > 0 ? GetPosition(majorPositionId) : Position.Null;

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
                    $"          e.work_mode,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active," +
                    $"          mp.position_id major_position_id,\n" +
                    $"          e.display_order,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v mp on e.id = mp.employee_id\n" +
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
        #endregion

        #region Workers
        private SiteWorker CreateSiteWorker(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var siteId = Utils.DbValueToShort(reader["site_id"]);
            var site = GetSite(siteId);
            var partnerId = Utils.DbValueToString(reader["partner_id"]);
            var partner = !string.IsNullOrWhiteSpace(partnerId) ? GetPartner(partnerId) : Partner.Null;
            var workerId = Utils.DbValueToInt(reader["worker_id"]);
            var worker = GetEmployee(workerId);
            var startDate = Utils.DbValueToDateTime(reader["last_date"]);

            var siteWorker = new SiteWorker(site, partner, worker, startDate);
            return siteWorker;
        }
        public SiteWorker[] GetBusyWorkers()
        {
            var siteWorkers = new List<SiteWorker>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"select sw.worker_id,\n" +
                    $"       sw.surname, sw.first_name, sw.patronymic,\n" +
                    $"       sw.site_id, sw.site_name,\n" +
                    $"       sw.partner_id, sw.partner_name,\n" +
                    $"       sw.last_date\n" +
                    $"  from {StaffSchema}.stf_busy_worker#v sw";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var siteWorker = CreateSiteWorker(reader);
                    siteWorkers.Add(siteWorker);
                }
                reader.Close();
            }
            siteWorkers.Sort();
            return siteWorkers.ToArray();
        }
        public Employee[] GetFreeWorkers()
        {
            var workers = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"select sw.worker_id,\n" +
                    $"       sw.surname, sw.first_name, sw.patronymic\n" +
                    $"  from {StaffSchema}.stf_free_worker#v sw";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var workerId = Utils.DbValueToInt(reader["worker_id"]);
                    var worker = GetEmployee(workerId);
                    workers.Add(worker);
                }
                reader.Close();
            }
            return workers.ToArray();
        }
        public SiteWorker[] GetFreeShiftWorkers()
        {
            var siteWorkers = new List<SiteWorker>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectPositionsSql =
                    $"select sw.worker_id,\n" +
                    $"       sw.surname, sw.first_name, sw.patronymic,\n" +
                    $"       sw.site_id, sw.site_name,\n" +
                    $"       sw.partner_id, sw.partner_name,\n" +
                    $"       sw.last_date\n" +
                    $"  from {StaffSchema}.stf_free_worker#v sw\n" +
                    $" where sw.last_date >= {StaffSchema}.get_shift_start(0, sysdate) + 1 / 96\n" +
                    $"   and sw.last_date <= {StaffSchema}.get_shift_finish(0, sysdate) + 1 / 96";
                command.CommandText = SelectPositionsSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var siteWorker = CreateSiteWorker(reader);
                    siteWorkers.Add(siteWorker);
                }
                reader.Close();
            }
            siteWorkers.Sort();
            return siteWorkers.ToArray();
        }
        public void EnterSite(Employee worker, Site site, Partner partner, string note = "")
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (site == null)
                throw new ArgumentNullException(nameof(site));
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));

            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var sql =
                $"begin" +
                $"    :l_id := {StaffSchema}.api_staff.enter_site(p_worker_id => :worker_id," +
                $"        p_site_id => :site_id," +
                $"        p_partner_id => :partner_id," +
                $"        p_note => :note);" +
                $"end;";
            command.CommandText = sql;
            command.Parameters.Add(new OracleParameter("worker_id", worker.Id));
            command.Parameters.Add(new OracleParameter("site_id", site.Id));
            command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
            command.Parameters.Add(new OracleParameter("note", string.IsNullOrWhiteSpace(note) ? DBNull.Value : note.Trim()));
            command.ExecuteScalar();
        }
        public void LeaveSite(Employee worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var sql =
                $"begin" +
                $"    :l_id := {StaffSchema}.api_staff.leave_site(p_worker_id => :worker_id);" +
                $"end;";
            command.CommandText = sql;
            command.Parameters.Add(new OracleParameter("worker_id", worker.Id));
            command.Parameters.Add(new OracleParameter("l_id", OracleDbType.Int32, ParameterDirection.Output));
            command.ExecuteNonQuery();
        }
        #endregion

        #region Reporting
        private const int DefaultShiftLength = 12 * 60 * 60;
        public dynamic[] CreatePositionsShiftReport(DateTime date)
        {
            var data = new Dictionary<short, dynamic>();
            using (var workersCommand = _connection.CreateCommand())
            {
                workersCommand.BindByName = true;
                var stringDate = date.ToString("dd.MM.yyyy HH.mm.ss");
                var selectWorkersSql = 
                    $"with data2 as (\n" +
                    $"   select sh.shift_start, sh.shift_finish,\n" +
                    $"          d.worker_id, e.full_name worker_full_name,\n" +
                    $"          emp.position_id worker_position_id, emp.position_name worker_position_name,\n" +
                    $"          e.work_mode worker_work_mode,\n" +
                    $"          case\n" +
                    $"              when e.work_mode = '5/2-40' then 8.0\n" +
                    $"              when e.work_mode in ('I', 'II', 'III', 'IV') then 10.5\n" +
                    $"              else 8.0\n" +
                    $"          end worker_standard,\n" +
                    $"          sum((d.finish_date - d.start_date) * 24) worker_labor_cost,\n" +
                    $"          case\n" +
                    $"              when sum(case\n" +
                    $"                           when not (sh.shift_start + 2/24 >= d.finish_date or sh.shift_start + 2/24 + 10/60/24 <= d.start_date) then 1 -- 11:00 - 11:10\n" +
                    $"                           when not (sh.shift_start + 4/24 >= d.finish_date or sh.shift_start + 5/24 <= d.start_date) then 1 -- 13:00 - 14:00\n" +
                    $"                           when not (sh.shift_start + 7/24 + 30/60/24 >= d.finish_date or sh.shift_start + 7/24 + 40/60/24 <= d.start_date) then 1 -- 16:30 - 16:40\n" +
                    $"                           when not (sh.shift_start + 9/24 >= d.finish_date or sh.shift_start + 9/24 + 35/60/24 <= d.start_date) then 1 -- 18:00 - 18:35\n" +
                    $"                           else 0\n" +
                    $"                       end) > 0 then 1\n" +
                    $"              else 0\n" +
                    $"          end rest_work\n" +
                    $"     from table({StaffSchema}.stf_get_all_works({StaffSchema}.get_shift_start(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')),\n" +
                    $"                                       {StaffSchema}.get_shift_finish(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')))) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                    $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"left join (select {StaffSchema}.get_shift_start(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')) shift_start,\n" +
                    $"                  {StaffSchema}.get_shift_finish(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')) shift_finish\n" +
                    $"           from dual) sh on 1 = 1\n" +
                    $"    where d.finish_date - d.start_date > 0\n" +
                    $"      and s.kind <> 'AUXILIARY'\n" +
                    $" group by d.worker_id, e.full_name, e.work_mode, emp.position_id, emp.position_name, sh.shift_start, sh.shift_finish)\n" +
                    $"   select d.worker_position_id, d.worker_position_name,\n" +
                    $"          d.worker_id, d.worker_full_name,\n" +
                    $"          round(sum(d.worker_labor_cost / d.worker_standard), 2) fact_labor_cost, -- Человекосмены\n" +
                    $"          round(count(distinct d.worker_id), 2) norm_labor_cost, -- Человекосмены\n" +
                    $"          round(sum(d.worker_labor_cost) / sum(d.worker_standard) * 100) efficiency,\n" +
                    $"          case\n" +
                    $"              when round(sum(d.worker_labor_cost) / sum(d.worker_standard) * 100, 2) >= 100.0 then 0.0\n" +
                    $"              else 100.0 - round(sum(d.worker_labor_cost) / sum(d.worker_standard) * 100)\n" +
                    $"          end losses,\n" +
                    $"          d.rest_work\n" +
                    $"     from data2 d\n" +
                    $"left join {StaffSchema}.stf_position#v p on d.worker_position_id = p.id\n" +
                    $"    where p.is_linear = 1\n" +
                    $" group by d.worker_id, d.worker_full_name, d.worker_position_id, d.worker_position_name, d.rest_work\n" +
                    $" order by d.worker_position_id, d.worker_full_name";                

                workersCommand.CommandText = selectWorkersSql;
                workersCommand.Parameters.Add(new OracleParameter("report_date", stringDate));
                using var workersReader = workersCommand.ExecuteReader();
                while (workersReader.Read())
                {
                    var efficiency = Utils.DbValueToDouble(workersReader["efficiency"]);
                    if (efficiency <= 2.56)
                        continue;

                    dynamic workerElement = new ExpandoObject();
                    var positionId = Utils.DbValueToShort(workersReader["worker_position_id"]);
                    
                    workerElement.positionId = positionId;
                    var workerId = Utils.DbValueToInt(workersReader["worker_id"]);
                    workerElement.workerId = workerId;
                    workerElement.workerFullName = Utils.DbValueToString(workersReader["worker_full_name"]);
                    workerElement.factLaborCost = Utils.DbValueToDouble(workersReader["fact_labor_cost"]);
                    workerElement.normLaborCost = Utils.DbValueToDouble(workersReader["norm_labor_cost"]);
                    workerElement.efficiency = Utils.DbValueToDouble(workersReader["efficiency"]);
                    workerElement.losses = Utils.DbValueToDouble(workersReader["losses"]);
                    workerElement.restWork = Utils.DbValueToShort(workersReader["rest_work"]);

                    dynamic? positionElement = null;
                    if (data.ContainsKey(positionId))
                        positionElement = data[positionId];
                    else
                    {
                        positionElement = new ExpandoObject();
                        positionElement.positionId = positionId;
                        positionElement.positionName = Utils.DbValueToString(workersReader["worker_position_name"]);
                        positionElement.factLaborCost = 0.0;
                        positionElement.normLaborCost = 0.0;
                        positionElement.efficiency = 0.0;
                        positionElement.losses = 0.0;
                        positionElement.workers = new List<dynamic>();
                        data.Add(positionId, positionElement);
                    }

                    positionElement.factLaborCost += Utils.DbValueToDouble(workersReader["fact_labor_cost"]);
                    positionElement.normLaborCost += Utils.DbValueToDouble(workersReader["norm_labor_cost"]);

                    workerElement.works = new List<dynamic>();
                    using var worksCommand = _connection.CreateCommand();
                    worksCommand.BindByName = true;

                    var worksSql = 
                        $"with data2 as (\n" +
                        $"   select sh.shift_start, sh.shift_finish,\n" +
                        $"          d.worker_id, e.full_name worker_full_name,\n" +
                        $"          emp.position_id worker_position_id, emp.position_name worker_position_name,\n" +
                        $"          e.work_mode worker_work_mode,\n" +
                        $"          case\n" +
                        $"              when e.work_mode = '5/2-40' then 8.0\n" +
                        $"              when e.work_mode in ('I', 'II', 'III', 'IV') then 10.5\n" +
                        $"              else 8.0\n" +
                        $"          end worker_standard,\n" +
                        $"          d.start_date, d.finish_date,\n" +
                        $"          d.site_id, -- Идентификатор участка\n" +
                        $"          s.name site_name, -- Название участка\n" +
                        $"          p.id partner_id, p.name partner_name,\n" +
                        $"          round(d.finish_date - d.start_date, 8) duration, -- Продолжительность\n" +
                        $"          case\n" +
                        $"              when not (sh.shift_start + 2/24 >= d.finish_date or sh.shift_start + 2/24 + 10/60/24 <= d.start_date) then 1 -- 11:00 - 11:10\n" +
                        $"              when not (sh.shift_start + 4/24 >= d.finish_date or sh.shift_start + 5/24 <= d.start_date) then 1 -- 13:00 - 14:00\n" +
                        $"              when not (sh.shift_start + 7/24 + 30/60/24 >= d.finish_date or sh.shift_start + 7/24 + 40/60/24 <= d.start_date) then 1 -- 16:30 - 16:40\n" +
                        $"              when not (sh.shift_start + 9/24 >= d.finish_date or sh.shift_start + 9/24 + 35/60/24 <= d.start_date) then 1 -- 18:00 - 18:35\n" +
                        $"              else 0\n" +
                        $"          end rest_work\n" +
                        $"     from table({StaffSchema}.stf_get_all_works({StaffSchema}.get_shift_start(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')),\n" +
                        $"                                       {StaffSchema}.get_shift_finish(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')))) d\n" +
                        $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                        $"left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                        $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                        $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                        $"left join(select {StaffSchema}.get_shift_start(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')) shift_start,\n" +
                        $"                  {StaffSchema}.get_shift_finish(0, to_date(:report_date, 'DD.MM.YYYY HH24:MI:SS')) shift_finish\n" +
                        $"             from dual) sh on 1 = 1\n" +
                        $"    where d.finish_date - d.start_date > 0\n" +
                        $"      and s.kind <> 'AUXILIARY'\n" +
                        $" )\n" +
                        $"   select *\n" +
                        $"     from data2 d\n" +
                        $"    where d.worker_id = :worker_id\n" +
                        $" order by d.worker_position_id, d.worker_id, d.worker_full_name, d.start_date, d.finish_date";
                    
                    worksCommand.CommandText = worksSql;
                    worksCommand.Parameters.Add(new OracleParameter("worker_id", workerId));
                    worksCommand.Parameters.Add(new OracleParameter("report_date", stringDate));
                    var worksReader = worksCommand.ExecuteReader();
                    while (worksReader.Read())
                    {
                        dynamic workElement = new ExpandoObject();
                        workElement.workerId = Utils.DbValueToInt(worksReader["worker_id"]);
                        workElement.startDate = Utils.DbValueToDateTime(worksReader["start_date"]);
                        workElement.finishDate = Utils.DbValueToDateTime(worksReader["finish_date"]);
                        workElement.duration = Math.Round(Utils.DbValueToDouble(worksReader["duration"]) * 24 * 60);
                        workElement.siteId = Utils.DbValueToShort(worksReader["site_id"]);
                        workElement.siteName = Utils.DbValueToString(worksReader["site_name"]);
                        workElement.partnerId = Utils.DbValueToString(worksReader["partner_id"]);
                        workElement.partnerName = Utils.DbValueToString(worksReader["partner_name"]);
                        workElement.restWork = Utils.DbValueToShort(worksReader["rest_work"]);
                        workerElement.works.Add(workElement);
                    }
                    worksReader.Close();
                    
                    positionElement.workers.Add(workerElement);
                }
            }

            foreach (var positionElement in data.Values)
            {                
                positionElement.efficiency = positionElement.normLaborCost > 0 ? 
                    Math.Round(100 * positionElement.factLaborCost / positionElement.normLaborCost) : 
                    0;
                positionElement.losses = positionElement.efficiency <= 100.0 ? 100.0 - positionElement.efficiency : 0.0;
                positionElement.factLaborCost = Math.Round(positionElement.factLaborCost, 2);
                positionElement.normLaborCost = Math.Round(positionElement.normLaborCost, 2);
            }
            return data.Values.ToArray();
        }
        public dynamic[] CreateStaffReport(DateTime fromDate, DateTime toDate)
        {
            // Подготовительная часть, берём данные из БД,
            // группируем по сменам работников, с указанием отработанного времени
            var shiftsData = new Dictionary<string, dynamic>();
            var shifts = Shift.GetShifts(fromDate, toDate);
            foreach (var shift in shifts)
            {
                var k = (double)shift.Length / DefaultShiftLength;
                dynamic shiftData = new ExpandoObject();
                shiftData.from = shift.From;
                shiftData.to = shift.To;
                shiftData.length = shift.Length;
                shiftData.k = k;
                using (var command = _connection.CreateCommand())
                {
                    command.BindByName = true;
                    var sql = 
                        $"   select emp.employee_id, emp.employee_surname, emp.employee_first_name, emp.employee_patronymic,\n" +
                        $"          emp.position_id, emp.position_name,\n" +
                        $"          emp.work_mode worker_work_mode, -- Режим работы\n" +
                        $"          case\n" +
                        $"              when emp.work_mode = '5/2-40' then 8.0\n" +
                        $"              when emp.work_mode in ('I', 'II', 'III', 'IV') then 10.5\n" +
                        $"              else 8.0\n" +
                        $"          end worker_standard, -- Нормы выработки\n" +
                        $"          d.start_date, d.finish_date\n" +
                        $"     from table({StaffSchema}.stf_get_all_works(:from_date, :to_date)) d\n" +
                        $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                        $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                        $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                        $"    where s.kind <> 'AUXILIARY' and emp.employee_id is not null\n" +
                        $" order by emp.position_id, emp.employee_id";
                    command.CommandText = sql;
                    command.Parameters.Add(new OracleParameter("from_date", shift.From));
                    command.Parameters.Add(new OracleParameter("to_date", shift.To));
                    using var reader = command.ExecuteReader();
                    var workers = new Dictionary<int, int>();
                    while (reader.Read())
                    {
                        var workerId = Utils.DbValueToInt(reader["employee_id"]);
                        var startTime = Utils.DbValueToDateTime(reader["start_date"]);
                        var finishTime = Utils.DbValueToDateTime(reader["finish_date"]);
                        if (!workers.ContainsKey(workerId))
                            workers.Add(workerId, 0);
                        var span = finishTime - startTime;
                        workers[workerId] += (int)Math.Round(span.TotalSeconds);
                    }                    
                    reader.Close();
                    shiftData.workers = workers;
                }                
                var key = $"{shift.From.ToString("yyyy.MM.dd HH.mm.ss")}-{shift.To.ToString("yyyy.MM.dd HH.mm.ss")}";                
                shiftsData.Add(key, shiftData);
            }

            // Готвим данные на вывод
            var employees = new Dictionary<int, dynamic>();
            foreach (var key in shiftsData.Keys)
            {
                var shiftData = shiftsData[key];
                var shiftWorkers = (Dictionary<int, int>)shiftData.workers;
                foreach (var workerId in shiftWorkers.Keys)
                {
                    var factor = shiftWorkers[workerId] / (double)shiftData.length;
                    if (factor < 0.1)
                        continue;

                    if (!employees.ContainsKey(workerId))
                    {                        
                        var employee = GetEmployee(workerId);
                        
                        dynamic worker = new ExpandoObject();
                        worker.id = workerId;
                        worker.positionId = employee.MajorPosition.Id;
                        worker.surname = employee.Surname;
                        worker.firstName = employee.FirstName;
                        worker.patronymic = employee.Patronymic;
                        worker.fullName = employee.FullName;
                        worker.workMode = employee.WorkMode;
                        worker.shiftLength = employee.ShiftLength * shiftData.k;
                        worker.laborCost = employee.Cost;
                        worker.laborCostCurrencyCode = employee.CostMeasureUnit.Code;
                        worker.planWorkTime = 0;
                        worker.factWorkTime = 0;
                        worker.planCost = 0.0;
                        worker.factCost = 0.0;
                        employees.Add(workerId, worker);
                    }
                    employees[workerId].planWorkTime += employees[workerId].shiftLength;
                    employees[workerId].factWorkTime += shiftWorkers[workerId];
                    employees[workerId].planCost += (double)employees[workerId].shiftLength / 60 / 60 * employees[workerId].laborCost;
                    employees[workerId].factCost += (double)shiftWorkers[workerId] / 60 / 60 * employees[workerId].laborCost;
                }
            }
            var positions = new Dictionary<short, dynamic>();
            foreach (var employee in employees.Values)
            {
                short positionId = employee.positionId;
                if (!positions.ContainsKey(employee.positionId))
                {                    
                    var position = GetPosition(positionId);
                    dynamic positionElement = new ExpandoObject();
                    positionElement.id = positionId;
                    positionElement.code = position.Code;
                    positionElement.name = position.Name;
                    positionElement.planWorkTime = 0;
                    positionElement.factWorkTime = 0;
                    positionElement.planCost = 0.0;
                    positionElement.factCost = 0.0;
                    positionElement.employees = new List<dynamic>();
                    positions.Add(positionId, positionElement);
                }
                positions[positionId].planWorkTime += employee.planWorkTime;
                positions[positionId].factWorkTime += employee.factWorkTime;
                positions[positionId].planCost += employee.planCost;
                positions[positionId].factCost += employee.factCost;
                var positionEmployees = (List<dynamic>)positions[employee.positionId].employees;
                positionEmployees.Add(employee);
            }
            return positions.Values.ToArray();
        }
        #endregion
    }
}