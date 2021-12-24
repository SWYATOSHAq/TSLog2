using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using TSLog.Common;
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
        private const string DefaultYardSchema = "Yard";
        /// <summary>
        /// Схема в БД для хранение объектов модуля управления двором
        /// </summary>
        public string YardSchema { private get; set; } = DefaultYardSchema;

        public YardPgSqlStorage(string host, string database) : base(host, database)
        { }

        private NpgsqlParameter CreateParameter(string name, NpgsqlDbType type, object value)
        {
            var parameter = new NpgsqlParameter(name, type)
            {
                Value = value
            };
            return parameter;
        }


        #region Domains
        private readonly Catalog<Gate> _gates = new("GATES", "Ворота");
        private Gate CreateGate(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            return new Gate(Convert.ToInt32(reader["id"]),
                Utils.DbValueToString(reader["code"]),
                Utils.DbValueToString(reader["name"]),
                Utils.DbValueToString(reader["short_name"]),
                Utils.DbValueToString(reader["alt_name"]),
                Utils.DbValueToString(reader["display_name"]),
                Utils.DbValueToString(reader["note"]));
        }
        public Gate GetGate(int id)
        {
            id = id > 0 ? id : -1;
            var gate = _gates[id];
            if (gate != null && gate != Gate.Null)
                return gate;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectGateByIdSql =
                        $"select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.note " +
                        $"  from {YardSchema}.v_gate g where g.id = :id";
                    command.CommandText = SelectGateByIdSql;
                    command.Parameters.Add(new NpgsqlParameter("id", id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        gate = CreateGate(reader);
                        _gates.Add(gate);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (gate != null && gate != Gate.Null)
                return gate;
            throw new EntityNotFoundException(nameof(Gate), id);
        }
        public Gate GetGate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var gate = _gates[code];
            if (gate != null && gate != Gate.Null)
                return gate;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectGateByCodeSql =
                        $"select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.note " +
                        $"  from {YardSchema}.v_gate g where g.code = :code";
                    command.CommandText = SelectGateByCodeSql;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        gate = CreateGate(reader);
                        _gates.Add(gate);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (gate != null && gate != Gate.Null)
                return gate;
            throw new EntityNotFoundException(nameof(Gate), code);
        }
        public Gate[] GetGates()
        {
            var gates = new List<Gate>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectGatesSql =
                        $"select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.note " +
                        $"  from {YardSchema}.v_gate g";
                    command.CommandText = SelectGatesSql;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gate = CreateGate(reader);
                        gates.Add(gate);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return gates.ToArray();
        }
        private readonly Catalog<Logist> _logists = new("LOGISTS", "Логисты");
        private Logist CreateLogist(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            return new Logist(Convert.ToInt32(reader["id"]),
                Utils.DbValueToString(reader["code"]),
                Utils.DbValueToString(reader["name"]),
                Utils.DbValueToString(reader["short_name"]),
                Utils.DbValueToString(reader["alt_name"]),
                Utils.DbValueToString(reader["display_name"]),
                Utils.DbValueToString(reader["note"]));
        }
        public Logist GetLogist(int id)
        {
            id = id > 0 ? id : -1;
            var logist = _logists[id];
            if (logist != null && logist != Logist.Null)
                return logist;

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectLogistByIdSql =
                        $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.note " +
                        $"  from {YardSchema}.v_logist l where l.id = :id";
                    command.CommandText = SelectLogistByIdSql;
                    command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Smallint, id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        logist = CreateLogist(reader);
                        _logists.Add(logist);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (logist != null && logist != Logist.Null)
                return logist;
            throw new EntityNotFoundException(nameof(logist), id);
        }
        public Logist GetLogist(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var logist = _logists[code];
            if (logist != null && logist != Logist.Null)
                return logist;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectLogistByCodeSql =
                        $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.note " +
                        $"  from {YardSchema}.v_logist l where l.code = :code";
                    command.CommandText = SelectLogistByCodeSql;
                    command.Parameters.Add(CreateParameter("code", NpgsqlDbType.Varchar, code));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        logist = CreateLogist(reader);
                        _logists.Add(logist);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (logist != null && logist != Logist.Null)
                return logist;
            throw new EntityNotFoundException(nameof(logist), code);
        }
        public Logist[] GetLogists()
        {
            var logists = new List<Logist>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectLogistsSql =
                        $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.note " +
                        $"  from {YardSchema}.v_logist l";
                    command.CommandText = SelectLogistsSql;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var logist = CreateLogist(reader);
                        logists.Add(logist);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return logists.ToArray();
        }

        private readonly Catalog<Entity>? _operationStates = new("OPERATION_STATES", "Состояния (статусы) операций двора");
        public Entity GetOperationState(int id)
        {
            var SelectOperationStateByIdSql =
                $"select os.id, os.code, os.name, os.short_name, os.alt_name, os.display_name, os.note " +
                $"  from {YardSchema}.v_operation_state os where os.id = :id";
            return GetEntity(id, SelectOperationStateByIdSql, _operationStates);
        }
        public Entity GetOperationState(string code)
        {
            var SelectOperationStateByCodeSql =
                $"select os.id, os.code, os.name, os.short_name, os.alt_name, os.display_name, os.note " +
                $"from {YardSchema}.v_operation_state os where os.code = :code";
            return GetEntity(code, SelectOperationStateByCodeSql, _operationStates);
        }
        public Entity[] GetOperationStates()
        {
            var SelectOperationStatesSql =
                $"select os.id, os.code, os.name, os.short_name, os.alt_name, os.display_name, os.note " +
                $"from {YardSchema}.v_operation_state os";
            return GetEntities(SelectOperationStatesSql);
        }

        private readonly Catalog<OperationType> _operationTypes = new("OPERATION_TYPES", "Типы операций двора");
        private OperationType CreateOperationType(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var direction = Convert.ToString(reader["direction"]) == "O" ? OperationType.TypeDirection.Out : OperationType.TypeDirection.In;
            var operationtype = new OperationType(Convert.ToInt32(reader["id"]),
                direction,
                Utils.DbValueToString(reader["code"]),
                Utils.DbValueToString(reader["name"]),
                Utils.DbValueToString(reader["short_name"]),
                Utils.DbValueToString(reader["alt_name"]),
                Utils.DbValueToString(reader["display_name"]),
                Convert.ToInt32(reader["time"]),
                Utils.DbValueToString(reader["note"])
                );
            return operationtype;
        }
        public OperationType GetOperationType(int id)
        {
            id = id > 0 ? id : -1;
            var operationType = _operationTypes[id];
            if (operationType != null && operationType != OperationType.Null)
                return operationType;

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypeByIdSql =
                        $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, " +
                        $"       ot.time, ot.note " +
                        $"  from {YardSchema}.v_operation_type ot " +
                        $" where ot.id = :id";
                    command.CommandText = SelectOperationTypeByIdSql;
                    command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Smallint, id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        operationType = CreateOperationType(reader);
                        _operationTypes.Add(operationType);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (operationType != null && operationType != OperationType.Null)
                return operationType;
            throw new EntityNotFoundException(nameof(operationType), id);
        }
        public OperationType GetOperationType(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var operationType = _operationTypes[code];
            if (operationType != null && operationType != OperationType.Null)
                return operationType;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypeByCodeSql =
                        $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, " +
                        $"       ot.time, ot.note " +
                        $"  from {YardSchema}.v_operation_type ot " +
                        $" where ot.code = :code";
                    command.CommandText = SelectOperationTypeByCodeSql;
                    command.Parameters.Add(CreateParameter("code", NpgsqlDbType.Varchar, code));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        operationType = CreateOperationType(reader);
                        _operationTypes.Add(operationType);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (operationType != null && operationType != OperationType.Null)
                return operationType;
            throw new EntityNotFoundException(nameof(operationType), code);
        }
        public OperationType[] GetOperationTypes(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            var operationTypes = new List<OperationType>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypesByPartnerSql =
                        $"select pot.id, pot.direction, pot.code, pot.name, pot.short_name, pot.alt_name, pot.display_name, " +
                        $"       pot.time, pot.note " +
                        $"  from {YardSchema}.v_partner_operation_type pot " +
                        $" where pot.partner_id = :partner_id";
                    command.CommandText = SelectOperationTypesByPartnerSql;
                    command.Parameters.Add(CreateParameter("partner_id", NpgsqlDbType.Varchar, partner.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var operationType = CreateOperationType(reader);
                        operationTypes.Add(operationType);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return operationTypes.ToArray();
        }
        public OperationType[] GetOperationTypes()
        {
            var operationtypes = new List<OperationType>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypesSql =
                        $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, " +
                        $"       ot.time, ot.note " +
                        $"  from {YardSchema}.v_operation_type ot";
                    command.CommandText = SelectOperationTypesSql;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var operationtype = CreateOperationType(reader);
                        operationtypes.Add(operationtype);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return operationtypes.ToArray();
        }

        private readonly Catalog<Entity> _vihicleKinds = new("VIHICLE_KINDS", "Виды автомашин");
        public Entity GetVehicleKind(int id)
        {
            var SelectVehicleKindByIdSql =
                $"select vk.id, vk.code, vk.name, vk.short_name, vk.alt_name, vk.display_name, vk.note " +
                $"  from {YardSchema}.v_vehicle_kind vk where vk.id = :id";
            return GetEntity(id, SelectVehicleKindByIdSql, _vihicleKinds);
        }
        public Entity GetVehicleKind(string code)
        {
            var SelectVehicleKindByCodeSql =
                $"select vk.id, vk.code, vk.name, vk.short_name, vk.alt_name, vk.display_name, vk.note " +
                $"  from {YardSchema}.v_vehicle_kind vk where vk.code = :code";
            return GetEntity(code, SelectVehicleKindByCodeSql, _vihicleKinds);
        }
        public Entity[] GetVehicleKinds()
        {
            var SelectVehicleKindsSql =
                $"select vk.id, vk.code, vk.name, vk.short_name, vk.alt_name, vk.display_name, vk.note " +
                $"from {YardSchema}.v_vehicle_kind vk";
            return GetEntities(SelectVehicleKindsSql);
        }

        public delegate Partner GetPartnerByIdDelegate(string id);
        public delegate Employee GetEmployeeByIdDelegate(int id);
        public delegate Position GetPositionByIdDelegate(int id);
        public GetPartnerByIdDelegate? GetPartnerById { private get; set; }
        public GetEmployeeByIdDelegate? GetEmployeeById { private get; set; }
        public GetPositionByIdDelegate? GetPositionById { private get; set; }
        public Partner GetPartner(string id)
        {
            return GetPartnerById != null ? GetPartnerById(id) : Partner.Null;
        }
        private Employee GetEmployee(int id)
        {
            return GetEmployeeById != null ? GetEmployeeById(id) : Employee.Null;
        }
        private Position GetPosition(int id)
        {
            return GetPositionById != null ? GetPositionById(id) : Position.Null;
        }
        #endregion

        #region Operations     
        public short GetTime(OperationType operationType, Entity vehicleKind)
        {
            if (operationType == null)
                throw new ArgumentNullException(nameof(operationType));
            if (vehicleKind == null)
                throw new ArgumentNullException(nameof(vehicleKind));

            var time = OperationType.DefaultTime;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select vkot.time " +
                        $"  from {YardSchema}.v_vehicle_kind_operation_type vkot " +
                        $" where vkot.operation_type_id = :operation_type_id " +
                        $"   and vkot.vehicle_kind_id = :vehicle_kind_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_type_id", NpgsqlDbType.Smallint, operationType.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint, vehicleKind.Id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                        time = Utils.DbValueToShort(reader["time"]);
                    reader.Close();
                }
                connection.Close();
            }
            // TODO: get default time for current operation type
            return time;
        }
        private Interval CreateInterval(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var from = Utils.DbValueToDateTime(reader["start_date"]);
            var to = Utils.DbValueToDateTime(reader["finish_date"]);
            var gate = GetGate(Utils.DbValueToShort(reader["gate_id"]));
            var interval = new Interval(from, to, gate);
            return interval;
        }
        public Interval[] GetAllIntervals(DateTime from, DateTime to)
        {
            var intervals = new List<Interval>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = $"select (yard.get_all_windows(:p_start_date, :p_finish_date)).*;";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("p_start_date", NpgsqlDbType.Timestamp, from));
                    command.Parameters.Add(CreateParameter("p_finish_date", NpgsqlDbType.Timestamp, to));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var interval = CreateInterval(reader);
                        intervals.Add(interval);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return intervals.ToArray();
        }
        public Interval[] GetAvalibleIntervals(DateTime from, DateTime to, OperationType operationType, Entity vehicleKind)
        {
            if (operationType == null)
                throw new ArgumentNullException(nameof(operationType));
            if (vehicleKind == null)
                throw new ArgumentNullException(nameof(vehicleKind));
            var intervals = new List<Interval>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select (yard.get_avalible_windows(:p_start_date," +
                        $"                                 :p_finish_date," +
                        $"                                 :p_operation_type_code," +
                        $"                                 :p_vehicle_kind_code)).*;";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("p_start_date", NpgsqlDbType.Timestamp, from));
                    command.Parameters.Add(CreateParameter("p_finish_date", NpgsqlDbType.Timestamp, to));
                    command.Parameters.Add(CreateParameter("p_operation_type_code", NpgsqlDbType.Varchar, operationType.Code));
                    command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, vehicleKind.Code));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var interval = CreateInterval(reader);
                        intervals.Add(interval);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return intervals.ToArray();
        }
        public Interval[] GetGateIntervals(Gate gate, DateTime from, DateTime to)
        {
            if (gate == null)
                throw new ArgumentNullException(nameof(gate));
            var intervals = new List<Interval>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = $"select (yard.get_gate_windows(:p_gate_code, :p_start_date, :p_finish_date)).*;";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("p_gate_code", NpgsqlDbType.Varchar, gate.Code));
                    command.Parameters.Add(CreateParameter("p_start_date", NpgsqlDbType.Timestamp, from));
                    command.Parameters.Add(CreateParameter("p_finish_date", NpgsqlDbType.Timestamp, to));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var interval = CreateInterval(reader);
                        intervals.Add(interval);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return intervals.ToArray();
        }
        public Operation AddOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = $"select {YardSchema}.add_operation(:p_type_code," +
                        ":p_plan_work_start_date," +
                        ":p_plan_work_finish_date," +
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
                    command.Parameters.Add(CreateParameter("p_plan_work_start_date", NpgsqlDbType.Timestamp, operation.PlanWorkStartDate));
                    command.Parameters.Add(CreateParameter("p_plan_work_finish_date", NpgsqlDbType.Timestamp, operation.PlanWorkFinishDate));
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
                connection.Close();
            }
            return operation;
        }
        private Operation CreateOperation(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var type = GetOperationType(Convert.ToInt16(reader["type_id"]));
            var planWorkStartDate = Utils.DbValueToDateTime(reader["plan_work_start_date"]);
            var planWorkFinishDate = Utils.DbValueToDateTime(reader["plan_work_finish_date"]);
            var vehicleKind = GetVehicleKind(Convert.ToInt16(reader["vehicle_kind_id"]));
            var vehicleBrand = Utils.DbValueToString(reader["vehicle_brand"]);
            var vehicleModel = Utils.DbValueToString(reader["vehicle_model"]);
            var vehicleNumber = Utils.DbValueToString(reader["vehicle_number"]);
            var driverName = Utils.DbValueToString(reader["driver_name"]);
            var state = GetOperationState(Convert.ToInt16(reader["state_id"]));
            var priority = Convert.ToInt16(reader["priority"]);
            var manager = GetEmployee(Convert.ToInt32(reader["manager_id"]));

            var operation = new Operation(Convert.ToInt64(reader["id"]), null,
                type,
                planWorkStartDate, planWorkFinishDate,
                vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, driverName,
                state,
                priority,
                manager);

            operation.FactWorkStartDate = Utils.DbValueToDateTime(reader["fact_work_start_date"]);
            operation.FactWorkFinishDate = Utils.DbValueToDateTime(reader["fact_work_finish_date"]);
            operation.StartDate = Utils.DbValueToDateTime(reader["start_date"]);
            operation.FinishDate = Utils.DbValueToDateTime(reader["finish_date"]);

            var logistId = Utils.DbValueToInt(reader["logist_id"]);
            operation.Logist = logistId > 0 ? GetLogist(logistId) : Logist.Null;

            var gateId = Utils.DbValueToInt(reader["gate_id"]);
            operation.Gate = gateId > 0 ? GetGate(gateId) : Gate.Null;

            var responcibleOfficerId = Utils.DbValueToInt(reader["responsible_officer_id"]);
            operation.ResponsibleOfficer = responcibleOfficerId > 0 ? GetEmployee(responcibleOfficerId) : Employee.Null;

            operation.Note = Convert.ToString(reader["note"]);

            var actions = GetActions(operation);
            operation.AddActions(actions);

            var workers = GetWorkers(operation);
            foreach (var pair in workers)
                operation.AddWorkers(pair.Key, pair.Value);

            return operation;
        }
        public Operation GetOperation(long id)
        {
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select o.id, " +
                        $"       o.type_id,  o.type_code, o.type_name, " +
                        $"       o.start_date, " +
                        $"       o.plan_work_start_date, o.plan_work_finish_date, " +
                        $"       o.fact_work_start_date, o.fact_work_finish_date, " +
                        $"       o.start_date, o.finish_date, " +
                        $"       o.finish_date, " +
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
                connection.Close();
            }
            throw new OperationNotFoundException(id);
        }
        public IDictionary<Position, short> GetWorkers(OperationType operationType, Entity vehicleKind)
        {
            if (operationType == null)
                throw new ArgumentNullException(nameof(operationType));
            if (vehicleKind == null)
                throw new ArgumentNullException(nameof(vehicleKind));
            if (GetPositionById == null)
                throw new ArgumentNullException(nameof(GetPositionById));
            var workers = new Dictionary<Position, short>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select position_id, workers_count from {YardSchema}.v_vk_ot_worker vkotw " +
                        $" where vkotw.operation_type_id = :operation_type_id " +
                        $"   and vkotw.vehicle_kind_id = :vehicle_kind_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_type_id", NpgsqlDbType.Smallint, operationType.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint, vehicleKind.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var position = GetPositionById(Utils.DbValueToInt(reader["position_id"]));
                        if (!workers.ContainsKey(position))
                            workers.Add(position, Utils.DbValueToShort(reader["workers_count"]));
                        else
                            workers[position] += Utils.DbValueToShort(reader["workers_count"]);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return workers;
        }
        public IDictionary<Position, short> GetWorkers(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            return GetWorkers(operation.Type, operation.VehicleKind);
        }
        public Operation[] GetOperations(DateTime from, DateTime to, Partner[]? partners)
        {
            var operations = new List<Operation>();
            using (var connection = CloneConnection())
            {
                if (partners != null && partners.Length > 0)
                {
                    var sql =
                        $"select distinct po.id, " +
                        $"       po.type_id, " +
                        $"       po.type_code, " +
                        $"       po.type_name, " +
                        $"       po.start_date, " +
                        $"       po.plan_work_start_date, " +
                        $"       po.fact_work_start_date, " +
                        $"       po.plan_work_finish_date, " +
                        $"       po.fact_work_finish_date, " +
                        $"       po.finish_date, " +
                        $"       po.workers_count, " +
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
                        $" where po.plan_work_start_date between :from_date and :to_date " +
                        $"   and po.plan_work_finish_date between :from_date and :to_date " +
                        $"   and po.partner_id = :partner_id";
                    foreach (var partner in partners)
                    {
                        using var command = connection.CreateCommand();
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
                    var sql =
                        $"select distinct o.id, " +
                        $"       o.type_id, " +
                        $"       o.type_code, " +
                        $"       o.type_name, " +
                        $"       o.plan_work_start_date, " +
                        $"       o.fact_work_start_date, " +
                        $"       o.plan_work_finish_date, " +
                        $"       o.fact_work_finish_date, " +
                        $"       o.start_date, " +
                        $"       o.finish_date, " +
                        $"       o.vehicle_kind_id, " +
                        $"       o.vehicle_kind_code, " +
                        $"       o.vehicle_kind_name, " +
                        $"       o.vehicle_brand, " +
                        $"       o.vehicle_model, " +
                        $"       o.vehicle_number, " +
                        $"       o.driver_name, " +
                        $"       o.logist_id, " +
                        $"       o.logist_code, " +
                        $"       o.logist_name, " +
                        $"       o.gate_id, " +
                        $"       o.gate_code, " +
                        $"       o.gate_name, " +
                        $"       o.state_id, " +
                        $"       o.state_code, " +
                        $"       o.state_name, " +
                        $"       o.priority, " +
                        $"       o.manager_id, " +
                        $"       o.responsible_officer_id, " +
                        $"       o.note " +
                        $"  from {YardSchema}.v_operation o " +
                        $" where o.plan_work_start_date between :from_date and :to_date " +
                        $"   and o.plan_work_finish_date between :from_date and :to_date";
                    using var command = connection.CreateCommand();
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
                connection.Close();
            }
            return operations.ToArray();
        }
        public Partner[] GetOperationsPartners(DateTime from, DateTime to)
        {
            var partners = new HashSet<Partner>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select distinct partner_id, op.plan_work_start_date, op.plan_work_finish_date " +
                        $"  from {YardSchema}.v_operation_partner op " +
                        $" where not (op.plan_work_start_date > :finish_date or op.plan_work_finish_date < :start_date)";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("start_date", NpgsqlDbType.Timestamp, from));
                    command.Parameters.Add(CreateParameter("finish_date", NpgsqlDbType.Timestamp, to));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
                        if (!partners.Contains(partner))
                            partners.Add(partner);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return partners.ToArray();
        }
        public Gate SetOperationGate(Operation operation, Gate? gate = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = $"select {YardSchema}.set_operation_gate(:p_id," +
                        $":p_gate_code);";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                    command.Parameters.Add(CreateParameter("p_gate_code", NpgsqlDbType.Varchar, gate != null ? gate.Code : DBNull.Value));
                    var gateId = Convert.ToInt32(command.ExecuteScalar());
                    operation.Gate = GetGate(gateId);
                }
                connection.Close();
            }
            return operation.Gate;
        }
        public void EditOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using (var command = connection.CreateCommand())
            {
                var sql = $"select {YardSchema}.edit_operation(:p_id," +
                    $":p_type_code," +
                    $":p_plan_work_start_date," +
                    $":p_plan_work_finish_date," +
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
                command.Parameters.Add(CreateParameter("p_plan_start_date", NpgsqlDbType.Timestamp, operation.PlanWorkStartDate));
                command.Parameters.Add(CreateParameter("p_plan_finish_date", NpgsqlDbType.Timestamp, operation.PlanWorkFinishDate));
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

            // Синхронизация складских действ (добавляем новые, удаляем удалённые)
            var actions = new Dictionary<string, WarehouseAction>();
            var indicators = new Dictionary<string, short>();
            var temp = GetActions(operation);
            foreach (var action in temp)
            {
                var key = action.Key;
                if (!actions.ContainsKey(key))
                {
                    actions.Add(key, action);
                    indicators.Add(key, -1);
                }
                else
                    indicators[key]--;
            }
            temp = operation.GetActions();
            foreach (var action in temp)
            {
                var key = action.Key;
                if (!actions.ContainsKey(key))
                {
                    actions.Add(key, action);
                    indicators.Add(key, 1);
                }
                else
                    indicators[key]++;
            }
            foreach (var action in actions)
            {
                var key = action.Key;
                if (indicators[key] < 0)
                    RemoveAction(operation, actions[key]);
                else if (indicators[key] > 0)
                    AddAction(operation, actions[key]);
            }

            connection.Close();
        }
        public void StartOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        public void SetResponsibleOfficer(Operation operation, Employee officer)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (officer == null)
                throw new ArgumentNullException(nameof(officer));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.set_operation_responsible_officer(:p_id, " +
                $":p_responsible_officer_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_responsible_officer_id", NpgsqlDbType.Integer, officer.Id));
            command.ExecuteScalar();
            operation.ResponsibleOfficer = officer;
            connection.Close();
        }
        public void StartOperationWork(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation_work(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        public void FinishOperationWork(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation_work(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        public void FinishOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        public void CancelOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.cancel_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        public void DeleteOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.delete_operation(:p_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        #endregion

        #region Actions
        public void AddAction(Operation operation, WarehouseAction action)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.add_action(:p_operation_id, " +
                $":p_action_kind, " +
                $":p_action_id, " +
                $":p_partner_id, " +
                $":p_partner_action_id);";
            command.CommandText = sql;
            var kind = action.Kind == ActionKind.Outgo ? "O" : "I";
            command.Parameters.Add(CreateParameter("p_operation_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_action_kind", NpgsqlDbType.Varchar, kind));
            command.Parameters.Add(CreateParameter("p_action_id", NpgsqlDbType.Varchar, action.Id));
            command.Parameters.Add(CreateParameter("p_partner_id", NpgsqlDbType.Varchar,
                action.Partner != null ? action.Partner.Id : DBNull.Value));
            command.Parameters.Add(CreateParameter("p_partner_action_id", NpgsqlDbType.Varchar,
                string.IsNullOrWhiteSpace(action.PartnerActionId) ? action.PartnerActionId : DBNull.Value));
            command.ExecuteScalar();
            connection.Close();
        }
        private WarehouseAction CreateWarehouseAction(DbDataReader reader)
        {
            var kind = Utils.DbValueToString(reader["action_kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var action = new WarehouseAction(kind,
                Utils.DbValueToString(reader["action_id"]),
                partner,
                Utils.DbValueToString(reader["partner_action_id"]));
            return action;
        }
        public WarehouseAction[] GetActions(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            var actions = new List<WarehouseAction>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select oa.operation_id, oa.action_kind, oa.action_id, oa.partner_id, oa.partner_action_id " +
                        $"  from {YardSchema}.v_operation_action oa " +
                        $" where oa.operation_id = :operation_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_id", NpgsqlDbType.Bigint, operation.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var action = CreateWarehouseAction(reader);
                        actions.Add(action);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return actions.ToArray();
        }
        public void RemoveAction(Operation operation, WarehouseAction action)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.remove_action(:p_operation_id," +
                $":p_action_kind," +
                $":p_action_id);";
            command.CommandText = sql;
            var kind = action.Kind == ActionKind.Outgo ? "O" : "I";
            command.Parameters.Add(CreateParameter("p_operation_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_action_kind", NpgsqlDbType.Varchar, kind));
            command.Parameters.Add(CreateParameter("p_action_id", NpgsqlDbType.Varchar, action.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        #endregion
    }
}