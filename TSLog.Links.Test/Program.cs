using System;
using System.Security;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.BO;
using TSLog.Links.Db;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Test
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var dataSource =
                "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = eccobdb)))";

            var storage = new DbLinksStorage(dataSource);
            storage.CommonSchema = "ecco";
            storage.LinksSchema = "links";
            storage.WmsSchema = "ecco2_s1_pdba";

            storage.InboundsStates = new Catalog<Entity>("INBOUND_STATES", "Состояия приходов");
            storage.OutboundsStates = new Catalog<Entity>("OUTBOUND_STATES", "Состояия расходов");
            storage.MeasureUnits = new Catalog<MeasureUnit>("MEASURE_UNITS", "Единицы измерения");
            storage.Partners = new Catalog<LegalEntity>("PARTNERS", "Партнёры");
            storage.Products = new Catalog<Product>("PRODUCTS", "Товары");
            storage.Currencies = new Catalog<Entity>("CURRENCIES", "валюты");

            var password = new SecureString();
            foreach (var symbol in "e")
                password.AppendChar(symbol);
            storage.Connect("ecco", password);

            // preload units and states
            storage.InboundsStates.Add(storage.GetInboundsStates());
            storage.OutboundsStates.Add(storage.GetOutboundsStates());
            storage.MeasureUnits.Add(storage.GetMeasureUnits());
            storage.Currencies.Add(storage.GetCurrncies());
            

            var xmlConverter = new Gs1EdiXmlConverter();
            xmlConverter.LinksStorage = storage;

            // ===========================================
            // reader test
            var fileName = "";
            XmlReader reader;

            // inboud test
            Console.WriteLine(">inboud<");
            fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\InboundInstruction0001.xml";
            reader = new XmlTextReader(fileName);
            var inboundInstruction = xmlConverter.InboundInstructionFromXml(reader);
            Console.WriteLine(inboundInstruction.Number);
            Console.WriteLine("|serialization|");
            var converterInbounds = new EINWMSInboundsConverter();
            Console.WriteLine(converterInbounds.Convert("test69420TODO", inboundInstruction));

            //// outbound test
            //Console.WriteLine(">outbound<");
            //fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\OutboundInstruction0001.xml";
            //reader = new XmlTextReader(fileName);
            //var outboundInstruction = xmlConverter.OutboundInstructionFromXml(reader);
            //Console.WriteLine(outboundInstruction.Number);
            //Console.WriteLine("|serialization|");
            //var converterOutbounds = new EINWMSOutboundsConverter();
            //Console.WriteLine(converterOutbounds.Convert("test69420TODO", outboundInstruction));

            //// item test
            //Console.WriteLine(">item<");
            //fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\ItemDataNotification0001.xml";
            //reader = new XmlTextReader(fileName);
            //var itemDataNotification = xmlConverter.ProductsListFromXml(reader);
            //Console.WriteLine(itemDataNotification.Number);
            //Console.WriteLine("|serialization|");
            //var converterProducts = new EINWMSProductsConverter();
            //Console.WriteLine(converterProducts.Convert("test69420TODO", itemDataNotification));


            Console.WriteLine(">finished<");
            reader.Close();
            Console.ReadLine();
        }
    }
}