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

    var operationState = storage.GetOperationState("CREATED");
    var operationType = storage.GetOperationType("BOX_OUTGO");
    var vehicleKind = storage.GetVehicleKind("20_TONS");

    var operation = new YardOperation(-1, null);
    operation.Type = operationType;
    operation.PlanStartDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
    operation.PlanFinshDate = DateTime.SpecifyKind(DateTime.Now.AddHours(3), DateTimeKind.Local);
    operation.VehicleKind = vehicleKind;
    operation.VehicleBrand = "Форд";
    operation.VehicleModel = "XXXX";
    operation.VehicleNumber = "12356789";
    operation.DriverName = "Петров Пётр Петрович";
    operation.Logist = null;
    operation.Manager = new TSLog.Staff.BO.Employee(1, "AAAA");
    operation.Note = "TEST";
        
    storage.AddOperation(operation);

    storage.DisconnectOracle();
    storage.DisconnectPgsql();
}

