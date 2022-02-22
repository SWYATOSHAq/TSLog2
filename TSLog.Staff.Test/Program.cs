// See https://aka.ms/new-console-template for more information
using TSLog.Common;
using TSLog.LegalEntities.BO;
using TSLog.Staff.BO;
using TSLog.Staff.Data;

Console.WriteLine("Staff test!");

var company = new LegalEntity("10004");
var employee = new Employee(1, "E01", "Surname", "FirstName", DateTime.Today, company, true);

var department = new Department(1, "AAA", "BBBB", "BBBB", "BBBB", "BBBB", true, -1, company, employee);


var dataSource = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = eccobdb)))";
StaffOracleStorage storage = new StaffOracleStorage(dataSource);
storage.Connect("ecco", Utils.StringToSecureString("e"));
storage.StaffSchema = "ecco";

var shifts = storage.GetShifts(DateTime.Today.AddDays(-300), DateTime.Today.AddDays(300));

storage.Disconnect();

Console.WriteLine("Test complete!");

Console.ReadKey();