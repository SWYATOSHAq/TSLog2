using System;
using System.Security;
using System.Xml;
using TSLog.BO;
using TSLog.Links.Db;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            var dataSource =
                "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = eccobdb)))";

            var inboundStates = new Catalog<Entity>("INBOUND_STATES", "Состояия приходов");
            var outboundStates = new Catalog<Entity>("OUTBOUND_STATES", "Состояия расходов");
            var measureUnits = new Catalog<MeasureUnit>("MEASURE_UNITS", "Единицы измерения");
            var partners = new Catalog<LegalEntity>("PARTNERS", "Партнёры");
            var products = new Catalog<Product>("PRODUCTS", "Товары");

            var storage = new DbLinksStorage(dataSource);
            storage.CommonSchema = "ecco";
            storage.LinksSchema = "links";
            storage.WmsSchema = "ecco2_s1_pdba";
            storage.InboundsStates = inboundStates;
            storage.OutboundsStates = outboundStates;
            storage.MeasureUnits = measureUnits;
            storage.Partners = partners;
            storage.Products = products;            
            var password = new SecureString();
            foreach (var symbol in "e")
                password.AppendChar(symbol);
            storage.Connect("ecco", password);

            inboundStates.Add(storage.GetInboundsStates());
            outboundStates.Add(storage.GetOutboundsStates());
            measureUnits.Add(storage.GetMeasureUnits());

            var xmlConverter = new Gs1EdiXmlConverter();
            xmlConverter.LinksStorage = storage;

            //var fileName = "C:\\m.burov\\Projects\\TSLog2\\TSLog.Links.Services\\TestMessages\\In\\InboundInstruction0001.xml";
            //XmlReader reader = new XmlTextReader(fileName);
            //var inboundInstruction = xmlConverter.InboundInstructionFromXml(reader);
            //Console.WriteLine(inboundInstruction.Number);

            //var fileName = "C:\\m.burov\\Projects\\TSLog2\\TSLog.Links.Services\\TestMessages\\In\\OutboundInstruction0001.xml";
            //XmlReader reader = new XmlTextReader(fileName);
            //var outboundInstruction = xmlConverter.OutboundInstructionFromXml(reader);
            //Console.WriteLine(outboundInstruction.Number);

            var fileName = "C:\\m.burov\\Projects\\TSLog2\\TSLog.Links.Services\\TestMessages\\In\\ItemDataNotification0001.xml";
            XmlReader reader = new XmlTextReader(fileName);
            var itemDataNotification = xmlConverter.ProductsListFromXml(reader);
            Console.WriteLine(itemDataNotification.Number);
        }
    }
}
