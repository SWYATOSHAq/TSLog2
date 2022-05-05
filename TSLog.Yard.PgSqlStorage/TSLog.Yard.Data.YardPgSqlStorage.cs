using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Dynamic;
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
    public class YardPgSqlStorage : PgSqlStorage, IYardCommonStorage, IYardReportingStorage
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
        public delegate Shift GetShiftByDateDelegate(DateTime date);
        public delegate Employee[] GetEmployeesByShiftDelegate(Shift shift);
        public delegate Employee[] GetEmployeesDelegate();
        public GetPartnerByIdDelegate? GetPartnerById { private get; set; }
        public GetEmployeeByIdDelegate? GetEmployeeById { private get; set; }
        public GetPositionByIdDelegate? GetPositionById { private get; set; }
        public GetShiftByDateDelegate? GetShiftByDate { private get; set; }
        public GetEmployeesByShiftDelegate? GetEmployeesByShift { private get; set; }
        public GetEmployeesDelegate? GetEmployees { private get; set; }
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
        private Shift? GetShift(DateTime date)
        {
            return GetShiftByDate != null ? GetShiftByDate(date) : null;
        }
        private Employee[] GetShiftEmployees(Shift shift)
        {
            return GetEmployeesByShift != null ? GetEmployeesByShift(shift) : Array.Empty<Employee>();
        }
        private Employee[] GetAllEmployees()
        {
            return GetEmployees != null ? GetEmployees() : Array.Empty<Employee>();
        }
        #endregion

        #region Operations     
        public short GetDuration(OperationType operationType, VehicleKind vehicleKind)
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
                        $"select otvkt.operation_time\n" +
                        $"  from {YardSchema}.v_ot_vk_time otvkt\n" +
                        $" where otvkt.operation_type_id = :operation_type_id\n" +
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
        public int GetLaborCost(OperationType operationType, VehicleKind vehicleKind)
        {
            if (operationType == null)
                throw new ArgumentNullException(nameof(operationType));
            if (vehicleKind == null)
                throw new ArgumentNullException(nameof(vehicleKind));

            var laborCost = OperationType.DefaultLaborCost;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select otvkw.time, otvkw.workers_count\n" +
                        $"  from {YardSchema}.v_ot_vk_work otvkw\n" +
                        $" where otvkw.operation_type_id = :operation_type_id\n" +
                        $"   and otvkw.vehicle_kind_id = :vehicle_kind_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("operation_type_id", NpgsqlDbType.Smallint, operationType.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint, vehicleKind.Id));
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                        laborCost += Utils.DbValueToShort(reader["time"]) * Utils.DbValueToShort(reader["workers_count"]);
                    reader.Close();
                }
                connection.Close();
            }
            return laborCost;
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
                        $"select (yard.get_avalible_windows(:p_start_date,\n" +
                        $"    :p_finish_date,\n" +
                        $"    :p_operation_type_code,\n" +
                        $"    :p_vehicle_kind_code)).*;";
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
        public Operation AddOperation(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = $"select {YardSchema}.add_operation(:p_type_code,\n" +
                        ":p_plan_work_start_date,\n" +
                        ":p_plan_work_finish_date,\n" +
                        ":p_vehicle_kind_code,\n" +
                        ":p_vehicle_brand,\n" +
                        ":p_vehicle_model,\n" +
                        ":p_vehicle_number,\n" +
                        ":p_trailer_number,\n" +
                        ":p_driver_name,\n" +
                        ":p_driver_phone_number,\n" +
                        ":p_logist_id,\n" +
                        ":p_priority,\n" +
                        ":p_partner_manager_name,\n" +
                        ":p_partner_manager_phone_number,\n" +
                        ":p_partner_manager_email,\n" +
                        ":p_manager_id,\n" +
                        ":p_note,\n" +
                        ":p_user);";
                    command.CommandText = sql;

                    command.Parameters.Add(CreateParameter("p_type_code", NpgsqlDbType.Varchar, operation.Type.Code));
                    command.Parameters.Add(CreateParameter("p_plan_work_start_date", NpgsqlDbType.Timestamp, operation.PlanWorkStartDate));
                    command.Parameters.Add(CreateParameter("p_plan_work_finish_date", NpgsqlDbType.Timestamp, operation.PlanWorkFinishDate));
                    command.Parameters.Add(CreateParameter("p_vehicle_kind_code", NpgsqlDbType.Varchar, operation.VehicleKind.Code));
                    command.Parameters.Add(CreateParameter("p_vehicle_brand", NpgsqlDbType.Varchar, operation.VehicleBrand));
                    command.Parameters.Add(CreateParameter("p_vehicle_model", NpgsqlDbType.Varchar, operation.VehicleModel));
                    command.Parameters.Add(CreateParameter("p_trailer_number", NpgsqlDbType.Varchar, operation.TrailerNumber));
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
                    command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));

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

            var createDate = Utils.DbValueToDateTime(reader["created"]);
            var creator = Utils.DbValueToString(reader["creator"]);
            var editDate = Utils.DbValueToDateTime(reader["edited"]);
            var editor = Utils.DbValueToString(reader["editor"]);

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
                FinishDate = Utils.DbValueToDateTime(reader["finish_date"]),
                CreateDate = createDate,
                Creator = creator,
                EditDate = editDate,
                Editor = editor
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

            var operationWorkers = GetOperationEmployees(operation);
            operation.AddOperationEmployees(operationWorkers);

            return operation;
        }
        public Operation GetOperation(long id)
        {
            var operation = Operation.Null;
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select o.id,\n" +
                        $"       o.type_id,  o.type_code, o.type_name,\n" +
                        $"       o.start_date,\n" +
                        $"       o.plan_work_start_date, o.plan_work_finish_date,\n" +
                        $"       o.fact_work_start_date, o.fact_work_finish_date,\n" +
                        $"       o.start_date, o.finish_date,\n" +
                        $"       o.finish_date,\n" +
                        $"       o.vehicle_kind_id, o.vehicle_kind_code, o.vehicle_kind_name,\n" +
                        $"       o.vehicle_brand, o.vehicle_model, o.vehicle_number, o.trailer_number,\n" +
                        $"       o.driver_name, o.driver_phone_number,\n" +
                        $"       o.logist_id, o.logist_code, o.logist_name,\n" +
                        $"       o.gate_id, o.gate_code, o.gate_name,\n" +
                        $"       o.state_id, o.state_code, o.state_name,\n" +
                        $"       o.priority,\n" +
                        $"       o.partner_manager_name, o.partner_manager_phone_number, o.partner_manager_email,\n" +
                        $"       o.manager_id,\n" +
                        $"       o.responsible_officer_id, o.note,\n" +
                        $"       o.created, o.creator, o.edited, o.editor\n" +
                        $"  from {YardSchema}.v_operation o\n" +
                        $" where o.id = :id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("id", NpgsqlDbType.Bigint, id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                        operation = CreateOperation(reader);
                    reader.Close();
                }
                connection.Close();
            }
            if (operation != null && operation != Operation.Null && operation != Operation.Empty)
                return operation;
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
                        $"select otvkw.work_id, otvkw.position_id, otvkw.time, otvkw.workers_count\n" +
                        $"  from {YardSchema}.v_ot_vk_work otvkw\n" +
                        $" where otvkw.operation_type_id = :operation_type_id\n" +
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
                        $"select distinct po.id,\n" +
                        $"       po.type_id,\n" +
                        $"       po.type_code,\n" +
                        $"       po.type_name,\n" +
                        $"       po.start_date,\n" +
                        $"       po.plan_work_start_date,\n" +
                        $"       po.fact_work_start_date,\n" +
                        $"       po.plan_work_finish_date,\n" +
                        $"       po.fact_work_finish_date,\n" +
                        $"       po.finish_date,\n" +
                        $"       po.vehicle_kind_id,\n" +
                        $"       po.vehicle_kind_code,\n" +
                        $"       po.vehicle_kind_name,\n" +
                        $"       po.vehicle_brand,\n" +
                        $"       po.vehicle_model,\n" +
                        $"       po.vehicle_number,\n" +
                        $"       po.trailer_number,\n" +
                        $"       po.driver_name,\n" +
                        $"       po.driver_phone_number,\n" +
                        $"       po.logist_id,\n" +
                        $"       po.logist_code,\n" +
                        $"       po.logist_name,\n" +
                        $"       po.gate_id,\n" +
                        $"       po.gate_code,\n" +
                        $"       po.gate_name,\n" +
                        $"       po.state_id,\n" +
                        $"       po.state_code,\n" +
                        $"       po.state_name,\n" +
                        $"       po.priority,\n" +
                        $"       po.partner_manager_name,\n" +
                        $"       po.partner_manager_phone_number,\n" +
                        $"       po.partner_manager_email,\n" +
                        $"       po.manager_id,\n" +
                        $"       po.responsible_officer_id,\n" +
                        $"       po.note,\n" +
                        $"       po.created, po.creator, po.edited, po.editor\n" +
                        $"  from {YardSchema}.v_partner_operation po\n" +
                        $" where not (po.plan_work_start_date > :to_date\n" +
                        $"    or po.plan_work_finish_date < :from_date)\n" +
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
                        $"select distinct o.id,\n" +
                        $"       o.type_id,\n" +
                        $"       o.type_code,\n" +
                        $"       o.type_name,\n" +
                        $"       o.plan_work_start_date,\n" +
                        $"       o.fact_work_start_date,\n" +
                        $"       o.plan_work_finish_date,\n" +
                        $"       o.fact_work_finish_date,\n" +
                        $"       o.start_date,\n" +
                        $"       o.finish_date,\n" +
                        $"       o.vehicle_kind_id,\n" +
                        $"       o.vehicle_kind_code,\n" +
                        $"       o.vehicle_kind_name,\n" +
                        $"       o.vehicle_brand,\n" +
                        $"       o.vehicle_model,\n" +
                        $"       o.vehicle_number,\n" +
                        $"       o.trailer_number,\n" +
                        $"       o.driver_name,\n" +
                        $"       o.driver_phone_number,\n" +
                        $"       o.logist_id,\n" +
                        $"       o.logist_code,\n" +
                        $"       o.logist_name,\n" +
                        $"       o.gate_id,\n" +
                        $"       o.gate_code,\n" +
                        $"       o.gate_name,\n" +
                        $"       o.state_id,\n" +
                        $"       o.state_code,\n" +
                        $"       o.state_name,\n" +
                        $"       o.priority,\n" +
                        $"       o.partner_manager_name,\n" +
                        $"       o.partner_manager_phone_number,\n" +
                        $"       o.partner_manager_email,\n" +
                        $"       o.manager_id,\n" +
                        $"       o.responsible_officer_id,\n" +
                        $"       o.note,\n" +
                        $"       o.created, o.creator, o.edited, o.editor\n" +
                        $"  from {YardSchema}.v_operation o\n" +
                        $" where not (o.plan_work_start_date > :to_date\n" +
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
            string? driverName,
            string? partnerActionId,
            Employee[] workers)
        {
            var operations = new List<Operation>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                       $"select distinct po.id,\n" +
                       $"       po.type_id, po.type_code, po.type_name,\n" +
                       $"       po.plan_work_start_date, po.fact_work_start_date,\n" +
                       $"       po.plan_work_finish_date, po.fact_work_finish_date,\n" +
                       $"       po.start_date, po.finish_date,\n" +
                       $"       po.vehicle_kind_id, po.vehicle_kind_code, po.vehicle_kind_name,\n" +
                       $"       po.vehicle_brand,\n" +
                       $"       po.vehicle_model,\n" +
                       $"       po.vehicle_number,\n" +
                       $"       po.trailer_number,\n" +
                       $"       po.driver_name, po.driver_phone_number,\n" +
                       $"       po.logist_id, po.logist_code, po.logist_name,\n" +
                       $"       po.gate_id, po.gate_code, po.gate_name,\n" +
                       $"       po.state_id, po.state_code, po.state_name,\n" +
                       $"       po.priority,\n" +
                       $"       po.partner_manager_name,\n" +
                       $"       po.partner_manager_phone_number,\n" +
                       $"       po.partner_manager_email,\n" +
                       $"       po.manager_id,\n" +
                       $"       po.responsible_officer_id,\n" +
                       $"       po.note,\n" +
                       $"       po.created, po.creator, po.edited, po.editor\n" +
                       $"  from {YardSchema}.v_partner_operation po\n" +
                       $" where not (:to_date is not null and po.plan_work_start_date > :to_date\n" +
                       $"         or :from_date is not null and po.plan_work_finish_date < :from_date)\n" +
                       $"   and (:type_id is null or :type_id = po.type_id)\n" +
                       $"   and (:vehicle_kind_id is null or :vehicle_kind_id = po.vehicle_kind_id)\n" +
                       $"   and (:state_id is null or :state_id = po.state_id)\n" +
                       $"   and (:gate_id is null or :gate_id = po.gate_id)\n" +
                       $"   and (:partner_id is null or :partner_id = po.partner_id)\n" +
                       $"   and (trim(coalesce(:vehicle_brand, '')) = '' or upper(po.vehicle_brand) like '%' || upper(:vehicle_brand) || '%')\n" +
                       $"   and (trim(coalesce(:vehicle_number, '')) = '' or upper(po.vehicle_number) like '%' || upper(:vehicle_number) || '%')\n" +
                       $"   and (trim(coalesce(:driver_name, '')) = '' or upper(po.driver_name) like '%' || upper(:driver_name) || '%')\n" +
                       $"   and (trim(coalesce(:partner_action_id, '')) = '' or upper(po.partner_action_id) like '%' || upper(:partner_action_id) || '%')\n";
                    command.CommandText = sql;

                    // TODO: add filter by employees

                    command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, from == null ? DBNull.Value : from));
                    command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, to == null ? DBNull.Value : to));
                    command.Parameters.Add(CreateParameter("type_id", NpgsqlDbType.Smallint,
                        type == null || type == OperationType.Null ? DBNull.Value : type.Id));
                    command.Parameters.Add(CreateParameter("vehicle_kind_id", NpgsqlDbType.Smallint,
                        vehicleKind == null || vehicleKind == VehicleKind.Null ? DBNull.Value : vehicleKind.Id));
                    command.Parameters.Add(CreateParameter("state_id", NpgsqlDbType.Smallint,
                        state == null || state == Entity.Null ? DBNull.Value : state.Id));
                    command.Parameters.Add(CreateParameter("gate_id", NpgsqlDbType.Smallint,
                        gate == null || gate == Gate.Null ? DBNull.Value : gate.Id));
                    command.Parameters.Add(CreateParameter("partner_id", NpgsqlDbType.Varchar,
                        partner == null || partner == Partner.Null ? DBNull.Value : partner.Id));
                    command.Parameters.Add(CreateParameter("vehicle_brand", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(vehicleBrand) ? DBNull.Value : vehicleBrand.ToUpper()));
                    command.Parameters.Add(CreateParameter("vehicle_number", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(vehicleNumber) ? DBNull.Value : vehicleNumber.ToUpper()));
                    command.Parameters.Add(CreateParameter("driver_name", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(driverName) ? DBNull.Value : driverName.ToUpper()));
                    command.Parameters.Add(CreateParameter("partner_action_id", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(partnerActionId) ? DBNull.Value : partnerActionId.ToUpper()));

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
        public Operation[] GetActiveOperations()
        {
            var operations = new List<Operation>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectActiveOperationsSql =
                       $"  select distinct ao.id,\n" +
                       $"         ao.type_id, ao.type_code, ao.type_name,\n" +
                       $"         ao.plan_work_start_date, ao.fact_work_start_date,\n" +
                       $"         ao.plan_work_finish_date, ao.fact_work_finish_date,\n" +
                       $"         ao.start_date, ao.finish_date,\n" +
                       $"         ao.vehicle_kind_id, ao.vehicle_kind_code, ao.vehicle_kind_name,\n" +
                       $"         ao.vehicle_brand,\n" +
                       $"         ao.vehicle_model,\n" +
                       $"         ao.vehicle_number,\n" +
                       $"         ao.trailer_number,\n" +
                       $"         ao.driver_name, ao.driver_phone_number,\n" +
                       $"         ao.logist_id, ao.logist_code, ao.logist_name,\n" +
                       $"         ao.gate_id, ao.gate_code, ao.gate_name,\n" +
                       $"         ao.state_id, ao.state_code, ao.state_name,\n" +
                       $"         ao.priority,\n" +
                       $"         ao.partner_manager_name,\n" +
                       $"         ao.partner_manager_phone_number,\n" +
                       $"         ao.partner_manager_email,\n" +
                       $"         ao.manager_id,\n" +
                       $"         ao.responsible_officer_id,\n" +
                       $"         ao.note,\n" +
                       $"         ao.created, ao.creator, ao.edited, ao.editor\n" +
                       $"    from {YardSchema}.v_active_operation ao\n" +
                       $"order by ao.plan_work_start_date, ao.plan_work_finish_date, ao.fact_work_start_date, ao.fact_work_finish_date";
                    command.CommandText = SelectActiveOperationsSql;

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
        public Operation[] GetGuardOperations()
        {
            var operations = new List<Operation>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectOperationsForGuardSql =
                       $"  select distinct o.id,\n" +
                       $"         o.type_id, o.type_code, o.type_name,\n" +
                       $"         o.plan_work_start_date, o.fact_work_start_date,\n" +
                       $"         o.plan_work_finish_date, o.fact_work_finish_date,\n" +
                       $"         o.start_date, o.finish_date,\n" +
                       $"         o.vehicle_kind_id, o.vehicle_kind_code, o.vehicle_kind_name,\n" +
                       $"         o.vehicle_brand,\n" +
                       $"         o.vehicle_model,\n" +
                       $"         o.vehicle_number,\n" +
                       $"         o.trailer_number,\n" +
                       $"         o.driver_name, o.driver_phone_number,\n" +
                       $"         o.logist_id, o.logist_code, o.logist_name,\n" +
                       $"         o.gate_id, o.gate_code, o.gate_name,\n" +
                       $"         o.state_id, o.state_code, o.state_name,\n" +
                       $"         o.priority,\n" +
                       $"         o.partner_manager_name,\n" +
                       $"         o.partner_manager_phone_number,\n" +
                       $"         o.partner_manager_email,\n" +
                       $"         o.manager_id,\n" +
                       $"         o.responsible_officer_id,\n" +
                       $"         o.note,\n" +
                       $"         o.created, o.creator, o.edited, o.editor\n" +
                       $"    from {YardSchema}.v_operation_for_guard o\n" +
                       $"order by o.vehicle_number";
                    command.CommandText = SelectOperationsForGuardSql;

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
                        $"select distinct partner_id, op.plan_work_start_date, op.plan_work_finish_date\n" +
                        $"  from {YardSchema}.v_operation_partner op\n" +
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
        public Gate SetOperationGate(Operation operation, Gate? gate = null, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = 
                        $"select {YardSchema}.set_operation_gate(:p_id,\n" +
                        $"   :p_gate_code,\n" +
                        $"   :p_user);";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                    command.Parameters.Add(CreateParameter("p_gate_code", NpgsqlDbType.Varchar, gate != null ? gate.Code : DBNull.Value));
                    command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
                    var gateId = Convert.ToInt16(command.ExecuteScalar());
                    operation.Gate = GetGate(gateId);
                }
                connection.Close();
            }
            return operation.Gate;
        }
        public void CancelOperationGate(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            using var connection = CloneConnection();
            using (var command = connection.CreateCommand())
            {
                var CancelOperationGateSql = $"select {YardSchema}.cancel_operation_gate(:p_id, :p_user);";
                command.CommandText = CancelOperationGateSql;
                command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
                command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
                command.ExecuteNonQuery();
                operation.Gate = Gate.Null;
            }
            connection.Close();
        }
        public void EditOperation(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using (var command = connection.CreateCommand())
            {
                var sql = 
                    $"select {YardSchema}.edit_operation(:p_id,\n" +
                    $"    :p_type_code,\n" +
                    $"    :p_plan_work_start_date,\n" +
                    $"    :p_plan_work_finish_date,\n" +
                    $"    :p_vehicle_kind_code,\n" +
                    $"    :p_vehicle_brand,\n" +
                    $"    :p_vehicle_model,\n" +
                    $"    :p_vehicle_number,\n" +
                    $"    :p_vehicle_trailer_number,\n" +
                    $"    :p_driver_name,\n" +
                    $"    :p_driver_phone_number,\n" +
                    $"    :p_logist_id,\n" +
                    $"    :p_priority,\n" +
                    $"    :p_partner_manager_name,\n" +
                    $"    :p_partner_manager_phone_number,\n" +
                    $"    :p_partner_manager_email,\n" +
                    $"    :p_manager_id,\n" +
                    $"    :p_note,\n" +
                    $"    :p_user);";
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
                command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));

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
        public void StartOperation(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation(:p_id, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            connection.Close();
        }
        public void SetResponsibleOfficer(Operation operation, Employee officer, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (officer == null)
                throw new ArgumentNullException(nameof(officer));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.set_operation_responsible_officer(:p_id, " +
                $":p_responsible_officer_id," +
                $":p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_responsible_officer_id", NpgsqlDbType.Integer, officer.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            operation.ResponsibleOfficer = officer;
            connection.Close();
        }
        public void SaveOperationWorkers(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            var oldWorkers = GetOperationEmployees(operation);
            var oldKeys = new HashSet<string>();
            foreach (var operationWorker in oldWorkers)
                oldKeys.Add($"{operationWorker.Employee.Id}_{operationWorker.Position.Id}");

            var newWorkers = operation.OperationEmployees;
            var newKeys = new HashSet<string>();
            foreach (var operationWorker in newWorkers)
            {
                var key = $"{operationWorker.Employee.Id}_{operationWorker.Position.Id}";
                newKeys.Add(key);
                if (!oldKeys.Contains(key))
                    AddEmployee(operation, operationWorker.Employee, operationWorker.Position, 0.0, MeasureUnit.Null);
            }

            foreach (var operationWorker in oldWorkers)
            {
                var key = $"{operationWorker.Employee.Id}_{operationWorker.Position.Id}";
                if (!newKeys.Contains(key))
                    RemoveEmployee(operation, operationWorker.Employee, operationWorker.Position);
            }
        }
        public void StartOperationWork(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.start_operation_work(:p_id, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            connection.Close();
        }
        public void FinishOperationWork(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation_work(:p_id, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            connection.Close();
        }
        public void SetOperationNote(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.set_operation_note(:p_id, :p_note, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_note", NpgsqlDbType.Varchar, operation.Note));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            connection.Close();
        }
        public void FinishOperation(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.finish_operation(:p_id, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            connection.Close();
        }
        public void CancelOperation(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.cancel_operation(:p_id, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
            command.ExecuteScalar();
            connection.Close();
        }
        public void DeleteOperation(Operation operation, string user = "")
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = $"select {YardSchema}.delete_operation(:p_id, :p_user);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_user", NpgsqlDbType.Varchar, user));
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
            var sql = 
                $"select {YardSchema}.add_action(:p_operation_id,\n" +
                $"    :p_action_kind,\n" +
                $"    :p_action_id,\n" +
                $"    :p_partner_id,\n" +
                $"    :p_partner_action_id);";
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
            var id = Utils.DbValueToString(reader["action_id"]);
            var number = string.Empty;
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var partnerId = Utils.DbValueToString(reader["partner_action_id"]);
            var note = string.Empty;
            var action = new WarehouseAction(kind,
                id,
                number,
                partner,
                partnerId,
                true,
                note);
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
                        $"select oa.operation_id, oa.action_kind, oa.action_id, oa.partner_id, oa.partner_action_id\n" +
                        $"  from {YardSchema}.v_operation_action oa\n" +
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
            var sql = 
                $"select {YardSchema}.remove_action(:p_operation_id,\n" +
                $"    :p_action_kind,\n" +
                $"    :p_action_id);";
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
        public void AddEmployee(Operation operation, Employee employee, Position position, double cost, MeasureUnit costCurrency)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            if (costCurrency == null)
                throw new ArgumentNullException(nameof(costCurrency));
            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = 
                $"select {YardSchema}.add_operation_worker(:p_operation_id,\n" +
                $"    :p_worker_id,\n" +
                $"    :p_position_id,\n" +
                $"    :p_cost,\n" +
                $"    :p_cost_currency);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_operation_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_worker_id", NpgsqlDbType.Integer, employee.Id));
            command.Parameters.Add(CreateParameter("p_position_id", NpgsqlDbType.Smallint, position.Id));
            command.Parameters.Add(CreateParameter("p_cost", NpgsqlDbType.Smallint, cost > 0.0 ? cost : DBNull.Value));
            command.Parameters.Add(CreateParameter("p_cost_currency", NpgsqlDbType.Varchar, cost > 0.0 ? costCurrency.Code : DBNull.Value));
            command.ExecuteScalar();
            connection.Close();
        }
        public Operation.OperationEmployee[] GetOperationEmployees(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            var operationWorkers = new List<Operation.OperationEmployee>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        $"select ow.worker_id, ow.position_id\n" +
                        $"  from {YardSchema}.v_operation_worker ow\n" +
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
                        var operationWorker = new Operation.OperationEmployee(worker, position);
                        operationWorkers.Add(operationWorker);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return operationWorkers.ToArray();
        }
        private Interval[] GetBisyIntervals(Employee employee, DateTime from, DateTime to)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            var intervals = new List<Interval>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var SelectBisyIntervals =
                        "select wbi.employee_id,\n" +
                        "       wbi.plan_work_start_date,\n" +
                        "       wbi.plan_work_finish_date\n" +
                        "  from yard.v_worker_busy_intervals wbi\n" +
                        " where not(wbi.plan_work_start_date > :to_date or wbi.plan_work_finish_date < :from_date)\n" +
                        "   and wbi.employee_id = :employee_id";
                    command.CommandText = SelectBisyIntervals;
                    command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, from));
                    command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, to));
                    command.Parameters.Add(CreateParameter("employee_id", NpgsqlDbType.Integer, employee.Id));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var fromTime = Utils.DbValueToDateTime(reader["plan_work_start_date"]);
                        var toTime = Utils.DbValueToDateTime(reader["plan_work_finish_date"]);
                        var interval = new Interval(fromTime, toTime);
                        intervals.Add(interval);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return intervals.ToArray();
        }
        private double GetEmployeeBisyShare(Employee employee, DateTime from, DateTime to)
        {
            var result = 0.0;
            var currentInterval = new Interval(from, to);
            var intervals = GetBisyIntervals(employee, from, to);            
            foreach (var interval in intervals)
            {
                var share = interval.IntervalShare(currentInterval);
                result += share;
            }            
            return result;
        }
        public Employee[] GetLeastBisyEmployees(DateTime from, DateTime to, Position position, Employee[] emplyees, short count = 1)
        {
            if (count > emplyees.Length)
                throw new ArgumentException($"Not enough employees (has - {emplyees.Length}, need - {count})!");
            var queue = new PriorityQueue<Employee, double>();
            foreach (var employee in emplyees)
            {                
                var bisyShare = GetEmployeeBisyShare(employee, from, to);
                queue.Enqueue(employee, bisyShare);
            }
            var result = new List<Employee>();
            while (count > 0)
            {
                result.Add(queue.Dequeue());
                count--;
            }
            return result.ToArray();
        }
        public void AllocateEmployees(Operation operation, DateTime? date = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            //  Удаляем текущие связи работников с операциями двора (ПРР)
            var operationEmployees = GetOperationEmployees(operation);
            foreach (var operationEmployee in operationEmployees)
                RemoveEmployee(operation, operationEmployee.Employee, operationEmployee.Position);

            date = date == null || date == DateTime.MinValue ? DateTime.Now : date;
            date = date > operation.PlanWorkStartDate ? date : operation.PlanWorkStartDate;

            using var connection = CloneConnection();
            using (var command = connection.CreateCommand())
            {
                // Получить смену, которая соответствует началу операции
                var shift = GetShift((DateTime)date);

                // Определеить сотрудников данной смены, по указанным должностям
                //var employees = GetShiftEmployees(shift);
                var employees = GetAllEmployees();

                // Определить должности, с нужным количеством сотрудников по каждой должности
                var workersCounts = operation.PositionsWorkersCount();
                foreach (var pair in workersCounts)
                {
                    // Определяем наименее загруженных сотрудников
                    var duration = operation.PlanWorkFinishDate - operation.PlanWorkStartDate;
                    var temp = GetLeastBisyEmployees((DateTime)date,
                        (DateTime)date + duration,
                        pair.Key,
                        employees,
                        pair.Value);

                    // Назначаем сотрудников
                    foreach (var employee in temp)
                        AddEmployee(operation, employee, pair.Key, employee.Cost, employee.CostMeasureUnit);
                }
            }
            connection.Close();
        }
        public void RemoveEmployee(Operation operation, Employee employee, Position position)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            using var connection = CloneConnection();
            using var command = connection.CreateCommand();
            var sql = 
                $"select {YardSchema}.remove_operation_worker(:p_operation_id,\n" +
                $"    :p_worker_id,\n" +
                $"    :p_position_id);";
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter("p_operation_id", NpgsqlDbType.Bigint, operation.Id));
            command.Parameters.Add(CreateParameter("p_worker_id", NpgsqlDbType.Integer, employee.Id));
            command.Parameters.Add(CreateParameter("p_position_id", NpgsqlDbType.Smallint, position.Id));
            command.ExecuteScalar();
            connection.Close();
        }
        #endregion

        #region Reporting

        public dynamic[] GetYardStandardLabocCosts(DateTime fromDate, DateTime toDate)
        {
            var lines = new List<dynamic>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql = 
                        "with operation_labor as (\n" +
                        "   select o.id,\n" +
                        "          o.plan_work_start_date, o.fact_work_start_date,\n" +
                        "          o.plan_work_finish_date, o.fact_work_finish_date,\n" +
                        "          o.plan_work_finish_date - o.plan_work_start_date plan_duration,\n" +
                        "          o.fact_work_finish_date - o.fact_work_start_date fact_duration,\n" +
                        "          case\n" +
                        "              when o.fact_work_finish_date is not null and o.fact_work_start_date is not null then 1\n" +
                        "              else 0\n" +
                        "          end work_finished,\n" +
                        "          case\n" +
                        "              when extract('epoch' from least(:to_date, o.plan_work_finish_date)) - extract('epoch' from greatest(:from_date, o.plan_work_start_date)) < 0 then 0\n" +
                        "              else extract('epoch' from least(:to_date, o.plan_work_finish_date)) - extract('epoch' from greatest(:from_date, o.plan_work_start_date))\n" +
                        "          end / (extract('epoch' from o.plan_work_finish_date) - extract('epoch' from o.plan_work_start_date)) plan_k,\n" +
                        "          case\n" +
                        "              when extract('epoch' from least(:to_date, o.fact_work_finish_date)) - extract('epoch' from greatest(:from_date, o.fact_work_start_date)) < 0 then 0\n" +
                        "              else extract('epoch' from least(:to_date, o.fact_work_finish_date)) - extract('epoch' from greatest(:from_date, o.fact_work_start_date))\n" +
                        "          end / (extract('epoch' from o.fact_work_finish_date) - extract('epoch' from o.fact_work_start_date)) fact_k,\n" +
                        "          case\n" +
                        "              when o.fact_work_finish_date is null\n" +
                        "                or o.fact_work_start_date is null\n" +
                        "                or o.plan_work_finish_date is null\n" +
                        "                or o.plan_work_start_date is null\n" +
                        "                or o.plan_work_finish_date = o.plan_work_start_date then 1\n" +
                        "              else (extract('epoch' from o.fact_work_finish_date) - extract('epoch' from o.fact_work_start_date))\n" +
                        "                 / (extract('epoch' from o.plan_work_finish_date) - extract('epoch' from o.plan_work_start_date))\n" +
                        "          end k,\n" +
                        "          --o.type_id,\n" +
                        "          --o.vehicle_kind_id,\n" +
                        "          ow.position_id,\n" +
                        "          --ow.work_id,\n" +
                        "          --ow.workers_count,\n" +
                        "          (select min(oa.partner_id) from yard.v_operation_action oa where oa.operation_id = o.id) partner_id,\n" +
                        "          sum(ow.\"time\" * ow.workers_count) labor_cost\n" +
                        "     from yard.v_operation o\n" +
                        "left join yard.v_ot_vk_work ow on o.type_id = ow.operation_type_id and o.vehicle_kind_id = ow.vehicle_kind_id\n" +
                        " group by o.id,\n" +
                        "          ow.position_id,\n" +
                        "          o.plan_work_start_date, o.fact_work_start_date,\n" +
                        "          o.plan_work_finish_date, o.fact_work_finish_date\n" +
                        ")\n" +
                        "   select ol.partner_id, p.name partner_name,\n" +
                        "          ol.position_id, ps.name position_name,\n" +
                        "          round((sum(ol.labor_cost * ol.plan_k) / 3600)::numeric, 2) plan_labor_cost,\n" +
                        "          round((sum(ol.labor_cost * ol.fact_k * ol.k) / 3600)::numeric, 2) fact_labor_cost\n" +
                        "     from operation_labor ol\n" +
                        "left join yard.v_partner p on ol.partner_id = p.id\n" +
                        "left join yard.v_position ps on ol.position_id = ps.id\n" +
                        "    where ol.work_finished = 1\n" +
                        "          and(ol.position_id != 1005)\n" +
                        "          and(ol.position_id != 1008)\n" +
                        "          and(ol.position_id != 1015)\n" +
                        "          and(ol.position_id != 1017)\n" +
                        "          and not(ol.plan_work_start_date > :to_date or ol.plan_work_finish_date < :from_date)\n" +
                        " group by ol.partner_id, p.name,\n" +
                        "          ol.position_id, ps.name\n" +
                        " order by ol.partner_id,\n" +
                        "          ol.position_id";
                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, fromDate));
                    command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, toDate));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        dynamic line = new ExpandoObject();
                        line.PartnerId = Utils.DbValueToString(reader["partner_id"]);
                        line.PositionId = Utils.DbValueToShort(reader["position_id"]);
                        line.PlanLaborCost = Utils.DbValueToDouble(reader["plan_labor_cost"]);
                        line.FactLaborCost = Utils.DbValueToDouble(reader["fact_labor_cost"]);
                        lines.Add(line);
                    }                       
                    reader.Close();
                }
                connection.Close();
            }
            return lines.ToArray();
        }
        public dynamic[] GetYardAllocatedLabocCosts(DateTime fromDate, DateTime toDate)
        {
            var lines = new List<dynamic>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        "with data as (\n" +
                        "   select ow.worker_id,\n" +
                        "          ow.position_id, pos.\"name\" position_name,\n" +
                        "          par.id partner_id, par.\"name\" partner_name,\n" +
                        "          o.plan_work_start_date, o.plan_work_finish_date,\n" +
                        "          case\n" +
                        "              when extract('epoch' from least(:to_date, o.plan_work_finish_date)) - extract('epoch' from greatest(:from_date, o.plan_work_start_date)) > 0\n" +
                        "                  then extract('epoch' from least(:to_date, o.plan_work_finish_date)) -extract('epoch' from greatest(:from_date, o.plan_work_start_date))\n" +
                        "              else 0\n" +
                        "          end plan_duration,\n" +
                        "          o.fact_work_start_date, o.fact_work_finish_date,\n" +
                        "          case\n" +
                        "              when o.plan_work_start_date is not null\n" +
                        "               and o.plan_work_finish_date is not null\n" +
                        "               and o.fact_work_start_date is not null\n" +
                        "               and o.fact_work_finish_date is not null\n" +
                        "               and extract('epoch' from least(:to_date, o.fact_work_finish_date)) - extract('epoch' from greatest(:from_date, o.fact_work_start_date)) > 0\n" +
                        "                   then extract('epoch' from least(:to_date, o.fact_work_finish_date)) - extract('epoch' from greatest(:from_date, o.fact_work_start_date))\n" +
                        "              else 0\n" +
                        "          end fact_duration\n" +
                        "     from yard.v_operation_worker ow\n" +
                        "left join yard.v_position pos on ow.position_id = pos.id\n" +
                        "left join yard.v_partner par on par.id = (select oa.partner_id from yard.v_operation_action oa where oa.operation_id = ow.operation_id)\n" +
                        "left join yard.v_operation o on ow.operation_id = o.id\n" +
                        "    where not(o.plan_work_start_date >= :to_date or o.plan_work_finish_date <= :from_date)\n" +
                        "       or not(o.fact_work_start_date >= :to_date or o.fact_work_finish_date <= :from_date)\n" +
                        ")\n" +
                        "  select d.partner_id, d.partner_name,\n" +
                        "         d.position_id, d.position_name,\n" +
                        "         count(distinct d.worker_id) workers_count,\n" +
                        "         round(sum(d.plan_duration)::numeric / 3600, 2) plan_labor_cost,\n" +
                        "         round(sum(d.plan_duration)::numeric / 3600 / count(distinct d.worker_id), 2) norm_plan_labor_cost,\n" +
                        "         round(sum(d.fact_duration)::numeric / 3600, 2) fact_labor_cost,\n" +
                        "         round(sum(d.fact_duration)::numeric / 3600 / count(distinct d.worker_id), 2) norm_fact_labor_cost\n" +
                        "    from data d\n" +
                        "group by d.partner_id, d.partner_name,\n" +
                        "         d.position_id, d.position_name\n" +
                        "  having sum(d.plan_duration) <> 0 and sum(d.fact_duration) <> 0\n" + 
                        "order by d.partner_id,\n" +
                        "         d.position_id";

                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("from_date", NpgsqlDbType.Timestamp, fromDate));
                    command.Parameters.Add(CreateParameter("to_date", NpgsqlDbType.Timestamp, toDate));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        dynamic line = new ExpandoObject();
                        line.PartnerId = Utils.DbValueToString(reader["partner_id"]);
                        line.PositionId = Utils.DbValueToShort(reader["position_id"]);
                        line.WorkersCount = Utils.DbValueToInt(reader["workers_count"]);
                        line.PlanLaborCost = Utils.DbValueToDouble(reader["plan_labor_cost"]);
                        line.FactLaborCost = Utils.DbValueToDouble(reader["fact_labor_cost"]);
                        lines.Add(line);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return lines.ToArray();
        }
        public dynamic[] GetYardShiftLabocCosts(DateTime date)
        {
            var lines = new List<dynamic>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    var sql =
                        "with tempdate as (\n" +
                        "    select yard.get_shift_start(:date::timestamp, 0::int2) start_date, yard.get_shift_finish(:date::timestamp, 0::int2) finish_date\n" +
                        "),\n" +
                        "plan_data as (\n" +
                        "   select --o.id,\n" +
                        "          --o.plan_work_start_date, o.plan_work_finish_date,\n" +
                        "          --least(o.plan_work_finish_date, td.finish_date) - greatest(o.plan_work_start_date, td.start_date) plan_duration,\n" +
                        "          nw.position_id,\n" +
                        "          o.gate_id,\n" +
                        "          sum(nw.\"time\" * nw.workers_count) plan_lc\n" +
                        "     from yard.v_ot_vk_work nw\n" +
                        "left join yard.v_operation o on nw.operation_type_id = o.type_id and nw.vehicle_kind_id = o.vehicle_kind_id\n" +
                        "left join tempdate td on true\n" +
                        "    where o.fact_work_start_date is not null\n" +
                        "      and o.fact_work_finish_date is not null\n" +
                        "      and not (o.plan_work_start_date >= td.finish_date or o.plan_work_finish_date <= td.start_date)\n" +
                        " group by nw.position_id, o.gate_id\n" +
                        "),\n" +
                        "fact_data as (\n" +
                        "   select --o.id,\n" +
                        "          --o.fact_work_start_date, o.fact_work_finish_date,\n" +
                        "          --least(o.fact_work_finish_date, td.finish_date) - greatest(o.fact_work_start_date, td.start_date) fact_duration,\n" +
                        "          ow.position_id,\n" +
                        "          o.gate_id,\n" +
                        "          sum(extract('epoch' from(least(o.fact_work_finish_date, td.finish_date) - greatest(o.fact_work_start_date, td.start_date)))) fact_lc\n" +
                        "    from yard.v_operation_worker ow\n" +
                        "left join yard.v_operation o on ow.operation_id = o.id\n" +
                        "left join tempdate td on true\n" +
                        "    where o.fact_work_start_date is not null\n" +
                        "      and o.fact_work_finish_date is not null\n" +
                        "      and not (o.fact_work_start_date >= td.finish_date or o.fact_work_finish_date <= td.start_date)\n" +
                        " group by ow.position_id, o.gate_id\n" +
                        "),\n" +
                        "gate_data as (\n" +
                        "   select coalesce(pd.position_id, fd.position_id) position_id, p.name position_name,\n" +
                        "          coalesce(pd.gate_id, fd.gate_id) gate_id, g.name gate_name,\n" +
                        "          round(coalesce(pd.plan_lc::numeric / 3600::numeric, 0)::numeric, 2) plan_lc,\n" +
                        "          round(coalesce(fd.fact_lc::numeric / 3600::numeric, 0)::numeric, 2) fact_lc,\n" +
                        "          case\n" +
                        "              when coalesce(fd.fact_lc, 0) <> 0 then coalesce(pd.plan_lc, 0) / coalesce(fd.fact_lc, 0) * 100\n" +
                        "              else 0\n" +
                        "          end perfomance\n" +
                        "     from plan_data pd\n" +
                        "full join fact_data fd on pd.position_id = fd.position_id and pd.gate_id = fd.gate_id\n" +
                        "left join yard.v_position p on coalesce(pd.position_id, fd.position_id) = p.id\n" +
                        "left join yard.v_gate g on coalesce(pd.gate_id, fd.gate_id) = g.id\n" +
                        "    where p.is_linear = 1\n" +
                        " order by coalesce(pd.position_id, fd.position_id),\n" +
                        "          coalesce(pd.gate_id, fd.gate_id)\n" +
                        ")\n" +
                        "  select gd.position_id, gd.position_name,\n" +
                        "         round(case\n" +
                        "             when sum(coalesce(gd.fact_lc, 0)) <> 0 then sum(coalesce(gd.plan_lc, 0)) / sum(coalesce(gd.fact_lc, 0)) * 100\n" +
                        "             else 0\n" +
                        "         end::numeric, 2) total,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1001 then gd.perfomance::numeric end), 2), 0.0) pg1,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1002 then gd.perfomance::numeric end), 2), 0.0) pg2,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1003 then gd.perfomance::numeric end), 2), 0.0) pg3,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1004 then gd.perfomance::numeric end), 2), 0.0) pg4,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1005 then gd.perfomance::numeric end), 2), 0.0) pg5,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1006 then gd.perfomance::numeric end), 2), 0.0) pg6,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1007 then gd.perfomance::numeric end), 2), 0.0) pg7,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1008 then gd.perfomance::numeric end), 2), 0.0) pg8,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1009 then gd.perfomance::numeric end), 2), 0.0) pg9,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1010 then gd.perfomance::numeric end), 2), 0.0) pg10,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1011 then gd.perfomance::numeric end), 2), 0.0) pg11,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1012 then gd.perfomance::numeric end), 2), 0.0) pg12,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1013 then gd.perfomance::numeric end), 2), 0.0) pg13,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1014 then gd.perfomance::numeric end), 2), 0.0) pg14,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1015 then gd.perfomance::numeric end), 2), 0.0) pg15,\n" +
                        "         coalesce(round(avg(case when gd.gate_id = 1016 then gd.perfomance::numeric end), 2), 0.0) pg16\n" +
                        "    from gate_data gd\n" +
                        "group by gd.position_id, gd.position_name";

                    command.CommandText = sql;
                    command.Parameters.Add(CreateParameter("date", NpgsqlDbType.Timestamp, date));
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        dynamic line = new ExpandoObject();
                        line.PositionId = Utils.DbValueToShort(reader["position_id"]);
                        line.LaborCost = Utils.DbValueToDouble(reader["total"]);

                        line.G01LaborCost = Utils.DbValueToDouble(reader["pg1"]);
                        line.G02LaborCost = Utils.DbValueToDouble(reader["pg2"]);
                        line.G03LaborCost = Utils.DbValueToDouble(reader["pg3"]);
                        line.G04LaborCost = Utils.DbValueToDouble(reader["pg4"]);
                        line.G05LaborCost = Utils.DbValueToDouble(reader["pg5"]);
                        line.G06LaborCost = Utils.DbValueToDouble(reader["pg6"]);
                        line.G07LaborCost = Utils.DbValueToDouble(reader["pg7"]);
                        line.G08LaborCost = Utils.DbValueToDouble(reader["pg8"]);
                        line.G09LaborCost = Utils.DbValueToDouble(reader["pg9"]);
                        line.G10LaborCost = Utils.DbValueToDouble(reader["pg10"]);
                        line.G11LaborCost = Utils.DbValueToDouble(reader["pg11"]);
                        line.G12LaborCost = Utils.DbValueToDouble(reader["pg12"]);
                        line.G13LaborCost = Utils.DbValueToDouble(reader["pg13"]);
                        line.G14LaborCost = Utils.DbValueToDouble(reader["pg14"]);
                        line.G15LaborCost = Utils.DbValueToDouble(reader["pg15"]);
                        line.G16LaborCost = Utils.DbValueToDouble(reader["pg16"]);

                        lines.Add(line);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return lines.ToArray();
        }

        #endregion
    }
}