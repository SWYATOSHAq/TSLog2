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
using OperationType = TSLog.Yard.BO.OperationType;

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
            _operationTypes = new Catalog<OperationType>("OPERATION_TYPES", "Типы операций двора");
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


        #region Domains
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
                reader.Close();
            }
            throw new EntityNotFoundException(nameof(Gate), id);
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
                reader.Close();
            }
            throw new EntityNotFoundException(nameof(Gate), code);
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
                reader.Close();
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

        private readonly Catalog<Entity> _operationStates = null;
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

        private readonly Catalog<OperationType> _operationTypes = null;
        private OperationType CreateOperationType(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var direction = Convert.ToString(reader["direction"]) == "O" ? OperationType.TypeDirection.Out : OperationType.TypeDirection.In;
            var operationtype = new OperationType(Convert.ToInt32(reader["id"]),
                direction,
                Convert.ToString(reader["code"]),
                Convert.ToString(reader["name"]),
                Convert.ToString(reader["short_name"]),
                Convert.ToString(reader["alt_name"]),
                Convert.ToString(reader["display_name"]),
                Convert.ToInt32(reader["time"]),
                Convert.ToInt16(reader["workers_count"]),
                Convert.ToString(reader["note"])
                );
            return operationtype;
        }
        public OperationType GetOperationType(int id)
        {
            id = id > 0 ? id : -1;
            var operationType = _operationTypes[id];
            if (operationType != null)
                return operationType;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, "
                    + $"ot.time, ot.workers_count, ot.note "
                    + $"from {YardSchema}.v_operation_type ot "
                    + $"where ot.id = :id";
                command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Smallint, id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    operationType = CreateOperationType(reader);
                    _operationTypes.Add(operationType);
                    return operationType;
                }
                reader.Close();
            }
            throw new EntityNotFoundException(nameof(operationType), id);
        }
        public OperationType GetOperationType(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var operationType = _operationTypes[code];
            if (operationType != null)
                return operationType;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, "
                    + $"ot.time, ot.workers_count, ot.note "
                    + $"from {YardSchema}.v_operation_type ot "
                    + $"where ot.code = :code";
                command.Parameters.Add(CreateParameter("code", NpgsqlDbType.Varchar, code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    operationType = CreateOperationType(reader);
                    _operationTypes.Add(operationType);
                    return operationType;
                }
                reader.Close();
            }
            throw new EntityNotFoundException(nameof(operationType), code);
        }
        public OperationType[] GetOperationTypes(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            var operationtypes = new List<OperationType>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select pot.id, pot.direction, pot.code, pot.name, pot.short_name, pot.alt_name, pot.display_name, "
                    + $"pot.time, pot.workers_count, pot.note "
                    + $"from {YardSchema}.v_partner_operation_type pot "
                    + $"where pot.partner_id = :partner_id";
                command.Parameters.Add(CreateParameter("partner_id", NpgsqlDbType.Varchar, partner.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var operationtype = CreateOperationType(reader);
                    operationtypes.Add(operationtype);
                }
                reader.Close();
            }
            return operationtypes.ToArray();
        }
        public OperationType[] GetOperationTypes()
        {
            var operationtypes = new List<OperationType>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select pot.id, pot.direction, pot.code, pot.name, pot.short_name, pot.alt_name, pot.display_name, "
                    + $"pot.time, pot.workers_count, pot.note "
                    + $"from {YardSchema}.v_partner_operation_type pot";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var operationtype = CreateOperationType(reader);
                    operationtypes.Add(operationtype);
                }
                reader.Close();
            }
            return operationtypes.ToArray();
        }

        private readonly Catalog<Entity> _vihicleKinds = null;
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

        public delegate Partner GetPartnerByIdDelegate(string id);
        public delegate Employee GetEmployeeByIdDelegate(int id);
        public GetPartnerByIdDelegate GetPartnerById { private get; set; }
        public GetEmployeeByIdDelegate GetEmployeeById { private get; set; }
        public Partner GetPartner(string id)
        {
            return GetPartnerById(id);
        }
        private Employee GetEmployee(int id)
        {
            return GetEmployeeById != null ? GetEmployeeById(id) : Employee.Null;
        }
        #endregion

        #region Operations        
        public Operation AddOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var command = _connection.CreateCommand())
            {
                var sql = $"select {YardSchema}.add_operation(:p_type_code," +
                    ":p_plan_start_date," +
                    ":p_plan_finish_date," +
                    ":p_workers_count," +
                    ":p_vehicle_kind_code," +
                    ":p_vehicle_brand," +
                    ":p_vehicle_model," +
                    ":p_vehicle_number," +
                    ":p_driver_name," +
                    ":p_logist_id," +
                    ":p_priority," +
                    ":p_manager_id," +
                    ":p_note);";
                command.CommandText = sql;

                command.Parameters.Add(CreateParameter("p_type_code", NpgsqlDbType.Varchar, operation.Type.Code));
                command.Parameters.Add(CreateParameter("p_plan_start_date", NpgsqlDbType.Timestamp, operation.PlanStartDate));
                command.Parameters.Add(CreateParameter("p_plan_finish_date", NpgsqlDbType.Timestamp, operation.PlanFinshDate));
                command.Parameters.Add(CreateParameter("p_workers_count", NpgsqlDbType.Smallint, operation.WorkersCount));
                command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, operation.VehicleKind.Code));
                command.Parameters.Add(CreateParameter("p_vehicle_brand", NpgsqlDbType.Varchar, operation.VehicleBrand));
                command.Parameters.Add(CreateParameter("p_vehicle_model", NpgsqlDbType.Varchar, operation.VehicleModel));
                command.Parameters.Add(CreateParameter("p_vehicle_number", NpgsqlDbType.Varchar, operation.VehicleNumber));
                command.Parameters.Add(CreateParameter("p_driver_name", NpgsqlDbType.Varchar, operation.DriverName));
                command.Parameters.Add(CreateParameter("p_logist_id", NpgsqlDbType.Integer, 
                    operation.Logist != null ? operation.Logist.Id : DBNull.Value));
                command.Parameters.Add(CreateParameter("p_priority", NpgsqlDbType.Smallint, operation.Priority));
                command.Parameters.Add(CreateParameter("p_manager_id", NpgsqlDbType.Integer, operation.Manager.Id));
                command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));

                operation.Id = Convert.ToInt64(command.ExecuteScalar());
            }

            var actions = operation.GetActions();
            // TODO: syncronize actions

            return operation;
        }
        private Operation CreateOperation(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var operation = new Operation(Convert.ToInt64(reader["id"]), null);

            operation.Type = GetOperationType(Convert.ToInt16(reader["type_id"]));
            operation.PlanStartDate = Convert.ToDateTime(reader["plan_start_date"]);
            operation.FactStartDate = Convert.ToDateTime(reader["fact_start_date"]);
            operation.GateArrivalDate = Convert.ToDateTime(reader["gate_arrival_date"]);
            operation.WorkStartDate = Convert.ToDateTime(reader["work_start_date"]);
            operation.WorkFinishDate = Convert.ToDateTime(reader["work_finish_date"]);
            operation.GateDepartureDate = Convert.ToDateTime(reader["gate_departure_date"]);
            operation.PlanFinshDate = Convert.ToDateTime(reader["plan_finish_date"]);
            operation.FactFinishDate = Convert.ToDateTime(reader["fact_finish_date"]);
            operation.WorkersCount = Convert.ToInt16(reader["workers_count"]);
            operation.VehicleKind = GetVehicleKind(Convert.ToInt16(reader["vehicle_kind_id"]));
            operation.VehicleBrand = Convert.ToString(reader["vehicle_brand"]);
            operation.VehicleModel = Convert.ToString(reader["vehicle_model"]);
            operation.VehicleNumber = Convert.ToString(reader["vehicle_number"]);
            operation.DriverName = Convert.ToString(reader["driver_name"]);
            operation.Logist = GetLogist(Convert.ToInt16(reader["logist_id"]));
            operation.Gate = GetGate(Convert.ToInt16(reader["gate_id"]));
            operation.State = GetOperationState(Convert.ToInt16(reader["state_id"]));
            operation.Priority = Convert.ToInt16(reader["priority"]);
            operation.Manager = GetEmployee(Convert.ToInt32(reader["manager_id"]));
            operation.ResponsibleOfficer = GetEmployee(Convert.ToInt32(reader["responsible_officer_id"])); ;
            operation.Note = Convert.ToString(reader["note"]);

            var actions = GetActions(operation);
            operation.AddActions(actions);

            return operation;
        }
        public Operation GetOperation(long id)
        {
            using (var command = _connection.CreateCommand())
            {
                var sql = 
                    $"select o.id, " +
                    $"       o.type_id,  o.type_code, o.type_name, " +
                    $"       o.plan_start_date, o.fact_start_date, " +
                    $"       o.gate_arrival_date, " +
                    $"       o.work_start_date, o.work_finish_date, " +
                    $"       o.gate_departure_date, " +
                    $"       o.plan_finish_date, o.fact_finish_date, " +
                    $"       o.workers_count, " +
                    $"       o.vehicle_kind_id, o.vehicle_kind_code, o.vehicle_kind_name, " +
                    $"       o.vehicle_brand, o.vehicle_model, o.vehicle_number, " +
                    $"       o.driver_name, " +
                    $"       o.logist_id, o.logist_code, o.logist_name, " +
                    $"       o.gate_id, o.gate_code, o.gate_name, " +
                    $"       o.state_id, o.state_code, o.state_name, " +
                    $"       o.priority, " +
                    $"       o.manager_id, " +
                    $"       o.responsible_officer_id, o.note " +
                    $"  from {YardSchema}.v_operation o " +
                    $" where o.id = :id";
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
        public Operation[] GetOperations(DateTime from, DateTime to, Partner[] partners)
        {
            var operations = new List<Operation>();
            if (partners != null && partners.Length > 0)
            {
                var sql = 
                    $"select distinct po.id, " +
                    $"       po.type_id, " +
                    $"       po.type_code, " +
                    $"       po.type_name, " +
                    $"       po.plan_start_date, " +
                    $"       po.fact_start_date, " +
                    $"       po.gate_arrival_date, " +
                    $"       po.work_start_date, " +
                    $"       po.work_end_date, " +
                    $"       po.gate_departure_date, " +
                    $"       po.plan_finish_date, " +
                    $"       po.fact_finish_date, " +
                    $"       po.vehicle_kind_id, " +
                    $"       po.vehicle_kind_code, " +
                    $"       po.vehicle_kind_name, " +
                    $"       po.vehicle_brand, " +
                    $"       po.vehicle_model, " +
                    $"       po.vehicle_number, " +
                    $"       po.driver_name, " +
                    $"       po.logist_id, " +
                    $"       po.logist_code, " +
                    $"       po.logist_name, " +
                    $"       po.gate_id, " +
                    $"       po.gate_code, " +
                    $"       po.gate_name, " +
                    $"       po.state_id, " +
                    $"       po.state_code, " +
                    $"       po.state_name, " +
                    $"       po.priority, " +
                    $"       po.manager_id, " +
                    $"       po.responsible_officer_id, " +
                    $"       po.note " +
                    $"  from {YardSchema}.v_partner_operation po " +
                    $" where po.plan_start_date between :from_date and :to_date " +
                    $"   and po.plan_finish_date between :from_date and :to_date " +
                    $"   and po.partner_id = :partner_id";
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
                var sql = $"select distinct po.id, " +
                    $"       po.type_id, " +
                    $"       po.type_code, " +
                    $"       po.type_name, " +
                    $"       po.plan_start_date, " +
                    $"       po.fact_start_date, " +
                    $"       po.gate_arrival_date, " +
                    $"       po.work_start_date, " +
                    $"       po.work_end_date, " +
                    $"       po.gate_departure_date, " +
                    $"       po.plan_finish_date, " +
                    $"       po.fact_finish_date, " +
                    $"       po.vehicle_kind_id, " +
                    $"       po.vehicle_kind_code, " +
                    $"       po.vehicle_kind_name, " +
                    $"       po.vehicle_brand, " +
                    $"       po.vehicle_model, " +
                    $"       po.vehicle_number, " +
                    $"       po.driver_name, " +
                    $"       po.logist_id, " +
                    $"       po.logist_code, " +
                    $"       po.logist_name, " +
                    $"       po.gate_id, " +
                    $"       po.gate_code, " +
                    $"       po.gate_name, " +
                    $"       po.state_id, " +
                    $"       po.state_code, " +
                    $"       po.state_name, " +
                    $"       po.priority, " +
                    $"       po.manager_id, " +
                    $"       po.responsible_officer_id, " +
                    $"       po.note " +
                    $" from {YardSchema}.v_partner_operation po " +
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
        public Partner[] GetOperationsPartners(DateTime from, DateTime to)
        {
            var partners = new HashSet<Partner>();
            using (var command = _connection.CreateCommand())
            {
                var sql = 
                    $"select distinct partner_id, op.plan_start_date, op.plan_finish_date " +
                    $"  from {YardSchema}.v_operation_partner op " +
                    $" where not (op.plan_start_date > :finish_date or op.plan_finish_date < :start_date)";
                command.CommandText = sql;
                command.Parameters.Add(CreateParameter("start_date", NpgsqlDbType.Timestamp, from));
                command.Parameters.Add(CreateParameter("finish_date", NpgsqlDbType.Timestamp, to));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var partner = GetPartner(Convert.ToString(reader["partner_id"]));
                    if (!partners.Contains(partner))
                        partners.Add(partner);
                }
                reader.Close();
            }
            return partners.ToArray();
        }
        public Gate SetOperationGate(Operation operation, Gate gate = null)
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
        public void EditOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var command = _connection.CreateCommand())
            {
                var sql = $"select yard.edit_operation(:p_id," +
                    $":p_type_code," +
                    $":p_plan_start_date," +
                    $":p_plan_finish_date," +
                    $":p_workers_count," +
                    $":p_vehicle_kind_code," +
                    $":p_vehicle_brand," +
                    $":p_vehicle_model," +
                    $":p_vehicle_number," +
                    $":p_driver_name," +
                    $":p_logist_id," +
                    $":p_priority," +
                    $":p_manager_id," +
                    $":p_note);";
                command.CommandText = sql;

                command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                command.Parameters.Add(CreateParameter("p_type_code", NpgsqlDbType.Varchar, operation.Type.Code));
                command.Parameters.Add(CreateParameter("p_plan_start_date", NpgsqlDbType.Timestamp, operation.PlanStartDate));
                command.Parameters.Add(CreateParameter("p_plan_finish_date", NpgsqlDbType.Timestamp, operation.PlanFinshDate));
                command.Parameters.Add(CreateParameter("p_workers_count", NpgsqlDbType.Smallint, operation.WorkersCount));
                command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, operation.VehicleKind.Code));
                command.Parameters.Add(CreateParameter("p_vehicle_brand", NpgsqlDbType.Varchar, operation.VehicleBrand));
                command.Parameters.Add(CreateParameter("p_vehicle_model", NpgsqlDbType.Varchar, operation.VehicleModel));
                command.Parameters.Add(CreateParameter("p_vehicle_number", NpgsqlDbType.Varchar, operation.VehicleNumber));
                command.Parameters.Add(CreateParameter("p_driver_name", NpgsqlDbType.Varchar, operation.DriverName));
                command.Parameters.Add(CreateParameter("p_logist_id", NpgsqlDbType.Integer,
                    operation.Logist != null ? operation.Logist.Id : DBNull.Value));
                command.Parameters.Add(CreateParameter("p_priority", NpgsqlDbType.Smallint, operation.Priority));
                command.Parameters.Add(CreateParameter("p_manager_id", NpgsqlDbType.Integer, operation.Manager.Id));
                command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));

                command.ExecuteScalar();
            }
            var actions = GetActions(operation);
            // TODO: synchronize action
        }
        public void StartOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void MoveToGate(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.operation_to_gate(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void SetResponsibleOfficer(Operation operation, Employee officer) 
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
        public void StartOperationWork(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation_work(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void FinishOperationWork(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation_work(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void LeaveGate(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.operation_leave_gate(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void FinishOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void CancelOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.cancel_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        public void DeleteOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var command = _connection.CreateCommand();
            var sql = $"select {YardSchema}.delete_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
        }
        #endregion

        #region Actions
        public void AddAction(Operation operation, WarehouseAction action)
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
            var kind = Convert.ToString(reader["kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var partner = GetPartner(Convert.ToString(reader["partner_id"]));
            var action = new WarehouseAction(kind,
                Convert.ToString(reader["id"]),
                partner,
                Convert.ToString(reader["partner_action_id"]));
            return action;
        }
        public WarehouseAction[] GetActions(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            var actions = new List<WarehouseAction>();
            using (var command = _connection.CreateCommand())
            {
                var sql = $"select oa.operation_id, oa.action_kind, oa.action_id, oa.partner_id, oa.partner_action_id " +
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
        public void RemoveAction(Operation operation, WarehouseAction action)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using var command = _connection.CreateCommand();
            command.CommandText = "";
            command.ExecuteScalar();
        }
        #endregion
    }
}