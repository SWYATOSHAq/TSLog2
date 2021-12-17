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
            using (var command = _connection.CreateCommand())
            {
                var sql = 
                    $"select a.id, a.action_kind, a.partner_id " +
                    $"  from {CommonSchema}.v_warehouse_action a " +
                    $" where a.id = :id " +                    
                    $"   and a.action_kind = :action_kind";
                command.Parameters.Add(new OracleParameter("id", id));                
                command.Parameters.Add(new OracleParameter("action_kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var action = CreateAction(reader);
                    return action;
                }
                reader.Close();
            }
            throw new WarehouseActionNotFoundException(id);
        }
        public WarehouseAction GetAction(ActionKind actionKind, Partner partner, string partnerActionId)
        {
            using (var command = _connection.CreateCommand())
            {
                var sql = $"select a.id, a.partner_id " +
                    $"from {CommonSchema}.v_warehouse_action a " +
                    $"where a.partner_number = :partner_number " +
                    $"and a.partner_id = :partner_id" +
                    $"and a.action_kind = :action_kind";
                command.Parameters.Add(new OracleParameter("partner_number", partnerActionId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                command.Parameters.Add(new OracleParameter("action_kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var action = CreateAction(reader);
                    return action;
                }
                reader.Close();
            }
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
            using (var command = _connection.CreateCommand())
            {
                var sql = $"select * from {CommonSchema}.v_partner p where upper(trim(p.id)) = :id";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("id", id.Trim().ToUpper()));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var partner = CreatePartner(reader);
                    return partner;
                }
            }
            throw new PartnerNotFoundException(id);
        }
        public Partner[] GetPartners()
        {
            var partners = new List<Partner>();
            using (var command = _connection.CreateCommand()) 
            {
                var sql = $"select * from {CommonSchema}.v_partner p";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var partner = CreatePartner(reader);
                    partners.Add(partner);
                }
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
            using (var command = _connection.CreateCommand())
            {
                var sql = $"select * from {CommonSchema}.v_employee e where e.id = :id";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    return employee;
                }
            }
            throw new EmployeeNotFoundException(id);
        }
        public Employee[] GetEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var sql = $"select * from {CommonSchema}.v_employee e";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
            }
            return employees.ToArray();
        }
    }
}