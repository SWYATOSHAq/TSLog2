using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
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
    public class BossControlOracleStorage : OracleStorage, IBossControlStorage
    {
        private const string DefaultStaffSchema = "staff";
        private const string DefaultLegalEntitiesSchema = "legal_entities";
        private const string DefaultPartnersSchema = "partners";
        public string StaffSchema { get; private set; } = DefaultStaffSchema;
        public string LegalEntitiesSchema { get; private set; } = DefaultLegalEntitiesSchema;
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
                    $"select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"       e.surname, e.first_name, e.patronymic,\n" +
                    $"       e.sex, e.birth_date,\n" +
                    $"       e.nationality,\n" +
                    $"       e.phone_number, e.email,\n" +
                    $"       e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"       e.employment_date, e.dismissal_date,\n" +
                    $"       e.company_id,\n" +
                    $"       e.department_id,\n" +
                    $"       e.cost,\n" +
                    $"       e.cost_currency_code,\n" +
                    $"       e.is_active,\n" +
                    $"       e.display_order,\n" +
                    $"       e.note\n" +
                    $"  from {StaffSchema}.stf_employee#v e\n" +
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
                var SelectPositionsSql =
                    $"select sw.worker_id,\n" +
                    $"       sw.surname, sw.first_name, sw.patronymic,\n" +
                    $"       sw.site_id, sw.site_name,\n" +
                    $"       sw.partner_id, sw.partner_name,\n" +
                    $"       sw.last_date\n" +
                    $"  from {StaffSchema}.stf_busy_worker#v sw";
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
        public Employee[] GetFreeWorkers()
        {
            var workers = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectPositionsSql =
                    $"select sw.worker_id,\n" +
                    $"       sw.surname, sw.first_name, sw.patronymic\n" +
                    $"  from {StaffSchema}.stf_free_worker#v sw";
                command.CommandText = SelectPositionsSql;
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
                    "  where sw.last_date >= ecco.get_shift_start(0, sysdate) + 1 / 96\n" +
                    "    and sw.last_date <= ecco.get_shift_finish(0, sysdate) + 1 / 96";
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
        public void EnterSite(Employee worker, Site site, Partner partner)
        { }
        public void LeaveSite(Employee worker)
        { }
    }
}
