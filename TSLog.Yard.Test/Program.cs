using TSLog.WarehouseActions.BO;
using TSLog.Yard;
using TSLog.Yard.BO;
using Microsoft.Extensions.Configuration;

// See https://aka.ms/new-console-template for more information

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
    var filteredOperations = storage.GetOperations(filterFromDate, filterToDate, null, null, null, null, null, string.Empty, string.Empty, string.Empty);
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
    // test full cycle
    Console.WriteLine("готово");

    Console.WriteLine("сборка тестовой операции...");
    var type = storage.GetOperationType("O_BOX_MIX");
    var testVehicleKind = storage.GetVehicleKind("20_TONS");
    var startDate = DateTime.Now.AddDays(3);
    var finishDate = startDate;
    finishDate = finishDate.Add(new TimeSpan(0, storage.GetTime(type, vehicleKind), 0));
    var defState = storage.GetOperationState("CREATED");
    var defManager = storage.GetEmployee(10405);

    var newOperation = new Operation(-1, null, type, startDate, finishDate, 
        vehicleKind, "some brand", "some model", "50me-nu|\\/|beR", "trailer number", 
        "Ivan Ivanov", "123-456-789", 
        defState, 1, 
        "Ivanov", "Ivanov", "Ivanov",
        defManager);
    newOperation.AddAction(new WarehouseAction(ActionKind.Income, "not-set", storage.GetPartner("0130"), "69420-80085"));
    storage.AddOperation(newOperation);
    Console.WriteLine("готово");

    Console.WriteLine("проверка выгрузки справочников...");
    var actions = newOperation.GetActions();
    var partners = newOperation.GetPartners();
    Console.WriteLine("готово");

    Console.WriteLine("проведение операции по состояниям...");
    storage.SetOperationGate(newOperation);
    storage.StartOperation(newOperation);
    storage.StartOperationWork(newOperation);
    storage.FinishOperation(newOperation);
    storage.FinishOperationWork(newOperation);
    Console.WriteLine("готово");

    Console.WriteLine("отмена и удаление операции...");
    storage.CancelOperation(newOperation);
    storage.DeleteOperation(newOperation);
    Console.WriteLine("готово");

    // TODO: test filter
    
    Console.WriteLine("закрытие соединения с БД...");
    storage.DisconnectOracle();
    storage.DisconnectPgsql();
    Console.WriteLine("готово");

    Console.WriteLine("[тест был пройден успешно]");
}
Console.ReadKey();


