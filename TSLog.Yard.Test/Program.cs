using System.Security;
using TSLog.Yard.BO;
using TSLog.Yard.Data;

// See https://aka.ms/new-console-template for more information

string host = "10.50.254.14";
string database = "tslog";
using (var pgSql = new PgSqlYardStorage(host, database))
{
    string user = "yardadmin";
    string temp = "12";
    var password = new SecureString();
    foreach (var symbol in temp)
        password.AppendChar(symbol);
    pgSql.Connect(user, password);
    pgSql.YardSchema = "yard";

    var operationState = pgSql.GetOperationState("CREATED");
    var operationType = pgSql.GetOperationType("BOX_OUTGO");
    var vehicleKind = pgSql.GetVehicleKind("20_TONS");

    var operation = new Operation(-1, null);
    operation.Type = operationType;
    operation.PlanStartDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
    operation.PlanFinshDate = DateTime.SpecifyKind(DateTime.Now.AddHours(3), DateTimeKind.Local);
    operation.VehicleKind = vehicleKind;
    operation.VehicleBrand = "Форд";
    operation.VehicleModel = "XXXX";
    operation.VehicleNumber = "12356789";
    operation.DriverName = "Петров Пётр Петрович";
    operation.Logist  = null;
    operation.Manager = new TSLog.Staff.BO.Employee(1, "AAAA");
    operation.Note = "TEST";

    pgSql.AddOperation(operation);

    pgSql.Disconnect();
}

