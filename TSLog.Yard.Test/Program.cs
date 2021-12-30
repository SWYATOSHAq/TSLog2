using TSLog.WarehouseActions.BO;
using TSLog.Yard;
using TSLog.Yard.BO;

// See https://aka.ms/new-console-template for more information

string oracleTns = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = eccobdb)))";
string oracleUserName = "ecco";
string oraclePassword = "e";
string oracleCommonSchema = "ecco";
string pgsqlHost = "10.50.254.14";
string pgsqlUserName = "yardadmin";
string pgsqlPassword = "12";
string pgsqlDatabase = "tslog";
string pgsqlYardSchema = "yard";

using (var storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase))
{
    storage.ConnectOracle(oracleUserName, oraclePassword);
    storage.SetOracleCommonSchema(oracleCommonSchema);
    storage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
    storage.SetPgsqlYardSchema(pgsqlYardSchema);

    var partner = storage.GetPartner("0003");
    var managers = storage.GetPartnerManagers(partner);

    var ot = storage.GetOperationType(1001);
    var vk = storage.GetVehicleKind(1001);
    var g = storage.GetGate(1001);

    var intervals = storage.GetAvalibleIntervals(DateTime.Now.AddDays(-20), DateTime.Now.AddDays(+20), ot, vk);
    intervals = storage.GetGateIntervals(g, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));
    intervals = storage.GetAllIntervals(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5));

    var union = Interval.Union(intervals);

    var operations = storage.GetOperations(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(+5), null);
    var operation = storage.GetOperation(10000002);
    var actions = operation.GetActions();
    var partners = operation.GetPartners();

    var operationState = storage.GetOperationState("CREATED");
    var operationType = storage.GetOperationType("BOX_OUTGO");
    var vehicleKind = storage.GetVehicleKind("20_TONS");

    // test full cycle
    var type = storage.GetOperationType("BOX_OUTGO");
    var testVehicleKind = storage.GetVehicleKind("20_TONS");
    var startDate = DateTime.Now.AddDays(3);
    var finishDate = startDate;
    finishDate = finishDate.Add(new TimeSpan(0, storage.GetTime(type, vehicleKind), 0));
    var defState = storage.GetOperationState("CREATED");
    var defManager = storage.GetEmployee(10405);

    var newOperation = new Operation(-1, null, type, startDate, finishDate, vehicleKind, "some brand", "some model", "50me-nu|\\/|beR", "Ivan Ivanov", defState, 1, defManager);
    newOperation.AddAction(new WarehouseAction(ActionKind.Income, "not-set",
                    storage.GetPartner("0130"), "69420-80085"));
    storage.AddOperation(newOperation);

    storage.SetOperationGate(newOperation);
    storage.StartOperation(newOperation);
    storage.SetOperationGate(newOperation);
    storage.StartOperationWork(newOperation);
    storage.FinishOperation(newOperation);
    storage.FinishOperationWork(newOperation);
    storage.CancelOperation(newOperation);
    storage.DeleteOperation(newOperation);

    //

    storage.DisconnectOracle();
    storage.DisconnectPgsql();
}

