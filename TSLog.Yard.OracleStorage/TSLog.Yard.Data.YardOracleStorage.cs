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
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var kind = Convert.ToString(reader["kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var action = new WarehouseAction(kind,
                Utils.DbValueToString(reader["id"]),                
                partner,
                Utils.DbValueToString(reader["partner_action_id"]));
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
        private Partner CreatePartner(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var partner = new Partner(Utils.DbValueToString(reader["id"]),
                Utils.DbValueToString(reader["name"]));
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
                var SelectPartnerByIdSql = $"select * from {CommonSchema}.v_partner p where upper(trim(p.id)) = :id";
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
        public Partner[] GetPartners()
        {
            var partners = new List<Partner>();
            using (var command = _connection.CreateCommand()) 
            {
                var SelectPartnersSql = $"select * from {CommonSchema}.v_partner p";
                command.CommandText = SelectPartnersSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var partner = CreatePartner(reader);
                    partners.Add(partner);
                }
                reader.Close();
            }
            return partners.ToArray();
        }

        private Employee CreateEmployee(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var employee = new Employee(Convert.ToInt64(reader["id"]),
                Utils.DbValueToString(reader["code"]),
                Utils.DbValueToString(reader["name"]));
            return employee;
        }
        public Employee GetEmployee(int id) 
        {
            id  = id > 0 ? id : 0;
            var employee = Employee.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById = $"select * from {CommonSchema}.v_employee e where e.id = :id";
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
        public Employee GetManager(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));

            var manager = Employee.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById = $"select p.manager_id from {CommonSchema}.v_partner p";
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    manager = CreateEmployee(reader);
                reader.Close();
            }
            if (manager != null && manager != Employee.Null)
                return manager;
            throw new PartnerNotFoundException(partner.Id);
        }
        public Employee[] GetEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql = $"select * from {CommonSchema}.v_employee e";
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
        private Position CreatePosition(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var position = new Position(Convert.ToInt16(reader["id"]),
                Utils.DbValueToString(reader["code"]),
                Utils.DbValueToString(reader["name"]),
                Utils.DbValueToString(reader["short_name"]),
                Utils.DbValueToString(reader["alt_name"]),
                Utils.DbValueToString(reader["display_name"]),
                Utils.DbValueToString(reader["note"]));
            return position;
        }
        public Position GetPosition(int id)
        {
            id = id > 0 ? id : 0;
            var position = Position.Null;
            using (var command = _connection.CreateCommand())
            {
                var SelectPositionByIdSql = $"select * from {CommonSchema}.v_position p where p.id = :id";
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
                var SelectPositionByCodeSql = $"select * from {CommonSchema}.v_position p where p.code = :code";
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
                var SelectPositionsSql = $"select * from {CommonSchema}.v_position p";
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