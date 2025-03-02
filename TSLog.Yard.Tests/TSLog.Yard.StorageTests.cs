using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Reflection;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using TSLog.Yard.Data;

namespace TSLog.Yard.Tests;

[TestClass]
public class StorageTests : IDisposable
{
    private readonly string _oracleTns = string.Empty;
    private readonly string _oracleUserName = string.Empty;
    private readonly string _oraclePassword = string.Empty;
    private readonly string _oracleCommonSchema;
    private readonly string _oracleStaffSchema;
    private readonly string _oracleLegalEntitiesSchema;
    private readonly string _oraclePartnersSchema;

    private readonly string _pgsqlHost = string.Empty;
    private readonly string _pgsqlAdminUserName = string.Empty;
    private readonly string _pgsqlUserName = string.Empty;
    private readonly string _pgsqlPassword = string.Empty;
    private readonly string _pgsqlDatabase = string.Empty;
    private readonly bool _pgsqlPooling;
    private readonly int _pgsqlMaxPoolSize;
    private readonly int _pgsqlConnectionIdleLifetime;
    private readonly int _pgsqlConnectionPruningInterval;
    private readonly string _pgsqlYardSchema;

    private readonly string DefaultUser = "m.burov";
    private readonly IYardStorage _storage;
    private readonly LoggerFactory _loggerFactory = new();


    public StorageTests()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        var configuration = builder.Build();

        var oracleSection = configuration.GetSection("OracleConnectionDebugSettings");
        _oracleTns = GetStringValue(oracleSection, "DataSource");
        _oracleUserName = GetStringValue(oracleSection, "UserName");
        _oraclePassword = GetStringValue(oracleSection, "Password");
        _oracleCommonSchema = GetStringValue(oracleSection, "CommonSchema");
        _oracleStaffSchema = GetStringValue(oracleSection, "StaffSchema");
        _oracleLegalEntitiesSchema = GetStringValue(oracleSection, "LegalEntitiesSchema");
        _oraclePartnersSchema = GetStringValue(oracleSection, "PartnersSchema");

        var pgsqlSection = configuration.GetSection("PostgreSqlConnectionDebugSettings");
        _pgsqlHost = GetStringValue(pgsqlSection, "Host");
        _pgsqlAdminUserName = GetStringValue(pgsqlSection, "AdminUserName");
        _pgsqlUserName = GetStringValue(pgsqlSection, "UserName");
        _pgsqlPassword = GetStringValue(pgsqlSection, "Password");
        _pgsqlDatabase = GetStringValue(pgsqlSection, "Database");
        _pgsqlPooling = GetBoolValue(pgsqlSection, "Pooling");
        _pgsqlMaxPoolSize = GetIntValue(pgsqlSection, "MaxPoolSize");
        _pgsqlConnectionIdleLifetime = GetIntValue(pgsqlSection, "ConnectionIdleLifetime");
        _pgsqlConnectionPruningInterval = GetIntValue(pgsqlSection, "ConnectionPruningInterval");
        _pgsqlYardSchema = GetStringValue(pgsqlSection, "YardSchema");

        var extensionParameters = new Dictionary<string, object>
        {
            { "Pooling", _pgsqlPooling },
            { "MaxPoolSize", _pgsqlMaxPoolSize },
            { "ConnectionIdleLifetime", _pgsqlConnectionIdleLifetime },
            { "ConnectionPruningInterval", _pgsqlConnectionPruningInterval }
        };

        _storage = new YardStorage(_oracleTns, _pgsqlHost, _pgsqlDatabase, _loggerFactory, extensionParameters);
        _storage.SetOracleCommonSchema(_oracleCommonSchema);
        _storage.SetOracleStaffSchema(_oracleStaffSchema);
        _storage.SetOracleLegalEntitiesSchema(_oracleLegalEntitiesSchema);
        _storage.SetOraclePartnersSchema(_oraclePartnersSchema);
        
        _storage.SetPgsqlYardSchema(_pgsqlYardSchema);
    }

    [TestInitialize]
    public void Setup()
    {
        Connect(true);
        _storage.TruncateDataTables();
        Disconnect();
        Connect();
    }
    [TestCleanup]
    public void TearDown()
    {
        Disconnect();
    }

    private static string GetStringValue(IConfigurationSection section, string name)
    {
        return section.GetValue<string>(name) ?? string.Empty;
    }
    private static bool GetBoolValue(IConfigurationSection section, string name)
    {
        return section.GetValue<bool>(name);
    }
    private static int GetIntValue(IConfigurationSection section, string name)
    {
        return section.GetValue<int>(name);
    }

    private void Connect(bool isAdmin = false)
    {
        _storage.ConnectOracle(_oracleUserName, _oraclePassword);
        _storage.ConnectPgSql(isAdmin ? _pgsqlAdminUserName : _pgsqlUserName, _pgsqlPassword);
    }

    private void Disconnect()
    {
        _storage.DisconnectOracle();
        _storage.DisconnectPgsql();
    }

    private static string GetPgSqlConnectionString(IYardStorage storage)
    {
        var type = typeof(YardStorage);
        var fieldInfo = type.GetField("_pgsqlStorage", BindingFlags.NonPublic | BindingFlags.Instance);
        var pgSqlStorage = (YardPgSqlStorage)fieldInfo.GetValue(storage);

        type = typeof(YardPgSqlStorage);
        fieldInfo = type.GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(fieldInfo);
        Assert.IsNotNull(fieldInfo.GetValue(pgSqlStorage));
        Assert.IsTrue(fieldInfo.GetValue(pgSqlStorage) is NpgsqlConnection);
        var connection = (NpgsqlConnection)fieldInfo.GetValue(pgSqlStorage);

        return connection.ConnectionString ?? string.Empty;
    }
    [TestMethod]
    public void CheckPgSqlConnectionSettings()
    {
        // Этот тест проверяет, что дополнительные параметры PGSQL соединения корректно применились.
        // Речь идёт о параметрах: Pooling, MaxPoolSize, ConnectionPruningInterval.
        var pgsqlConnectionString = GetPgSqlConnectionString(_storage);
        var pgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(pgsqlConnectionString);
        Assert.AreEqual(_pgsqlPooling, pgsqlConnectionStringBuilder.Pooling);
        Assert.AreEqual(_pgsqlMaxPoolSize, pgsqlConnectionStringBuilder.MaxPoolSize);

        // TODO: Fix it!
        //Assert.IsTrue(pgsqlConnectionStringBuilder.ConnectionIdleLifetime == _pgsqlConnectionIdleLifetime);
        
        Assert.IsTrue(pgsqlConnectionStringBuilder.ConnectionPruningInterval == _pgsqlConnectionPruningInterval);
    }
    [TestMethod]
    public void CheckPgSqlCustomConnectionSettings()
    {
        // Этот тест создаёт отдельный YardStorage с особыми параметрами подключения к PGSQL (Pooling = false, и не только ...)
        // и проверяет что доп. параметры корректно применились.
        var extensionParameters = new Dictionary<string, object>
        {
            { "Pooling", false },
            { "MaxPoolSize", 100 },
            { "ConnectionIdleLifetime", 300 },
            { "ConnectionPruningInterval", 10 }
        };

        using var storage = new YardStorage(_oracleTns, _pgsqlHost, _pgsqlDatabase, _loggerFactory, extensionParameters);
        storage.SetOracleCommonSchema(_oracleCommonSchema);
        storage.SetOracleStaffSchema(_oracleStaffSchema);
        storage.SetOracleLegalEntitiesSchema(_oracleLegalEntitiesSchema);
        storage.SetOraclePartnersSchema(_oraclePartnersSchema);
        storage.SetPgsqlYardSchema(_pgsqlYardSchema);
        storage.ConnectPgSql(_pgsqlUserName, _pgsqlPassword);

        var pgsqlConnectionString = GetPgSqlConnectionString(storage);
        var pgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(pgsqlConnectionString);
        Assert.IsFalse(pgsqlConnectionStringBuilder.Pooling);
        Assert.AreEqual(100, pgsqlConnectionStringBuilder.MaxPoolSize);

        // TODO: Fix it!
        //Assert.IsTrue(pgsqlConnectionStringBuilder.ConnectionIdleLifetime == _pgsqlConnectionIdleLifetime);
        
        Assert.AreEqual(10, pgsqlConnectionStringBuilder.ConnectionPruningInterval);
        storage.DisconnectPgsql();
    }
    [TestMethod]
    public void OperationFiltering()
    {
        // Фильтрация операций двора (ПРР) в БД по различным параметрам.
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);

        var filterFromDate = DateTime.Now.AddDays(+1);
        var filterToDate = DateTime.Now.AddDays(+8);
        var filteredOperations = _storage.GetOperations(string.Empty, filterFromDate, filterToDate, null, null, null, null, null,
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, []);
        Assert.IsTrue(filteredOperations.Length > 0);
    }
    private Entity GetOperationStateById(short id)
    {
        var operationState = _storage.GetOperationState(id);
        Assert.AreEqual(id, operationState.Id);
        return operationState;
    }
    private Entity GetOperationStateByCode(string code)
    {
        Assert.IsTrue(!string.IsNullOrWhiteSpace(code));
        var operationState = _storage.GetOperationState(code);
        Assert.AreEqual(code, operationState.Code);
        return operationState;
    }
    private BO.OperationType GetOperationTypeById(short id)
    {
        var operationType = _storage.GetOperationType(id);
        Assert.AreEqual(id, operationType.Id);
        return operationType;
    }
    private BO.OperationType GetOperationTypeByCode(string code)
    {
        Assert.IsTrue(!string.IsNullOrWhiteSpace(code));
        var operationType = _storage.GetOperationType(code);
        Assert.AreEqual(code, operationType.Code);
        return operationType;
    }
    private VehicleKind GetVehicleKindById(short id)
    {
        var vehicleKind = _storage.GetVehicleKind(id);
        Assert.AreEqual(id, vehicleKind.Id);
        return vehicleKind;
    }
    private VehicleKind GetVehicleKindByCode(string code)
    {
        Assert.IsTrue(!string.IsNullOrWhiteSpace(code));
        var vehicleKind = _storage.GetVehicleKind(code);
        Assert.AreEqual(code, vehicleKind.Code);
        return vehicleKind;
    }
    private Gate GetGateById(short id)
    {
        var gate = _storage.GetGate(id);
        Assert.AreEqual(id, gate.Id);
        return gate;
    }
    private Gate GetGateByCode(string code)
    {
        var gate = _storage.GetGate(code);
        Assert.AreEqual(code, gate.Code);
        return gate;
    }
    private Employee GetEmployee(int id)
    {
        var employee = _storage.GetEmployee(id);
        Assert.IsNotNull(employee);
        return employee;
    }
    private short GetDuration(BO.OperationType operationType, VehicleKind vehicleKind)
    {
        var duration = _storage.GetDuration(operationType, vehicleKind, out _, out _);
        Assert.IsTrue(duration > 0);
        return duration;
    }
    private Partner GetPartnerById(string id)
    {
        Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
        var partner = _storage.GetPartner(id);
        Assert.AreEqual(id, partner.Code);
        return partner;
    }
    private WarehouseAction GetWarehouseAction(ActionKind kind, Partner partner, string number)
    {
        var action = _storage.GetWarehouseAction(kind, partner, number);
        Assert.IsNotNull(action);
        return action;
    }
    [TestMethod]
    public void GetOperationState()
    {
        // Загрузка состояния операций по id и по коду.
        var operationState1 = GetOperationStateById(1001);
        var operationState2 = GetOperationStateByCode(operationState1.Code);
        Assert.AreEqual(operationState2, operationState1);
    }
    [TestMethod]
    public void GetOperationType()
    {
        // Загрузка типа операций по id и по коду.
        var operationType1 = GetOperationTypeById(1001);
        var operationType2 = GetOperationTypeByCode(operationType1.Code);
        Assert.AreEqual(operationType2, operationType1);
    }
    [TestMethod]
    public void GetVehicleKind()
    {
        // Загрузка типа тип машины по id и по коду.
        var vehicleKind1 = GetVehicleKindById(1001);
        var vehicleKind2 = GetVehicleKindByCode(vehicleKind1.Code);
        Assert.AreEqual(vehicleKind2, vehicleKind1);
    }
    [TestMethod]
    public void GetGate()
    {
        // Загрузка ворота по id и по коду.
        var gate1 = GetGateById(1001);
        var gate2 = GetGateByCode(gate1.Code);
        Assert.AreEqual(gate2, gate1);
    }
    [TestMethod]
    public void GetEmployee()
    {
        // Загрузка сотрудника по id.
        GetEmployee(100004);
    }
    [TestMethod]
    public void GetDuration()
    {
        var operationType = GetOperationTypeByCode("O_BOX_MIX");
        var vehicleKind = GetVehicleKindByCode("20_TONS");
        GetDuration(operationType, vehicleKind);
    }
    [TestMethod]
    public void GetIntervals()
    {
        // Проверка выгрузки свободных интервалов.
        var operationType = GetOperationTypeById(1001);
        var vehicleKind = GetVehicleKindById(1001);
        var gate = GetGateById(1001);

        var intervals = _storage.GetAvalibleIntervals(DateTime.Now.AddDays(-20), DateTime.Now.AddDays(+20),
            operationType, vehicleKind);
        Assert.IsTrue(intervals.Length > 0);

        intervals = _storage.GetGateIntervals(gate, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
        Assert.IsTrue(intervals.Length > 0);

        intervals = _storage.GetAllIntervals(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
        Assert.IsTrue(intervals.Length > 0);

        var union = Interval.Union(intervals);
        Assert.AreEqual(1, union.Length);
    }
    [TestMethod]
    public void GetPartner()
    {
        // Загрузка клиента по id.
        GetPartnerById("0003");
    }
    [TestMethod]
    public void GetPartnerWithManagers()
    {
        // Загрузка партнера по mndnr и информации по партнеру.
        var partner = GetPartnerById("0003");
        var managers = _storage.GetPartnerManagers(partner);
        Assert.IsTrue(managers.Length > 0);
    }
    [TestMethod]
    public void GetOperations()
    {
        // Проверка выгрузки операций.
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);

        var operations = _storage.GetOperations(DateTime.Now.AddDays(+1), DateTime.Now.AddDays(+8), null);
        Assert.IsTrue(operations.Length > 0);
    }


    private Operation CreateSomeOperation()
    {
        var state = GetOperationStateByCode("CREATED");
        var type = GetOperationTypeByCode("O_BOX_MIX");
        var vehicleKind = GetVehicleKindByCode("20_TONS");
        var manager = GetEmployee(100004);
        var duration = GetDuration(type, vehicleKind);

        var startDate = DateTime.Now.AddDays(3);
        var finishDate = startDate;
        finishDate = finishDate.Add(new TimeSpan(0, 0, duration));

        var operation = new Operation(-1, null, type, startDate, finishDate,
            vehicleKind, "some brand", "some model", "ABCDEF", "trailer number",
            "Ivan Ivanov", "123-456-789",
            state, 1,
            "Ivanov", "Ivan", "Ivanovich",
            manager);

        operation = _storage.AddOperation(operation, DefaultUser);
        Assert.IsTrue(operation.Id > 0);

        return operation;
    }
    private void AddSomeOperationWarehouseActions_Ok(Operation operation, string partnerActionNumber = "ВБ00-008867")
    {
        operation = _storage.GetOperation(operation.Id);
        var partner = GetPartnerById("0003");
        var newAction = GetWarehouseAction(ActionKind.Outgo, partner, partnerActionNumber);
        var oldActionsCount = operation.GetActions().Length;
        operation.AddAction(newAction);

        var actions = operation.GetActions();
        Assert.AreEqual(oldActionsCount + 1, actions.Length);
        var partners = operation.GetPartners();
        Assert.IsTrue(partners.Length > 0);

        _storage.AddAction(operation, newAction);

        var updatedOperation = _storage.GetOperation(operation.Id);
        Assert.AreEqual(updatedOperation.ActionsCount, operation.ActionsCount);

        foreach (var warehouseAction in actions)
        {
            var addedWarehouseAction = updatedOperation.GetAction(warehouseAction.Kind, warehouseAction.Id);
            Assert.IsNotNull(addedWarehouseAction);
            Assert.AreEqual(warehouseAction.Kind, addedWarehouseAction.Kind);
            Assert.AreEqual(warehouseAction.Id, addedWarehouseAction.Id);
            Assert.AreEqual(warehouseAction.Partner, addedWarehouseAction.Partner);
            Assert.AreEqual(warehouseAction.PartnerActionId, addedWarehouseAction.PartnerActionId);
        }
    }
    private void AddSomeOperationWarehouseActions_Error(Operation operation, string partnerActionNumber = "ВБ00-008867")
    {
        operation = _storage.GetOperation(operation.Id);
        var partner = GetPartnerById("0003");
        var action = GetWarehouseAction(ActionKind.Outgo, partner, partnerActionNumber);

        Assert.ThrowsExactly<PostgresException>(() => _storage.AddAction(operation, action));
    }
    private void EditSomeOperation_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);

        var oldDriverName = operation.DriverName;
        var newDriverName = "Petrov Petr Petrovich";
        var oldDriverPhoneNumber = operation.DriverPhoneNumber;
        var newDriverPhoneNumber = "987-654-321";

        operation.DriverName = newDriverName;
        Assert.AreNotEqual(oldDriverName, operation.DriverName);
        Assert.AreEqual(newDriverName, operation.DriverName);
        operation.DriverPhoneNumber = newDriverPhoneNumber;
        Assert.AreNotEqual(oldDriverPhoneNumber, operation.DriverPhoneNumber);
        Assert.AreEqual(newDriverPhoneNumber, operation.DriverPhoneNumber);

        _storage.EditOperation(operation);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual(newDriverName, operation.DriverName);
        Assert.AreEqual(newDriverPhoneNumber, operation.DriverPhoneNumber);

        // Проверяем, что не создался дубликат запси.
        var operations = _storage.GetOperations(DateTime.Now.AddYears(-64), DateTime.Now.AddYears(64), []);
        Assert.AreEqual(1, operations.Length);
    }
    private void EditSomeOperation_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);

        var oldDriverName = operation.DriverName;
        var newDriverName = "Petrov Petr Petrovich";
        var oldDriverPhoneNumber = operation.DriverPhoneNumber;
        var newDriverPhoneNumber = "987-654-321";

        operation.DriverName = newDriverName;
        Assert.AreNotEqual(oldDriverName, operation.DriverName);
        Assert.AreEqual(newDriverName, operation.DriverName);
        operation.DriverPhoneNumber = newDriverPhoneNumber;
        Assert.AreNotEqual(oldDriverPhoneNumber, operation.DriverPhoneNumber);
        Assert.AreEqual(newDriverPhoneNumber, operation.DriverPhoneNumber);

        Assert.ThrowsExactly<PostgresException>(() => _storage.EditOperation(operation));

        // Проверяем, что не создался дубликат запси.
        var operations = _storage.GetOperations(DateTime.Now.AddYears(-64), DateTime.Now.AddYears(64),
            Array.Empty<Partner>());
        Assert.AreEqual(1, operations.Length);
    }
    private void SetSomeOperationGate_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.SetOperationGate(operation, null, DefaultUser);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual("CONFIRMED", operation.State.Code);
        Assert.IsNotNull(operation.Gate);
    }
    private void SetSomeOperationGate_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _ = _storage.SetOperationGate(operation, null, DefaultUser));
    }
    private void StartSomeOperation_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.StartOperation(operation, DefaultUser);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual("STARTED", operation.State.Code);
        Assert.IsTrue(operation.StartDate != DateTime.MinValue);
    }
    private void StartSomeOperation_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.StartOperation(operation, DefaultUser));

    }
    private void AddSomeOperationWorkers_Ok(Operation operation, int workerId = 100065)
    {
        //var workerId = 100065; // Алимардонов Шахром Хушназаррвич.
        var stringPositionCode = "EXT_LOADER";

        operation = _storage.GetOperation(operation.Id);
        int oldCount = _storage.GetOperationWorkers(operation).Length;
        var worker = _storage.GetEmployee(workerId);
        var position = _storage.GetPosition(stringPositionCode);
        _storage.AddOperationWorker(operation, worker, position, 0.0, MeasureUnit.Null);
        var workers = _storage.GetOperationWorkers(operation);
        Assert.AreEqual(workers.Length - 1, oldCount);
    }
    private void AddSomeOperationWorkers_Error(Operation operation, int workerId = 100065)
    {
        //var workerId = 100065; // Алимардонов Шахром Хушназаррвич.
        var stringPositionCode = "EXT_LOADER";

        operation = _storage.GetOperation(operation.Id);
        var worker = _storage.GetEmployee(workerId);
        var position = _storage.GetPosition(stringPositionCode);
        Assert.ThrowsExactly<PostgresException>(() => _storage.AddOperationWorker(operation, worker, position, 0.0, MeasureUnit.Null));
    }
    private void RemoveSomeOperationWorkers_Ok(Operation operation, int workerId = 100065)
    {
        //var workerId = 100065; // Алимардонов Шахром Хушназаррвич.
        var stringPositionCode = "EXT_LOADER";

        operation = _storage.GetOperation(operation.Id);
        int oldCount = _storage.GetOperationWorkers(operation).Length;
        var worker = _storage.GetEmployee(workerId);
        var position = _storage.GetPosition(stringPositionCode);
        _storage.RemoveOperationWorker(operation, worker, position);
        var workers = _storage.GetOperationWorkers(operation);
        Assert.AreEqual(workers.Length + 1, oldCount);
    }
    private void RemoveSomeOperationWorkers_Error(Operation operation, int workerId = 100065)
    {
        //var workerId = 100065; // Алимардонов Шахром Хушназаррвич.
        var stringPositionCode = "EXT_LOADER";

        operation = _storage.GetOperation(operation.Id);
        var worker = _storage.GetEmployee(workerId);
        var position = _storage.GetPosition(stringPositionCode);
        Assert.ThrowsExactly<PostgresException>(() => _storage.RemoveOperationWorker(operation, worker, position));
    }
    private void StartSomeOperationWork_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.StartOperationWork(operation, DefaultUser);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual("WORK_STARTED", operation.State.Code);
        Assert.IsTrue(operation.FactWorkStartDate !=DateTime.MinValue);
    }
    private void StartSomeOperationWork_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.StartOperationWork(operation, DefaultUser));
    }
    private void FinishSomeOperationWork_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.FinishOperationWork(operation, DefaultUser);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual("WORK_FINISHED", operation.State.Code);
        Assert.IsTrue(operation.FactWorkFinishDate != DateTime.MinValue);
    }
    private void FinishSomeOperationWork_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.FinishOperationWork(operation, DefaultUser));
    }
    private void FinishSomeOperation_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.FinishOperation(operation, DefaultUser);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual("FINISHED", operation.State.Code);
        Assert.IsTrue(operation.FinishDate != DateTime.MinValue);
    }
    private void FinishSomeOperation_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.FinishOperation(operation, DefaultUser));
    }
    private void CancelSomeOperation_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.CancelOperation(operation, DefaultUser);
        operation = _storage.GetOperation(operation.Id);
        Assert.AreEqual("CANCELED", operation.State.Code);
    }
    private void CancelSomeOperation_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.CancelOperation(operation, DefaultUser));
    }
    private void DeleteSomeOperation_Ok(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        _storage.DeleteOperation(operation, DefaultUser);
    }
    private void DeleteSomeOperation_Error(Operation operation)
    {
        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.DeleteOperation(operation, DefaultUser));
    }

    [TestMethod]
    public void CreateOperationWithWarehouseActions()
    {
        // Сборка тестовой операции.
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
    }
    [TestMethod]
    public void AddCreatedOperationWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
    }
    [TestMethod]
    public void AddCreatedOperationSameWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008867");
        AddSomeOperationWarehouseActions_Error(operation, "ВБ00-008867");
    }
    [TestMethod]
    public void AddConfirmedOperationWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008867");
        SetSomeOperationGate_Ok(operation);
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008864");
    }
    [TestMethod]
    public void AddStartedOperationWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008867");
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008864");
    }
    [TestMethod]
    public void AddWorkStartedOperationWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008867");
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        AddSomeOperationWarehouseActions_Error(operation, "ВБ00-008864");
    }
    [TestMethod]
    public void AddWorkFinishedOperationWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008867");
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        FinishSomeOperationWork_Ok(operation);
        AddSomeOperationWarehouseActions_Error(operation, "ВБ00-008864");
    }
    [TestMethod]
    public void AddFinishedOperationWarehouseAction()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation, "ВБ00-008867");
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        FinishSomeOperationWork_Ok(operation);
        FinishSomeOperation_Ok(operation);
        AddSomeOperationWarehouseActions_Error(operation, "ВБ00-008864");
    }

    [TestMethod]
    public void EditOperation()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        EditSomeOperation_Ok(operation);
    }
    [TestMethod]
    public void EditConfirmedOperation()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        EditSomeOperation_Ok(operation);
    }
    [TestMethod]
    public void EditStartedOperation()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        EditSomeOperation_Ok(operation);
    }
    [TestMethod]
    public void EditWorkStartedOperation()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        EditSomeOperation_Error(operation);
    }
    [TestMethod]
    public void AddCreatedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
    }
    [TestMethod]
    public void AddConfirmedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
    }
    [TestMethod]
    public void AddStartedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
    }
    [TestMethod]
    public void RemoveStartedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation, 100065);
        RemoveSomeOperationWorkers_Ok(operation, 100065);
    }
    [TestMethod]
    public void AddStartedOperationSameWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        AddSomeOperationWorkers_Error(operation);
    }
    [TestMethod]
    public void RemoveStartedOperationSameWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation, 100065);
        RemoveSomeOperationWorkers_Ok(operation, 100065);
        RemoveSomeOperationWorkers_Error(operation, 100065);
    }
    [TestMethod]
    public void AddWorkStartedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        // Аллашукуров Уктам Мухаммедович id = 100066.
    }
    [TestMethod]
    public void RemoveWorkStartedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation, 100066);
        StartSomeOperationWork_Ok(operation);
        // Аллашукуров Уктам Мухаммедович id = 100066.
        RemoveSomeOperationWorkers_Ok(operation, 100066);
    }
    [TestMethod]
    public void AddWorkFinishedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        FinishSomeOperationWork_Ok(operation);
        // Аллашукуров Уктам Мухаммедович id = 100066.
        AddSomeOperationWorkers_Error(operation, 100066);
    }
    [TestMethod]
    public void RemoveWorkFinishedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation, 100066);
        StartSomeOperationWork_Ok(operation);
        FinishSomeOperationWork_Ok(operation);
        // Аллашукуров Уктам Мухаммедович id = 100066.
        RemoveSomeOperationWorkers_Error(operation, 100066);
    }
    [TestMethod]
    public void AddFinishedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation, 100065);
        StartSomeOperationWork_Ok(operation);
        FinishSomeOperationWork_Ok(operation);
        FinishSomeOperation_Ok(operation);
        // Аллашукуров Уктам Мухаммедович id = 100066.
        AddSomeOperationWorkers_Error(operation, 100066);
    }
    [TestMethod]
    public void RemoveFinishedOperationWorker()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation, 100066);
        StartSomeOperationWork_Ok(operation);
        FinishSomeOperationWork_Ok(operation);
        FinishSomeOperation_Ok(operation);
        // Аллашукуров Уктам Мухаммедович id = 100066.
        RemoveSomeOperationWorkers_Error(operation, 100066);
    }
    [TestMethod]
    public void AllocateOperationWorkers()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        _storage.AllocateOperationWorkers(operation);
        operation = _storage.GetOperation(operation.Id);
        var workers = _storage.GetOperationWorkers(operation);
        Assert.IsTrue(workers.Length > 0);
    }
    [TestMethod]
    public void StartOperationWorkWithoutWarehouseActions()
    {
        var operation = CreateSomeOperation();
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);

        operation = _storage.GetOperation(operation.Id);
        Assert.ThrowsExactly<PostgresException>(() => _storage.StartOperationWork(operation, DefaultUser));
    }
    [TestMethod]
    public void StartOperationWorkWithoutWorkers()
    {
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        SetSomeOperationGate_Ok(operation);
        StartSomeOperation_Ok(operation);
        Assert.ThrowsExactly<PostgresException>(() => _storage.StartOperationWork(operation, DefaultUser));
    }
    [TestMethod]
    public void ChangeOperationsStates()
    {
        // Сборка тестовой операции и проход по состояниям.
        var operation = CreateSomeOperation();
        StartSomeOperation_Error(operation);
        StartSomeOperationWork_Error(operation);
        FinishSomeOperationWork_Error(operation);
        FinishSomeOperation_Error(operation);
        AddSomeOperationWarehouseActions_Ok(operation);

        SetSomeOperationGate_Ok(operation);
        StartSomeOperationWork_Error(operation);
        FinishSomeOperationWork_Error(operation);
        FinishSomeOperation_Error(operation);

        StartSomeOperation_Ok(operation);
        StartSomeOperation_Error(operation);
        FinishSomeOperationWork_Error(operation);
        FinishSomeOperation_Error(operation);
        AddSomeOperationWorkers_Ok(operation);

        StartSomeOperationWork_Ok(operation);
        SetSomeOperationGate_Error(operation);
        StartSomeOperation_Error(operation);
        StartSomeOperationWork_Error(operation);
        FinishSomeOperation_Error(operation);

        FinishSomeOperationWork_Ok(operation);
        SetSomeOperationGate_Error(operation);
        StartSomeOperation_Error(operation);
        StartSomeOperationWork_Error(operation);
        FinishSomeOperationWork_Error(operation);

        FinishSomeOperation_Ok(operation);
        SetSomeOperationGate_Error(operation);
        StartSomeOperation_Error(operation);
        StartSomeOperationWork_Error(operation);
        FinishSomeOperationWork_Error(operation);
        FinishSomeOperation_Error(operation);
    }
    [TestMethod]
    public void CancelAndDeleteOperation()
    {
        // Сборка тестовой операции и её отмена и удаление.
        var operation = CreateSomeOperation();
        AddSomeOperationWarehouseActions_Ok(operation);
        CancelSomeOperation_Ok(operation);

        operation = _storage.GetOperation(operation.Id);
        DeleteSomeOperation_Ok(operation);
    }

    [TestMethod]
    public void GetGuardOperations()
    {
        var operation = CreateSomeOperation();
        var operations = _storage.GetGuardOperations();
        Assert.AreEqual(0, operations.Length);

        SetSomeOperationGate_Ok(operation);
        operation = _storage.GetOperation(operation.Id);
        operations = _storage.GetGuardOperations();
        Assert.AreEqual(1, operations.Length);
        Assert.AreEqual(operation.Id, operations[0].Id);
        Assert.AreEqual("CONFIRMED", operations[0].State.Code);

        StartSomeOperation_Ok(operation);
        operations = _storage.GetGuardOperations();
        Assert.AreEqual(0, operations.Length);

        AddSomeOperationWarehouseActions_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        operations = _storage.GetGuardOperations();
        Assert.AreEqual(0, operations.Length);

        FinishSomeOperationWork_Ok(operation);
        operation = _storage.GetOperation(operation.Id);
        operations = _storage.GetGuardOperations();
        Assert.AreEqual(1, operations.Length);
        Assert.AreEqual(operation.Id, operations[0].Id);
        Assert.AreEqual("WORK_FINISHED", operations[0].State.Code);

        FinishSomeOperation_Ok(operation);
        operations = _storage.GetGuardOperations();
        Assert.AreEqual(0, operations.Length);
    }
    [TestMethod]
    public void GetActiveOperations()
    {
        var operation = CreateSomeOperation();
        var operations = _storage.GetActiveOperations();
        Assert.AreEqual(0, operations.Length);

        SetSomeOperationGate_Ok(operation);
        operation = _storage.GetOperation(operation.Id);
        operations = _storage.GetActiveOperations();
        Assert.IsTrue(operations.Length == 0);

        StartSomeOperation_Ok(operation);
        operation = _storage.GetOperation(operation.Id);
        operations = _storage.GetActiveOperations();
        Assert.AreEqual(1, operations.Length);
        Assert.AreEqual(operation.Id, operations[0].Id);
        Assert.AreEqual("STARTED", operations[0].State.Code);

        AddSomeOperationWarehouseActions_Ok(operation);
        AddSomeOperationWorkers_Ok(operation);
        StartSomeOperationWork_Ok(operation);
        operation = _storage.GetOperation(operation.Id);
        operations = _storage.GetActiveOperations();
        Assert.AreEqual(1, operations.Length);
        Assert.AreEqual(operation.Id, operations[0].Id);
        Assert.AreEqual("WORK_STARTED", operations[0].State.Code);

        FinishSomeOperationWork_Ok(operation);
        operation = _storage.GetOperation(operation.Id);
        operations = _storage.GetActiveOperations();
        Assert.AreEqual(1, operations.Length);
        Assert.AreEqual(operation.Id, operations[0].Id);
        Assert.AreEqual("WORK_FINISHED", operations[0].State.Code);

        FinishSomeOperation_Ok(operation);
        operations = _storage.GetActiveOperations();
        Assert.AreEqual(0, operations.Length);
    }

    public void Dispose() => _storage.Dispose();
}
