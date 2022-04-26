using TSLog.Yard;
using TSLog.Yard.BO;
using Microsoft.Extensions.Configuration;
using TSLog.WarehouseActions.BO;

var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

var configuration = builder.Build();

var isDebug = configuration.GetValue<bool>("IsDebug");
var oracleConnectionSettings = isDebug
    ? configuration.GetSection("OracleConnectionDebugSettings")
    : configuration.GetSection("OracleConnectionSettings");
string oracleTns = oracleConnectionSettings.GetValue<string>("DataSource");
string oracleUserName = oracleConnectionSettings.GetValue<string>("UserName");
string oraclePassword = oracleConnectionSettings.GetValue<string>("Password");
string oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema");
string oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema");
string oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema");
string oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema");

var pgsqlConnectionSettings = isDebug
    ? configuration.GetSection("PostgreeSqlConnectionDebugSettings")
    : configuration.GetSection("PostgreeSqlConnectionSettings");
string pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host");
string pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName");
string pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password");
string pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database");
string pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema");

Console.WriteLine("запуск теста...");

using (var storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase))
{
    Console.WriteLine("подключение к БД...");
    storage.ConnectOracle(oracleUserName, oraclePassword);
    storage.SetOracleCommonSchema(oracleCommonSchema);        
    storage.SetOracleStaffSchema(oracleStaffSchema);
    storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
    storage.SetOraclePartnersSchema(oraclePartnersSchema);

    storage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
    storage.SetPgsqlYardSchema(pgsqlYardSchema);
    Console.WriteLine("готово");

    Console.WriteLine("фильтрация операций двора (ПРР) в БД по различным параметрам...");
    var filterType = storage.GetOperationType(1006);
    var filterFromDate = DateTime.Parse("15.01.2022");
    var filterToDate = DateTime.Parse("16.01.2022");
    var filteredOperations = storage.GetOperations(filterFromDate, filterToDate, null, null, null, null, null, 
        string.Empty, string.Empty, string.Empty, string.Empty);
    Console.WriteLine("готово");

    Console.WriteLine("загрузка партнера по mndnr и информации по партнеру...");
    var partner = storage.GetPartner("0003");
    var managers = storage.GetPartnerManagers(partner);
    Console.WriteLine("готово");

    Console.WriteLine("загрузка типа операций, тип машины, ворота по id...");
    var ot = storage.GetOperationType(1001);
    var vk = storage.GetVehicleKind(1001);
    var g = storage.GetGate(1001);
    Console.WriteLine("готово");

    Console.WriteLine("проверка выгрузки свободных интервалов...");
    var intervals = storage.GetAvalibleIntervals(DateTime.Now.AddDays(-20), DateTime.Now.AddDays(+20), ot, vk);
    intervals = storage.GetGateIntervals(g, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
    intervals = storage.GetAllIntervals(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
    var union = Interval.Union(intervals);
    Console.WriteLine("готово");

    Console.WriteLine("проверка выгрузки операций...");
    var operations = storage.GetOperations(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5), null);
    Console.WriteLine("готово");

    Console.WriteLine("проверка выгрузки объектов по коду...");
    var operationState = storage.GetOperationState("CREATED");
    var operationType = storage.GetOperationType("O_BOX_MONO");
    var vehicleKind = storage.GetVehicleKind("20_TONS");    
    Console.WriteLine("готово");

    // Test full cycle
    Console.WriteLine("сборка тестовой операции...");
    var type = storage.GetOperationType("O_BOX_MIX");
    var testVehicleKind = storage.GetVehicleKind("20_TONS");
    var startDate = DateTime.Now.AddDays(3);
    var finishDate = startDate;
    finishDate = finishDate.Add(new TimeSpan(0, storage.GetDuration(type, vehicleKind), 0));
    var defState = storage.GetOperationState("CREATED");
    var defManager = storage.GetEmployee(100004);

    var user = "m.burov";
    var operation = new Operation(-1, null, type, startDate, finishDate, 
        vehicleKind, "some brand", "some model", "ABCDEF", "trailer number", 
        "Ivan Ivanov", "123-456-789", 
        defState, 1, 
        "Ivanov", "Ivan", "Ivanovich",
        defManager);

    var action = storage.GetAction(ActionKind.Outgo, partner, "ВБ00-008867");

    operation.AddAction(action);
    storage.AddOperation(operation, user);
    Console.WriteLine("готово");

    Console.WriteLine("проверка выгрузки справочников...");
    var actions = operation.GetActions();
    var partners = operation.GetPartners();
    Console.WriteLine("готово");

    Console.WriteLine("проведение операции по состояниям...");
    storage.SetOperationGate(operation, null, user);
    storage.StartOperation(operation, user);
    storage.StartOperationWork(operation, user);
    storage.FinishOperation(operation, user);
    storage.FinishOperationWork(operation, user);
    Console.WriteLine("готово");

    Console.WriteLine("отмена и удаление операции...");
    storage.CancelOperation(operation, user);
    storage.DeleteOperation(operation, user);
    Console.WriteLine("готово");

    // TODO: test filter
    
    Console.WriteLine("закрытие соединения с БД...");
    storage.DisconnectOracle();
    storage.DisconnectPgsql();
    Console.WriteLine("готово");

    Console.WriteLine("[тест был пройден успешно]");
}
Console.ReadKey();


