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
    /// <summary>
    /// Класс для работы с данными в БД PostgreeSQL
    /// </summary>
    public class YardPgSqlStorage : PgSqlStorage, IYardCommonStorage
    {
        private const string DefaultYardSchema = "yard";
        /// <summary>
        /// Схема в БД для хранение объектов модуля управления двором
        /// </summary>
        public string YardSchema { private get; set; } = DefaultYardSchema;

        public YardPgSqlStorage(string host, string database) : base(host, database)
        { }

        private static NpgsqlParameter CreateParameter(string name, NpgsqlDbType type, object value)
        {
            var parameter = new NpgsqlParameter(name, type)
            {
                Value = value
            };
            return parameter;
        }

        #region Domains
        private readonly Catalog<Gate> _gates = new("GATES", "Ворота");
        private static Gate CreateGate(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToShort(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            return new Gate(id,
                code,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                note);
        }
        public Gate GetGate(short id)
        {
            id = id > 0 ? id : (short)-1;
            var gate = _gates[id];
            if (gate != null && gate != Gate.Null && gate != Gate.Empty)
                return gate;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectGateByIdSql =
                        $"select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.is_active, g.display_order, g.note " +
                        $"  from {YardSchema}.v_gate g where g.id = :id order by g.display_order";
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
            if (gate != null && gate != Gate.Null && gate != Gate.Empty)
                return gate;
            throw new EntityNotFoundException(nameof(Gate), id);
        }
        public Gate GetGate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var gate = _gates[code];
            if (gate != null && gate != Gate.Null && gate != Gate.Empty)
                return gate;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectGateByCodeSql =
                        $"select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.is_active, g.display_order, g.note " +
                        $"  from {YardSchema}.v_gate g where g.code = :code order by g.display_order";
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
            if (gate != null && gate != Gate.Null && gate != Gate.Empty)
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
                        $"select g.id, g.code, g.name, g.short_name, g.alt_name, g.display_name, g.is_active, g.display_order, g.note " +
                        $"  from {YardSchema}.v_gate g order by g.display_order";
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
            gates.Sort();
            return gates.ToArray();
        }
        private readonly Catalog<Logist> _logists = new("LOGISTS", "Логисты");
        private static Logist CreateLogist(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt32(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            return new Logist(id,
                code,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                note);
        }
        public Logist GetLogist(int id)
        {
            id = id > 0 ? id : -1;
            var logist = _logists[id];
            if (logist != null && logist != Logist.Null && logist != Logist.Empty)
                return logist;

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectLogistByIdSql =
                        $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.is_active, l.display_order, l.note " +
                        $"  from {YardSchema}.v_logist l " +
                        $" where l.id = :id order by l.display_order";
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
            if (logist != null && logist != Logist.Null && logist != Logist.Empty)
                return logist;
            throw new EntityNotFoundException(nameof(logist), id);
        }
        public Logist GetLogist(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var logist = _logists[code];
            if (logist != null && logist != Logist.Null && logist != Logist.Empty)
                return logist;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectLogistByCodeSql =
                        $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.is_active, l.display_order, l.note " +
                        $"  from {YardSchema}.v_logist l " +
                        $" where l.code = :code order by l.display_order";
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
            if (logist != null && logist != Logist.Null && logist != Logist.Empty)
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
                        $"select l.id, l.code, l.name, l.short_name, l.alt_name, l.display_name, l.is_active, l.display_order, l.note " +
                        $"  from {YardSchema}.v_logist l order by l.display_order";
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
            logists.Sort();
            return logists.ToArray();
        }

        private readonly Catalog<Entity>? _operationStates = new("OPERATION_STATES", "Состояния (статусы) операций двора");
        public Entity GetOperationState(short id)
        {
            var SelectOperationStateByIdSql =
                $"select os.id, os.code, os.name, os.short_name, os.alt_name, os.display_name, os.is_active, os.display_order, os.note " +
                $"  from {YardSchema}.v_operation_state os where os.id = :id";
            return GetEntity(id, SelectOperationStateByIdSql, _operationStates);
        }
        public Entity GetOperationState(string code)
        {
            var SelectOperationStateByCodeSql =
                $"select os.id, os.code, os.name, os.short_name, os.alt_name, os.display_name, os.is_active, os.display_order, os.note " +
                $"from {YardSchema}.v_operation_state os where os.code = :code";
            return GetEntity(code, SelectOperationStateByCodeSql, _operationStates);
        }
        public Entity[] GetOperationStates()
        {
            var SelectOperationStatesSql =
                $"select os.id, os.code, os.name, os.short_name, os.alt_name, os.display_name, os.is_active, os.display_order, os.note " +
                $"from {YardSchema}.v_operation_state os";
            return GetEntities(SelectOperationStatesSql);
        }

        private readonly Catalog<OperationType> _operationTypes = new("OPERATION_TYPES", "Типы операций двора");
        private static OperationType CreateOperationType(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var id = Utils.DbValueToShort(reader["id"]);
            var direction = Utils.DbValueToString(reader["direction"]) == "O" ? OperationType.TypeDirection.Out : OperationType.TypeDirection.In;
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var time = Utils.DbValueToInt(reader["time"]);
            var isActive = Utils.DbValueToInt(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var operationtype = new OperationType(id,
                direction,
                code,
                name,
                shortName,
                altName,
                displayName,
                time,
                isActive,
                displayOrder,
                note);
            return operationtype;
        }
        public OperationType GetOperationType(short id)
        {
            id = id > 0 ? id : (short)-1;
            var operationType = _operationTypes[id];
            if (operationType != null && operationType != OperationType.Null && operationType != OperationType.Empty)
                return operationType;

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypeByIdSql =
                        $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, " +
                        $"       ot.time, ot.is_active, ot.display_order, ot.note " +
                        $"  from {YardSchema}.v_operation_type ot " +
                        $" where ot.id = :id order by ot.display_order";
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
            if (operationType != null && operationType != OperationType.Null && operationType != OperationType.Empty)
                return operationType;
            throw new EntityNotFoundException(nameof(operationType), id);
        }
        public OperationType GetOperationType(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var operationType = _operationTypes[code];
            if (operationType != null && operationType != OperationType.Null && operationType != OperationType.Empty)
                return operationType;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypeByCodeSql =
                        $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, " +
                        $"       ot.time, ot.is_active, ot.display_order, ot.note " +
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
            if (operationType != null && operationType != OperationType.Null && operationType != OperationType.Empty)
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
                        $"       pot.time, pot.is_active, pot.display_order, pot.note " +
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
            operationTypes.Sort();
            return operationTypes.ToArray();
        }
        public OperationType[] GetOperationTypes()
        {
            var operationTypes = new List<OperationType>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationTypesSql =
                        $"select ot.id, ot.direction, ot.code, ot.name, ot.short_name, ot.alt_name, ot.display_name, " +
                        $"       ot.time, ot.is_active, ot.display_order, ot.note " +
                        $"  from {YardSchema}.v_operation_type ot";
                    command.CommandText = SelectOperationTypesSql;
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
            operationTypes.Sort();
            return operationTypes.ToArray();
        }

        private readonly Catalog<VehicleKind> _vehicleKinds = new("VEHICLE_KINDS", "Виды автомашин");
        private static VehicleKind CreateVehicleKind(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToShort(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var capacity = Utils.DbValueToInt(reader["capacity"]);
            var palletsCount = Utils.DbValueToShort(reader["pallets_count"]);
            var bodyLength = Utils.DbValueToDouble(reader["body_length"]);
            var bodyWidth = Utils.DbValueToDouble(reader["body_width"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var vehicleKind = new VehicleKind(id, code, name, shortName, altName, displayName,
                capacity, palletsCount, bodyLength, bodyWidth,
                isActive, displayOrder,
                note);
            return vehicleKind;
        }
        public VehicleKind GetVehicleKind(short id)
        {
            id = id > 0 ? id : (short)-1;
            var vehicleKind = _vehicleKinds[id];
            if (vehicleKind != null && vehicleKind != VehicleKind.Null && vehicleKind != VehicleKind.Empty)
                return vehicleKind;

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectVehicleKindByIdSql =
                        $"select vk.id, vk.code, vk.name, vk.short_name, vk.alt_name, vk.display_name, " +
                        $"       vk.capacity, vk.pallets_count, vk.body_length, vk.body_width, " +
                        $"       vk.is_active, vk.display_order, vk.note " +
                        $"  from {YardSchema}.v_vehicle_kind vk where vk.id = :id";
                    command.CommandText = SelectVehicleKindByIdSql;
                    command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Smallint, id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        vehicleKind = CreateVehicleKind(reader);
                        _vehicleKinds.Add(vehicleKind);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (vehicleKind != null && vehicleKind != VehicleKind.Null && vehicleKind != VehicleKind.Empty)
                return vehicleKind;
            throw new EntityNotFoundException(nameof(vehicleKind), id);
        }
        public VehicleKind GetVehicleKind(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var vehicleKind = _vehicleKinds[code];
            if (vehicleKind != null && vehicleKind != VehicleKind.Null && vehicleKind != VehicleKind.Empty)
                return vehicleKind;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectVehicleKindByCodeSql =
                        $"select vk.id, vk.code, vk.name, vk.short_name, vk.alt_name, vk.display_name, " +
                        $"       vk.capacity, vk.pallets_count, vk.body_length, vk.body_width, " +
                        $"       vk.is_active, vk.display_order, vk.note " +
                        $"  from {YardSchema}.v_vehicle_kind vk where vk.code = :code";
                    command.CommandText = SelectVehicleKindByCodeSql;
                    command.Parameters.Add(CreateParameter("code", NpgsqlDbType.Varchar, code));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        vehicleKind = CreateVehicleKind(reader);
                        _vehicleKinds.Add(vehicleKind);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (vehicleKind != null && vehicleKind != VehicleKind.Null && vehicleKind != VehicleKind.Empty)
                return vehicleKind;
            throw new EntityNotFoundException(nameof(vehicleKind), code);
        }
        public VehicleKind[] GetVehicleKinds()
        {
            var vehicleKinds = new List<VehicleKind>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectVehicleKindsSql =
                        $"select vk.id, vk.code, vk.name, vk.short_name, vk.alt_name, vk.display_name, " +
                        $"       vk.capacity, vk.pallets_count, vk.body_length, vk.body_width, " +
                        $"       vk.is_active, vk.display_order, vk.note " +
                        $"  from {YardSchema}.v_vehicle_kind vk";
                    command.CommandText = SelectVehicleKindsSql;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var vehicleKind = CreateVehicleKind(reader);
                        vehicleKinds.Add(vehicleKind);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            vehicleKinds.Sort();
            return vehicleKinds.ToArray();
        }

        private readonly Catalog<Work> _works = new("WORKS", "Элементарные работы");
        private static Work CreateWork(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToShort(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var time = Utils.DbValueToShort(reader["time"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var work = new Work(id, code, name, shortName, altName, displayName, time,
                isActive, displayOrder,
                note);
            return work;
        }
        public Work GetWorkById(short id)
        {
            id = id > 0 ? id : (short)-1;
            var work = _works[id];
            if (work != null && work != Work.Null)
                return work;

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectWorkByIdSql =
                        $"select w.id, w.code, w.name, w.short_name, w.alt_name, w.display_name, " +
                        $"       w.time, " +
                        $"       w.is_active, w.display_order, w.note " +
                        $"  from {YardSchema}.v_work w where w.id = :id";
                    command.CommandText = SelectWorkByIdSql;
                    command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Smallint, id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        work = CreateWork(reader);
                        _works.Add(work);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (work != null && work != Work.Null)
                return work;
            throw new EntityNotFoundException(nameof(work), id);
        }
        public Work GetWorkByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var work = _works[code];
            if (work != null && work != Work.Null)
                return work;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectWorkByCodeSql =
                        $"select w.id, w.code, w.name, w.short_name, w.alt_name, w.display_name, " +
                        $"       w.time, " +
                        $"       w.is_active, w.display_order, w.note " +
                        $"  from {YardSchema}.v_work w where w.code = :code";
                    command.CommandText = SelectWorkByCodeSql;
                    command.Parameters.Add(CreateParameter("code", NpgsqlDbType.Varchar, code));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        work = CreateWork(reader);
                        _works.Add(work);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (work != null && work != Work.Null)
                return work;
            throw new EntityNotFoundException(nameof(work), code);
        }
        public Work[] GetWorks()
        {
            var works = new List<Work>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectWorksSql =
                        $"select w.id, w.code, w.name, w.short_name, w.alt_name, w.display_name, " +
                        $"       w.time, " +
                        $"       w.is_active, w.display_order, w.note " +
                        $"  from {YardSchema}.v_work w";
                    command.CommandText = SelectWorksSql;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var work = CreateWork(reader);
                        works.Add(work);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return works.ToArray();
        }

        public delegate Partner GetPartnerByIdDelegate(string id);
        public delegate Employee GetEmployeeByIdDelegate(int id);
        public delegate Position GetPositionByIdDelegate(short id);
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
        private Position GetPosition(short id)
        {
            return GetPositionById != null ? GetPositionById(id) : Position.Null;
        }
        #endregion

        #region Operations     
        public short GetTime(OperationType operationType, VehicleKind vehicleKind)
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
                        $"select otvkt.operation_time " +
                        $"  from {YardSchema}.v_ot_vk_time otvkt " +
                        $" where otvkt.operation_type_id = :operation_type_id " +
                        $"   and otvkt.vehicle_kind_id = :vehicle_kind_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_type_id", NpgsqlDbType.Smallint, operationType.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint, vehicleKind.Id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                        time = Utils.DbValueToShort(reader["operation_time"]);
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
                        ":p_driver_phone_number," +
                        ":p_logist_id," +
                        ":p_priority," +
                        ":p_partner_manager_name," +
                        ":p_partner_manager_phone_number," +
                        ":p_partner_manager_email," +
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
                    command.Parameters.Add(CreateParameter("p_driver_phone_number", NpgsqlDbType.Varchar, operation.DriverPhoneNumber));
                    command.Parameters.Add(CreateParameter("p_logist_id", NpgsqlDbType.Integer,
                        operation.Logist != null ? operation.Logist.Id : DBNull.Value));
                    command.Parameters.Add(CreateParameter("p_priority", NpgsqlDbType.Smallint, operation.Priority));
                    command.Parameters.Add(CreateParameter("p_partner_manager_name", NpgsqlDbType.Varchar, operation.PartnerManagerName));
                    command.Parameters.Add(CreateParameter("p_partner_manager_phone_number", NpgsqlDbType.Varchar, operation.PartnerManagerPhoneNumber));
                    command.Parameters.Add(CreateParameter("p_partner_manager_email", NpgsqlDbType.Varchar, operation.PartnerManagerEmail));
                    command.Parameters.Add(CreateParameter("p_manager_id", NpgsqlDbType.Integer, operation.Manager.Id));
                    command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));

                    operation.Id = Convert.ToInt64(command.ExecuteScalar());
                }
                var actions = operation.GetActions();
                foreach (var action in actions)
                    AddAction(operation, action);
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
            var trailerNumber = Utils.DbValueToString(reader["trailer_number"]);
            var driverName = Utils.DbValueToString(reader["driver_name"]);
            var driverPhoneNumber = Utils.DbValueToString(reader["driver_phone_number"]);
            var state = GetOperationState(Convert.ToInt16(reader["state_id"]));
            var priority = Convert.ToInt16(reader["priority"]);
            var partnerManagerName = Utils.DbValueToString(reader["partner_manager_name"]);
            var partnerManagerPhoneNumber = Utils.DbValueToString(reader["partner_manager_phone_number"]);
            var partnerManagerEmail = Utils.DbValueToString(reader["partner_manager_email"]);
            var manager = GetEmployee(Convert.ToInt32(reader["manager_id"]));

            var operation = new Operation(Convert.ToInt64(reader["id"]), null,
                type,
                planWorkStartDate, planWorkFinishDate,
                vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber, driverName, driverPhoneNumber,
                state,
                priority,
                partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
                manager)
            {
                FactWorkStartDate = Utils.DbValueToDateTime(reader["fact_work_start_date"]),
                FactWorkFinishDate = Utils.DbValueToDateTime(reader["fact_work_finish_date"]),
                StartDate = Utils.DbValueToDateTime(reader["start_date"]),
                FinishDate = Utils.DbValueToDateTime(reader["finish_date"])
            };

            var logistId = Utils.DbValueToInt(reader["logist_id"]);
            operation.Logist = logistId > 0 ? GetLogist(logistId) : Logist.Null;

            var gateId = Utils.DbValueToShort(reader["gate_id"]);
            operation.Gate = gateId > 0 ? GetGate(gateId) : Gate.Null;

            var responcibleOfficerId = Utils.DbValueToInt(reader["responsible_officer_id"]);
            operation.ResponsibleOfficer = responcibleOfficerId > 0 ? GetEmployee(responcibleOfficerId) : Employee.Null;

            operation.Note = Convert.ToString(reader["note"]);

            var actions = GetActions(operation);
            operation.AddActions(actions);

            var operationWorks = GetOperationWorks(operation);
            operation.AddOperationWorks(operationWorks);

            var operationWorkers = GetOperationWorkers(operation);
            operation.AddOperationWorkers(operationWorkers);

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
                        $"       o.vehicle_brand, o.vehicle_model, o.vehicle_number, o.trailer_number, " +
                        $"       o.driver_name, o.driver_phone_number, " +
                        $"       o.logist_id, o.logist_code, o.logist_name, " +
                        $"       o.gate_id, o.gate_code, o.gate_name, " +
                        $"       o.state_id, o.state_code, o.state_name, " +
                        $"       o.priority, " +
                        $"       o.partner_manager_name, o.partner_manager_phone_number, o.partner_manager_email, " +
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
        public Operation.OperationWork[] GetOperationWorks(OperationType operationType, VehicleKind vehicleKind)
        {
            if (operationType == null)
                throw new ArgumentNullException(nameof(operationType));
            if (vehicleKind == null)
                throw new ArgumentNullException(nameof(vehicleKind));
            if (GetPositionById == null)
                throw new ArgumentException($"{nameof(GetPositionById)} is null");
            var operationWorks = new List<Operation.OperationWork>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select otvkw.work_id, otvkw.position_id, otvkw.time, otvkw.workers_count " +
                        $"  from {YardSchema}.v_ot_vk_work otvkw " +
                        $" where otvkw.operation_type_id = :operation_type_id " +
                        $"   and otvkw.vehicle_kind_id = :vehicle_kind_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_type_id", NpgsqlDbType.Smallint, operationType.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint, vehicleKind.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var position = GetPositionById(Utils.DbValueToShort(reader["position_id"]));
                        var work = GetWorkById(Utils.DbValueToShort(reader["work_id"]));
                        var time = Utils.DbValueToShort(reader["time"]);
                        var workersCount = Utils.DbValueToShort(reader["workers_count"]);

                        var operationWork = new Operation.OperationWork(work, position, time, workersCount);
                        operationWorks.Add(operationWork);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return operationWorks.ToArray();
        }
        public Operation.OperationWork[] GetOperationWorks(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            return GetOperationWorks(operation.Type, operation.VehicleKind);
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
                        $"       po.vehicle_kind_id, " +
                        $"       po.vehicle_kind_code, " +
                        $"       po.vehicle_kind_name, " +
                        $"       po.vehicle_brand, " +
                        $"       po.vehicle_model, " +
                        $"       po.vehicle_number, " +
                        $"       po.trailer_number, " +
                        $"       po.driver_name, " +
                        $"       po.driver_phone_number, " +
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
                        $"       po.partner_manager_name, " +
                        $"       po.partner_manager_phone_number, " +
                        $"       po.partner_manager_email, " +
                        $"       po.manager_id, " +
                        $"       po.responsible_officer_id, " +
                        $"       po.note " +
                        $"  from {YardSchema}.v_partner_operation po " +
                        $" where not (po.plan_work_start_date > :to_date " +
                        $"    or po.plan_work_finish_date < :from_date) " +
                        $"   and po.partner_id = :partner_id order by po.plan_work_start_date";
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
                        $"       o.trailer_number, " +
                        $"       o.driver_name, " +
                        $"       o.driver_phone_number, " +
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
                        $"       o.partner_manager_name, " +
                        $"       o.partner_manager_phone_number, " +
                        $"       o.partner_manager_email, " +
                        $"       o.manager_id, " +
                        $"       o.responsible_officer_id, " +
                        $"       o.note " +
                        $"  from {YardSchema}.v_operation o " +
                        $" where not (o.plan_work_start_date > :to_date " +
                        $"    or o.plan_work_finish_date < :from_date)";
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
        public Operation[] GetOperations(DateTime? from, DateTime? to,
            OperationType? type,
            VehicleKind? vehicleKind,
            Entity? state,
            Gate? gate,
            Partner? partner,
            string? vehicleBrand,
            string? vehicleNumber,
            string? driverName)
        {
            var operations = new List<Operation>();

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                       $"select distinct po.id, " +
                       $"       po.type_id, po.type_code, po.type_name, " +
                       $"       po.plan_work_start_date, po.fact_work_start_date, " +
                       $"       po.plan_work_finish_date, po.fact_work_finish_date, " +
                       $"       po.start_date, po.finish_date, " +
                       $"       po.vehicle_kind_id, po.vehicle_kind_code, po.vehicle_kind_name, " +
                       $"       po.vehicle_brand, " +
                       $"       po.vehicle_model, " +
                       $"       po.vehicle_number, " +
                       $"       po.trailer_number, " +
                       $"       po.driver_name, po.driver_phone_number, " +
                       $"       po.logist_id, po.logist_code, po.logist_name, " +
                       $"       po.gate_id, po.gate_code, po.gate_name, " +
                       $"       po.state_id, po.state_code, po.state_name, " +
                       $"       po.priority, " +
                       $"       po.partner_manager_name, " +
                       $"       po.partner_manager_phone_number, " +
                       $"       po.partner_manager_email, " +
                       $"       po.manager_id, " +
                       $"       po.responsible_officer_id, " +
                       $"       po.note " +
                       $"  from {YardSchema}.v_partner_operation po " +
                       $" where not (:to_date is not null and po.plan_work_start_date > :to_date " +
                       $"         or :from_date is not null and po.plan_work_finish_date < :from_date) " +
                       $"   and (:type_id is null or :type_id = po.type_id) " +
                       $"   and (:vehicle_kind_id is null or :vehicle_kind_id = po.vehicle_kind_id) " +
                       $"   and (:state_id is null or :state_id = po.state_id) " +
                       $"   and (:gate_id is null or :gate_id = po.gate_id) " +
                       $"   and (:partner_id is null or :partner_id = po.partner_id) " +
                       $"   and (trim(coalesce(:vehicle_brand, '')) = '' or upper(po.vehicle_brand) like '%' || upper(:vehicle_brand) || '%') " +
                       $"   and (trim(coalesce(:vehicle_number, '')) = '' or upper(po.vehicle_number) like '%' || upper(:vehicle_number) || '%') " +
                       $"   and (trim(coalesce(:driver_name, '')) = '' or upper(po.driver_name) like '%' || upper(:driver_name) || '%') ";
                    command.CommandText = sql;

                    command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, from == null ? DBNull.Value : from));
                    command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, to == null ? DBNull.Value : to));
                    command.Parameters.Add(CreateParameter("type_id", NpgsqlDbType.Smallint,
                        type == null || type == OperationType.Null ? DBNull.Value : type.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint,
                        vehicleKind == null || vehicleKind == VehicleKind.Null ? DBNull.Value : vehicleKind.Id));
                    command.Parameters.Add(CreateParameter("state_id", NpgsqlDbType.Smallint,
                        state == null || state == Entity.Null ? DBNull.Value : state.Id));
                    command.Parameters.Add(CreateParameter("gate_id", NpgsqlDbType.Smallint,
                        gate == null || gate == Entity.Null ? DBNull.Value : gate.Id));
                    command.Parameters.Add(CreateParameter("partner_id", NpgsqlDbType.Varchar,
                        partner == null || partner == Partner.Null ? DBNull.Value : partner.Id));
                    command.Parameters.Add(CreateParameter("vehicle_brand", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(vehicleBrand) ? DBNull.Value : vehicleBrand.ToUpper()));
                    command.Parameters.Add(CreateParameter("vehicle_number", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(vehicleNumber) ? DBNull.Value : vehicleNumber.ToUpper()));
                    command.Parameters.Add(CreateParameter("driver_name", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(driverName) ? DBNull.Value : driverName.ToUpper()));

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
                    var gateId = Convert.ToInt16(command.ExecuteScalar());
                    operation.Gate = GetGate(gateId);
                }
                connection.Close();
            }
            return operation.Gate;
        }
        public void CancelOperationGate(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            using var connection = CloneConnection();
            using (var command = connection.CreateCommand())
            {
                var CancelOperationGateSql = $"select {YardSchema}.cancel_operation_gate(:p_id,);";
                command.CommandText = CancelOperationGateSql;
                command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                command.ExecuteNonQuery();
                operation.Gate = Gate.Null;
            }
            connection.Close();
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
                    $":p_vehicle_trailer_number," +
                    $":p_driver_name," +
                    $":p_driver_phone_number," +
                    $":p_logist_id," +
                    $":p_priority," +
                    $":p_partner_manager_name," +
                    $":p_partner_manager_phone_number," +
                    $":p_partner_manager_email," +
                    $":p_manager_id," +
                    $":p_note);";
                command.CommandText = sql;

                command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                command.Parameters.Add(CreateParameter("p_type_code", NpgsqlDbType.Varchar, operation.Type.Code));
                command.Parameters.Add(CreateParameter("p_plan_work_start_date", NpgsqlDbType.Timestamp, operation.PlanWorkStartDate));
                command.Parameters.Add(CreateParameter("p_plan_work_finish_date", NpgsqlDbType.Timestamp, operation.PlanWorkFinishDate));
                command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, operation.VehicleKind.Code));
                command.Parameters.Add(CreateParameter("p_vehicle_brand", NpgsqlDbType.Varchar, operation.VehicleBrand));
                command.Parameters.Add(CreateParameter("p_vehicle_model", NpgsqlDbType.Varchar, operation.VehicleModel));
                command.Parameters.Add(CreateParameter("p_vehicle_number", NpgsqlDbType.Varchar, operation.VehicleNumber));
                command.Parameters.Add(CreateParameter("p_vehicle_trailer_number", NpgsqlDbType.Varchar, operation.TrailerNumber));
                command.Parameters.Add(CreateParameter("p_driver_name", NpgsqlDbType.Varchar, operation.DriverName));
                command.Parameters.Add(CreateParameter("p_driver_phone_number", NpgsqlDbType.Varchar, operation.DriverPhoneNumber));
                command.Parameters.Add(CreateParameter("p_logist_id", NpgsqlDbType.Integer,
                    operation.Logist != null ? operation.Logist.Id : DBNull.Value));
                command.Parameters.Add(CreateParameter("p_priority", NpgsqlDbType.Smallint, operation.Priority));
                command.Parameters.Add(CreateParameter("p_partner_manager_name", NpgsqlDbType.Varchar, operation.PartnerManagerName));
                command.Parameters.Add(CreateParameter("p_partner_manager_phone_number", NpgsqlDbType.Varchar, operation.PartnerManagerPhoneNumber));
                command.Parameters.Add(CreateParameter("p_partner_manager_email", NpgsqlDbType.Varchar, operation.PartnerManagerEmail));
                command.Parameters.Add(CreateParameter("p_manager_id", NpgsqlDbType.Integer, operation.Manager.Id));
                command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));

                command.ExecuteScalar();
            }

            // Синхронизация складских действ (добавляем новые, удаляем удалённые)
            // TODO: ПЕРЕПИСАТЬ! ГОВНОКОД!
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
        public void SaveOperationWorkers(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            var oldWorkers = GetOperationWorkers(operation);
            var oldKeys = new HashSet<string>();
            foreach (var operationWorker in oldWorkers)
                oldKeys.Add($"{operationWorker.Worker.Id}_{operationWorker.Position.Id}");

            var newWorkers = operation.OperationWorkers;
            var newKeys = new HashSet<string>();
            foreach (var operationWorker in newWorkers)
            {
                var key = $"{operationWorker.Worker.Id}_{operationWorker.Position.Id}";
                newKeys.Add(key);
                if (!oldKeys.Contains(key))
                    AddWorker(operation, operationWorker.Worker, operationWorker.Position);
            }

            foreach (var operationWorker in oldWorkers)
            {
                var key = $"{operationWorker.Worker.Id}_{operationWorker.Position.Id}";
                if (!newKeys.Contains(key))
                    RemoveWorker(operation, operationWorker.Worker, operationWorker.Position);
            }
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
                !string.IsNullOrWhiteSpace(action.PartnerActionId) ? action.PartnerActionId : DBNull.Value));
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

        #region Workers
        public void AddWorker(Operation operation, Employee worker, Position position)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.add_operation_worker(:p_operation_id, " +
                $":p_worker_id, " +
                $":p_position_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_operation_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_worker_id", NpgsqlDbType.Integer, worker.Id));
            command.Parameters.Add(CreateParameter("p_position_id", NpgsqlDbType.Smallint, position.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        public Operation.OperationWorker[] GetOperationWorkers(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            var operationWorkers = new List<Operation.OperationWorker>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select ow.worker_id, ow.position_id " +
                        $"  from {YardSchema}.v_operation_worker ow " +
                        $" where ow.operation_id = :operation_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_id", NpgsqlDbType.Bigint, operation.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var workerId = Utils.DbValueToInt(reader["worker_id"]);
                        var positionId = Utils.DbValueToShort(reader["position_id"]);
                        var worker = GetEmployee(workerId);
                        var position = GetPosition(positionId);
                        var operationWorker = new Operation.OperationWorker(worker, position);
                        operationWorkers.Add(operationWorker);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return operationWorkers.ToArray();
        }
        public void RemoveWorker(Operation operation, Employee worker, Position position)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.remove_operation_worker(:p_operation_id, " +
                $":p_worker_id, " +
                $":p_position_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_operation_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_worker_id", NpgsqlDbType.Integer, worker.Id));
            command.Parameters.Add(CreateParameter("p_position_id", NpgsqlDbType.Smallint, position.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        #endregion
    }
}