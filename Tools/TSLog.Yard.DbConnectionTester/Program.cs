using Microsoft.Extensions.Configuration;

namespace TSLog.Yard.DbConnectionTester;

internal class Program
{
    private static void Test(Worker worker, bool callClose, Worker.DisposeMethod disposeMethod, bool throwException)
    {
        try
        {
            Console.WriteLine($"callClose: {callClose}, disposeMethod: {disposeMethod}, throwException: {throwException})");
            worker.TestConnections(callClose, disposeMethod);
            Console.WriteLine("Ok.");
        }
        catch (CustomException)
        { }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
        Console.WriteLine();
    }
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(("appsettings.json"))
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        Console.WriteLine($"Start.");
        var worker = new Worker(configuration);
        //Test(worker, callClose: true, disposeMethod: Worker.DisposeMethod.Using, throwException: true);
        Test(worker, callClose: true, disposeMethod: Worker.DisposeMethod.Using, throwException: false);
        //Test(worker, callClose: true, disposeMethod: Worker.DisposeMethod.Direct, throwException: true);
        //Test(worker, callClose: true, disposeMethod: Worker.DisposeMethod.Direct, throwException: false);
        //Test(worker, callClose: true, disposeMethod: Worker.DisposeMethod.NotDispose, throwException: true);
        //Test(worker, callClose: true, disposeMethod: Worker.DisposeMethod.NotDispose, throwException: false);
        //Test(worker, callClose: false, disposeMethod: Worker.DisposeMethod.Using, throwException: true);
        //Test(worker, callClose: false, disposeMethod: Worker.DisposeMethod.Using, throwException: false);
        //Test(worker, callClose: false, disposeMethod: Worker.DisposeMethod.Direct, throwException: true);
        //Test(worker, callClose: false, disposeMethod: Worker.DisposeMethod.Direct, throwException: false);
        //Test(worker, callClose: false, disposeMethod: Worker.DisposeMethod.NotDispose, throwException: true);
        //Test(worker, callClose: false, disposeMethod: Worker.DisposeMethod.NotDispose, throwException: false);

        Console.WriteLine($"Finish.");
        Console.ReadKey();
    }
}
