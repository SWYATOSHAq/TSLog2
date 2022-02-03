// See https://aka.ms/new-console-template for more information
using TSLog.LegalEntities.BO;
using TSLog.Staff.BO;

Console.WriteLine("Staff test!");

var company = new LegalEntity("10004");
var employee = new Employee(1, "E01", "Surname", "FirstName", DateTime.Today, company, true);

var department = new Department(1, "AAA", "BBBB", "BBBB", "BBBB", "BBBB", true, -1, company, employee);

Console.WriteLine("Test complete!");

Console.ReadKey();
