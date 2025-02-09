using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace TSLog.Links.Connector;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        IStorage storage;
        IWmsStorage wmsStorage;
        IDiadocWorker diadocWorker;
        var isDebug = Settings.Default.IsDebug;
        if (isDebug)
        {
            storage = new FakeStorage();
            wmsStorage = new FakeWmsStorage();
            diadocWorker = new FakeDiadocWorker();
        }
        else
        {
            //var dbDataSource = Settings.Default.DbDataSource;
            //var dbDataSource = "tsklpdb";

            var dbDataSource = 
                "(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.253.43)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = tsklpdb)))";

            var dbLogin = Settings.Default.DbLogin;
            var dbPassword = Settings.Default.DbPassword;
            storage = new Storage(dbDataSource);
            var securityDbPassword = TSLog.Common.Utils.StringToSecureString(dbPassword);
            storage.Connect(dbLogin, securityDbPassword);

            wmsStorage = new WmsStorage();

            var clientId = Settings.Default.DiadocClientId;
            var apiUrl = Settings.Default.DiadocApiUrl;
            var login = Settings.Default.DiadocLogin;
            var password = Settings.Default.DiadocPassword;
            var boxId = Settings.Default.DiadocBoxId;
            var certificatePath = Settings.Default.DiadocCertificatePath;
            diadocWorker = new DiadocWorker(clientId, apiUrl, login, password, boxId, certificatePath);

            // --force-start
            //diadocWorker.Test();
        }
        var worker = new Worker(storage, wmsStorage, diadocWorker);

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(worker));

        storage.Dispose();
    }
}