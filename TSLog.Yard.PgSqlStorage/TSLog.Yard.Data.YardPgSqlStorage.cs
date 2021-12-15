using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Data
{
    public class YardPgSqlStorage : PgSqlStorage, IYardCommonStorage
    {
        /// <summary>
        /// Схема в БД для хранение объектов модуля управления двором
        /// </summary>
        public string YardSchema { private get; set; }

        public YardPgSqlStorage(string host, string database) : base(host, database)
        {
            _gates = new Catalog<Gate>("GATES", "Ворота");
            _logists = new Catalog<Entity>("LOGISTS", "Логисты");
            _operationStates = new Catalog<Entity>("OPERATION_STATES", "Состояния (статусы) операций двора");
            _operationTypes = new Catalog<Entity>("OPERATION_TYPES", "Типы операций двора");
            _vihicleKinds = new Catalog<Entity>("VIHICLE_KINDS", "Виды автомашин");
        }

        private NpgsqlParameter CreateParameter(string name, NpgsqlDbType type, object value)
        {
            var parameter = new NpgsqlParameter(name, type)
            {
                Value = value
            };
            return parameter;
        }

        public void AddAction(YardOperation operation, WarehouseAction action)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using var command = _connection.CreateCommand();
            command.CommandText = "";
            command.ExecuteScalar();
        }
        private WarehouseAction CreateWarehouseAction(DbDataReader reader)
        {
            return null;
        }
        public WarehouseAction[] GetActions(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            var actions = new List<WarehouseAction>();
            using (var command = _connection.CreateCommand()) 
            {
                var sql = $"select oa.operation_id, oa.action_kind, oa.action_id " +
                    $"from {YardSchema}.v_operation_action oa where oa.operation_id = :operation_id";
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("oparetion_id", operation.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var action = CreateWarehouseAction(reader);
                    actions.Add(action);
                }
            }
            return actions.ToArray();
        }
        public void RemoveAction(YardOperation operation, WarehouseAction action)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using var command = _connection.CreateCommand();
            command.CommandText = "";
            command.ExecuteScalar();
        }        

        private readonly Catalog<Gate> _gates = null;    
        private Gate CreateGate(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            return new Gate(Convert.ToInt32(reader["id"]),
                Convert.ToString(reader["code"]),
                Convert.ToString(reader["name"]),
                Convert.ToString(reader["short_name"]),
                Convert.ToString(reader["alt_name"]),
                Convert.ToString(reader["display_name"]),
                Convert.ToString(reader["note"]));
        }
        public Gate GetGate(int id) 
        {
            id = id > 0 ? id : -1;
            var gate = _gates[id];
            if (gate != null)
                return gate;
            using (var command = _connection.CreateCommand())
            {
                var sql = "select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.note from yard.v_gate g where g.id = :id";
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    gate = CreateGate(reader);
                    _gates.Add(gate);
                    return gate;
                }
            }
            throw new Exception("Gate not found by id " + id);
        }
        public Gate GetGate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var gate = _gates[code];
            if (gate != null)
                return gate;
            using (var command = _connection.CreateCommand())
            {
                var sql = "select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.note from yard.v_gate g where g.code = :code";
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    gate = CreateGate(reader);
                    _gates.Add(gate);
                    return gate;
                }
            }
            throw new Exception("Gate not found by code " + code);
        }
        public Gate[] GetGates()
        {
            var gates = new List<Gate>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.note " 
                    + "from yard.v_gate g";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var gate = CreateGate(reader);
                    gates.Add(gate);
                }
            }
            return gates.ToArray();
        }
        public Interval[] GetIntervals(DateTime from, DateTime to, Entity vehicleKind, Entity operationKind)
        {
            throw new NotImplementedException();
        }

        private readonly Catalog<Entity> _logists = null;
        public Entity GetLogist(int id)
        {
            var sql = $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.note " + 
                "from {YardSchema}.v_logist l where l.id = :id";
            return GetEntity(id, sql, _logists);
        }
        public Entity GetLogist(string code)
        {
            var sql = $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.note " + 
                "from {YardSchema}.v_logist l where l.code = :code";
            return GetEntity(code, sql, _logists);
        }
        public Entity[] GetLogists()
        {
            var sql = $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.note " +
                "from {YardSchema}.v_logist l";
            return GetEntities(sql);
        }

        private Catalog<Entity> _operationStates = null;
        public Entity GetOperationState(int id)
        {
            var sql = $"select * from {YardSchema}.v_operation_state os where os.id = :id";
            return GetEntity(id, sql, _operationStates);
        }
        public Entity GetOperationState(string code)
        {
            var sql = $"select * from {YardSchema}.v_operation_state os where os.code = :code";
            return GetEntity(code, sql, _operationStates);
        }
        public Entity[] GetOperationStates()
        {
            var sql = $"select * from {YardSchema}.v_operation_state os";
            return GetEntities(sql);
        }

        private Catalog<Entity> _operationTypes = null;
        public Entity GetOperationType(int id)
        {
            var sql = $"select * from {YardSchema}.v_operation_type ot where ot.id = :id";
            return GetEntity(id, sql, _operationTypes);
        }
        public Entity GetOperationType(string code)
        {
            var sql = $"select * from {YardSchema}.v_operation_type ot where ot.code = :code";
            return GetEntity(code, sql, _operationTypes);
        }
        public Entity[] GetOperationTypes()
        {
            var sql = $"select * from {YardSchema}.v_operation_type ot";
            return GetEntities(sql);
        }

        private Catalog<Entity> _vihicleKinds = null;
        public Entity GetVehicleKind(int id)
        {
            var sql = $"select * from {YardSchema}.v_vehicle_kind vk where vk.id = :id";
            return GetEntity(id, sql, _vihicleKinds);
        }
        public Entity GetVehicleKind(string code)
        {
            var sql = $"select * from {YardSchema}.v_vehicle_kind vk where vk.code = :code";
            return GetEntity(code, sql, _vihicleKinds);
        }
        public Entity[] GetVehicleKinds()
        {
            var sql = $"select * from {YardSchema}.v_vehicle_kind vk";
            return GetEntities(sql);
        }


        public YardOperation AddOperation(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var command = _connection.CreateCommand())
            {
                var sql = $"select {YardSchema}.add_operation(:p_type_code," +
                    ":p_plan_start_date," +
                    ":p_plan_finish_date," +
                    ":p_vehicle_kind_code," +
                    ":p_vehicle_brand," +
                    ":p_vehicle_model," +
                    ":p_vehicle_number," +
                    ":p_driver_name," +
                    ":p_logist_id," +
                    ":p_manager_id," +
                    ":p_note);";
                command.CommandText = sql;

                command.Parameters.Add(CreateParameter("p_type_code", NpgsqlDbType.Varchar, operation.Type.Code));
                command.Parameters.Add(CreateParameter("p_plan_start_date", NpgsqlDbType.Timestamp, operation.PlanStartDate));
                command.Parameters.Add(CreateParameter("p_plan_finish_date", NpgsqlDbType.Timestamp, operation.PlanFinshDate));
                command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, operation.VehicleKind.Code));
                command.Parameters.Add(CreateParameter("p_vehicle_brand", NpgsqlDbType.Varchar, operation.VehicleBrand));
                command.Parameters.Add(CreateParameter("p_vehicle_model", NpgsqlDbType.Varchar, operation.VehicleModel));
                command.Parameters.Add(CreateParameter("p_vehicle_number", NpgsqlDbType.Varchar, operation.VehicleNumber));
                command.Parameters.Add(CreateParameter("p_driver_name", NpgsqlDbType.Varchar, operation.DriverName));
                command.Parameters.Add(CreateParameter("p_logist_id", NpgsqlDbType.Integer, 
                    operation.Logist != null ? operation.Logist.Id : DBNull.Value));
                command.Parameters.Add(CreateParameter("p_manager_id", NpgsqlDbType.Integer, operation.Manager.Id));
                command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));

                operation.Id = Convert.ToInt64(command.ExecuteScalar());
            }

            var actions = operation.GetActions();

            return operation;
        }
        private YardOperation CreateOperation(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var operation = new YardOperation(Convert.ToInt64(reader["id"]), null);
            return operation;
        }
        public YardOperation GetOperation(long id)
        {
            using (var command = _connection.CreateCommand())
            {
                var sql = $"select * from {YardSchema}.v_operation o where o.id = :id";
                command.CommandText = sql;
                command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Bigint, id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var operation = CreateOperation(reader);
                    return operation;
                }
                reader.Close();
            }
            throw new OperationNotFoundException(id);
        }
        public YardOperation[] GetOperations(DateTime from, DateTime to, Partner[] partners)
        {
            var operations = new List<YardOperation>();
            if (partners != null && partners.Length > 0)
            {
                var sql = $"select * from {YardSchema}.v_partner_operation po " +
                    $"where po.plan_start_date between :from_date and :to_date " +
                    $"  and po.plan_finish_date between :from_date and :to_date " +
                    $"  and po.partner_id = :partner_id";
                foreach (var partner in partners)
                {
                    using var command = _connection.CreateCommand();
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, from));
                    command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, to));
                    command.Parameters.Add(CreateParameter("partner_id", NpgsqlDbType.Varchar, partner.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var operation = CreateOperation(reader);
                        operations.Add(operation);
                    }
                    reader.Close();
                }
            }
            else
            {
                var sql = $"select * from {YardSchema}.v_partner_operation po " +
                    $"where po.plan_start_date between :from_date and :to_date " +
                    $"  and po.plan_finish_date between :from_date and :to_date";
                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, from));
                command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, to));                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var operation = CreateOperation(reader);
                    operations.Add(operation);
                }
                reader.Close();
            }
            return operations.ToArray();
        }
        public Gate SetOperationGate(YardOperation operation, Gate gate = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var command = _connection.CreateCommand())
            {
                var sql = $"select {YardSchema}.set_operation_gate(:p_id," +
                    $":p_gate_code);";
                command.CommandText = sql;
                command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                command.Parameters.Add(CreateParameter("p_gate_code", NpgsqlDbType.Varchar, gate != null ? gate.Code : DBNull.Value));
                var gateId = Convert.ToInt32(command.ExecuteScalar());
                operation.Gate = GetGate(gateId);
            }
            return operation.Gate;
        }
        public void EditOperation(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var command = _connection.CreateCommand())
            {
                var sql = $"select yard.edit_operation(:p_id," +
                    $":p_type_code," +
                    $":p_plan_start_date," +
                    $":p_plan_finish_date," +
                    $":p_vehicle_kind_code," +
                    $":p_vehicle_brand," +
                    $":p_vehicle_model," +
                    $":p_vehicle_number," +
                    $":p_driver_name," +
                    $":p_logist_id," +
                    $":p_manager_id," +
                    $":p_note);";
                command.CommandText = sql;

                command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                command.Parameters.Add(CreateParameter("p_type_code", NpgsqlDbType.Varchar, operation.Type.Code));
                command.Parameters.Add(CreateParameter("p_plan_start_date", NpgsqlDbType.Timestamp, operation.PlanStartDate));
                command.Parameters.Add(CreateParameter("p_plan_finish_date", NpgsqlDbType.Timestamp, operation.PlanFinshDate));
                command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, operation.VehicleKind.Code));
                command.Parameters.Add(CreateParameter("p_vehicle_brand", NpgsqlDbType.Varchar, operation.VehicleBrand));
                command.Parameters.Add(CreateParameter("p_vehicle_model", NpgsqlDbType.Varchar, operation.VehicleModel));
                command.Parameters.Add(CreateParameter("p_vehicle_number", NpgsqlDbType.Varchar, operation.VehicleNumber));
                command.Parameters.Add(CreateParameter("p_driver_name", NpgsqlDbType.Varchar, operation.DriverName));
                command.Parameters.Add(CreateParameter("p_logist_id", NpgsqlDbType.Integer,
                    operation.Logist != null ? operation.Logist.Id : DBNull.Value));
                command.Parameters.Add(CreateParameter("p_manager_id", NpgsqlDbType.Integer, operation.Manager.Id));
                command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));

                command.ExecuteScalar();
            }

            // TODO: synchronize action
            throw new NotImplementedException();
        }
        public void StartOperation(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void MoveToGate(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.operation_to_gate(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void SetResponsibleOfficer(YardOperation operation, Employee officer) 
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (officer == null)
                throw new ArgumentNullException(nameof(officer));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.set_operation_responsible_officer(:p_id, " +
                $":p_responsible_officer_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_responsible_officer_id", NpgsqlDbType.Integer, officer.Id));
            command.ExecuteScalar();
            operation.ResponsibleOfficer = officer;
        }
        public void StartOperationWork(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation_work(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void FinishOperationWork(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation_work(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void LeaveGate(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.operation_leave_gate(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void FinishOperation(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void CancelOperation(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.cancel_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void DeleteOperation(YardOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.delete_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
    }
}