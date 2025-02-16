using AngleSharp.Dom;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using TSLog.Outgoes.BO;
using TSLog.Partners.BO;

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
            storage.CommonSchema = "ecco";
            storage.LinksSchema = "ecco";
            storage.PartnersSchema = "ecco";
            storage.Connect(dbLogin, securityDbPassword);

            Environment.SetEnvironmentVariable("ITEXT_BOUNCY_CASTLE_FACTORY_NAME", "bouncy-castle");
            wmsStorage = new WmsStorage(storage);

            var document = new Document() { Id = "00070000W0007N0005694", Kind = "O" };

            /*
            string id = "250000-000090";
            string number = "250000-000090";
            DateTime date = DateTime.Now;
            string partnerId = "уп2_01/04-2016";
            Partner partner = new(partnerId);
            string partnerNumber = "1234";
            TSLog.Common.BO.Entity state = new(-1, "");
            Outgo outgo = new Outgo(id, number, date, partner, partnerNumber, state);
            */

            wmsStorage.GetFile(document);
            return;

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

        var outgo = storage.GetOutgo("00070000W0007N0005425");

        Application.Run(new MainForm(worker, outgo));

        storage.Dispose();
    }
}