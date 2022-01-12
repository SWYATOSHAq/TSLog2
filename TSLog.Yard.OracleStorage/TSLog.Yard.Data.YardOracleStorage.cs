using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using TSLog.Common;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
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
        private const string DefaultCommonSchema = "ecco";
        /// <summary>
        /// Схема для "общих" объектов БД
        /// </summary>
        public string CommonSchema { private get; set; } = DefaultCommonSchema;
        public YardOracleStorage(string dataSource) : base(dataSource)
        { }

        private WarehouseAction CreateAction(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));            
            var kind = Convert.ToString(reader["kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var id = Utils.DbValueToString(reader["id"]);
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var partnerActionId = Utils.DbValueToString(reader["partner_action_id"]);
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
                    $"select a.id, a.action_kind, a.partner_id " +
                    $"  from {CommonSchema}.v_warehouse_action a " +
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
            if (action != null && action != WarehouseAction.Null)
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
                var SelectWarehouseActionByPartnerActionIdSql = $"select a.id, a.partner_id " +
                    $"from {CommonSchema}.v_warehouse_action a " +
                    $"where a.partner_number = :partner_number " +
                    $"and a.partner_id = :partner_id" +
                    $"and a.action_kind = :action_kind";
                command.CommandText = SelectWarehouseActionByPartnerActionIdSql;
                command.Parameters.Add(new OracleParameter("partner_number", partnerActionId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                command.Parameters.Add(new OracleParameter("action_kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    action = CreateAction(reader);
                reader.Close();
            }
            if (action != null && action != WarehouseAction.Null)
                return action;
            throw new WarehouseActionNotFoundException(partner.Id, partnerActionId);
        }
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
            var partner = Partner.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql = 
                    $"select p.id, p.name, p.display_order " +
                    $"  from {CommonSchema}.v_partner p " +
                    $" where upper(trim(p.id)) = :id";
                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    partner = CreatePartner(reader);
                reader.Close();
            }
            if (partner != null && partner != Partner.Null)
                return partner;
            throw new PartnerNotFoundException(id);
        }
        public Employee[] GetPartnerManagers(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof (partner));

            var managers = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerManagersSql = 
                    $"select pm.manager_id " +
                    $"  from {CommonSchema}.v_partner_manager pm " +
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
        public Partner[] GetPartners(bool getOnlyActive = false)
        {
            var partners = new List<Partner>();
            using (var command = _connection.CreateCommand()) 
            {
                var SelectPartnersSql = 
                    $"select p.id, p.name, p.display_order " +
                    $"  from {CommonSchema}.v_partner p";
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
            return partners.ToArray();
        }
        private static Employee CreateEmployee(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt64(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var employee = new Employee(id,
                code,
                name);
            return employee;
        }
        public Employee GetEmployee(int id) 
        {
            id  = id > 0 ? id : 0;
            var employee = Employee.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById = 
                    $"select e.id, e.code, e.name " +
                    $"  from {CommonSchema}.v_employee e " +
                    $" where e.id = :id";
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    employee = CreateEmployee(reader);
                reader.Close();
            }
            if (employee != null && employee != Employee.Null)
                return employee;
            throw new EmployeeNotFoundException(id);
        }
        public Employee[] GetEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql = 
                    $"select e.id, e.code, e.name " +
                    $"  from {CommonSchema}.v_employee e";
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
        private static Position CreatePosition(DbDataReader reader)
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
            var position = new Position(id,
                code,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                note);
            return position;
        }
        public Position GetPosition(short id)
        {
            id = id > 0 ? id : (short)0;
            var position = Position.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectPositionByIdSql = 
                    $"select p.id, p.code, p.name, p.short_name, p.alt_name, p.display_name, p.is_active, p.display_order, p.note " +
                    $"  from {CommonSchema}.v_position p " +
                    $" where p.id = :id";
                command.CommandText = SelectPositionByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    position = CreatePosition(reader);
                reader.Close();
            }
            if (position != null && position != Position.Null)
                return position;
            throw new PositionNotFoundException(id);
        }
        public Position GetPosition(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var position = Position.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectPositionByCodeSql =
                    $"select p.id, p.code, p.name, p.short_name, p.alt_name, p.display_name, p.is_active, p.display_order, p.note " +
                    $"  from {CommonSchema}.v_position p " +
                    $" where p.code = :code";
                command.CommandText = SelectPositionByCodeSql;
                command.Parameters.Add(new OracleParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    position = CreatePosition(reader);
                reader.Close();
            }
            if (position != null && position != Position.Null)
                return position;
            throw new PositionNotFoundException(code);
        }
        public Position[] GetPositions()
        {
            var positions = new List<Position>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPositionsSql =
                    $"select p.id, p.code, p.name, p.short_name, p.alt_name, p.display_name, p.is_active, p.display_order, p.note " +
                    $"  from {CommonSchema}.v_position p";
                command.CommandText = SelectPositionsSql;
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
    }
}