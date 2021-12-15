using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using TSLog.Common.Data;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Data
{
    public class YardOracleStorage : OracleStorage, IYardWmsStorage
    {
        /// <summary>
        /// Схема для "общих" объектов БД
        /// </summary>
        public string CommonSchema { private get; set; }
        public YardOracleStorage(string dataSource) : base(dataSource)
        { }

        private WarehouseAction CreateWarehouseAction(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var partner = GetPartner(Convert.ToString(reader["partner_id"]));
            var kind = Convert.ToString(reader["kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var action = new WarehouseAction(Convert.ToString(reader["id"]),
                kind,
                partner);
            return action;
        }
        public WarehouseAction GetAction(Partner partner, ActionKind actionKind, string partnerActionId)
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
                    var action = CreateWarehouseAction(reader);
                    return action;
                }
                reader.Close();
            }
            throw new Exception("action not found");
        }
        private Partner CreatePartner(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var partner = new Partner(Convert.ToString(reader["id"]),
                Convert.ToString(reader["name"]));
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
            throw new Exception($"partner not found by id {id}");
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
                Convert.ToString(reader["code"]),
                Convert.ToString(reader["name"]));
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
            throw new Exception("employee not found");
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