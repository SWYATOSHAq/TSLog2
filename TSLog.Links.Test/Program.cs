using System;
using System.Security;
using System.Xml;
using TSlog.Links.PSI.DataConverter;
using TSLog.Common.BO;
using TSLog.Links.Data;
using TSLog.Links.Gs1EdiXml;
using TSLog.Partners.BO;
using TSLog.Products.BO;

namespace TSLog.Links.Test
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var dataSource =
                "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = eccobdb)))";

            var storage = new LinksOracleStorage(dataSource)
            {
                CommonSchema = "ecco",
                LinksSchema = "links",
                WmsSchema = "ecco2_s1_pdba",

                IncomeStates = new Catalog<Entity>("INBOUND_STATES", "Состояия приходов"),
                OutgoStates = new Catalog<Entity>("OUTBOUND_STATES", "Состояия расходов"),
                MeasureUnits = new Catalog<MeasureUnit>("MEASURE_UNITS", "Единицы измерения"),
                Partners = new Catalog<Partner>("PARTNERS", "Партнёры"),
                Currencies = new Catalog<Entity>("CURRENCIES", "валюты"),

                Products = new ProductCatalog<Product>("PRODUCTS", "Товары")
            };

            var password = new SecureString();
            foreach (var symbol in "e")
                password.AppendChar(symbol);
            storage.Connect("ecco", password);

            // preload units and states
            storage.IncomeStates.Add(storage.GetIncomeStates());
            storage.OutgoStates.Add(storage.GetOutgoStates());
            storage.MeasureUnits.Add(storage.GetMeasureUnits());
            storage.Currencies.Add(storage.GetCurrncies());

            var xmlConverter = new Gs1EdiXmlConverter
            {
                LinksStorage = storage
            };

            // ===========================================
            // reader test
            var fileName = "";
            XmlReader reader;

            // inboud test
            Console.WriteLine(">Income<");
            fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\InboundInstruction0001.xml";
            reader = new XmlTextReader(fileName);
            var inboundInstruction = xmlConverter.IncomeRequestFromXml(reader);
            Console.WriteLine("---serialization---");
            var converterInbounds = new EINWMSInboundsConverter();
            Console.WriteLine(converterInbounds.Convert(storage.GenerateTransactionId(), inboundInstruction));

            // outbound test
            Console.WriteLine(">Outgo<");
            fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\OutboundInstruction0001.xml";
            reader = new XmlTextReader(fileName);
            var outboundInstruction = xmlConverter.OutgoRequestFromXml(reader);
            Console.WriteLine(outboundInstruction.Number);
            Console.WriteLine("---serialization---");
            var converterOutbounds = new EINWMSOutboundsConverter();
            Console.WriteLine(converterOutbounds.Convert(storage.GenerateTransactionId(), outboundInstruction));

            // item test
            Console.WriteLine(">item<");
            fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\ItemDataNotification0001.xml";
            reader = new XmlTextReader(fileName);
            var itemDataNotification = xmlConverter.ProductsListFromXml(reader);
            Console.WriteLine(itemDataNotification.Number);
            Console.WriteLine("---serialization---");
            var converterProducts = new EINWMSProductsConverter();
            Console.WriteLine(converterProducts.Convert(storage.GenerateTransactionId(), itemDataNotification));

            // Clients test
            Console.WriteLine(">Clients<");
            fileName = @"..\..\..\..\TSLog.Links.Services\TestMessages\In\ClientsDataNotification0001.xml";
            reader = new XmlTextReader(fileName);
            var clientsList = xmlConverter.ClientsFromXml(reader);
            //Console.WriteLine(itemDataNotification.Number);
            Console.WriteLine("---serialization---");
            var converterClients = new EINWMSClientsConverter();
            Console.WriteLine(converterClients.Convert(storage.GenerateTransactionId(), clientsList));

            Console.WriteLine(">finished<");
            reader.Close();

            ////// test response
            Partner partner = storage.GetPartner("0014");
            string xml;

            // GetInboundNotification
            Console.WriteLine(">GetInboundNotification<");

            var notifocationInbound = storage.GetReceiptEndNotice(partner, "П-ВЖНФ-000400");
            xml = xmlConverter.ReceiptEndNoticeToXml(notifocationInbound);
            Console.WriteLine(xml);



            // GetOutboundNotification
            Console.WriteLine(">GetOutboundNotification<");
            var notifocationOutbound = storage.GetPickingBeginNotice(partner, "З-ВЖНФ-006613");
            xml = xmlConverter.PickingBeginNoticeToXml(notifocationOutbound);
            Console.WriteLine(xml);

            Console.WriteLine(">GetOutboundNotification<");
            var notifocationOutbound1 = storage.GetPickingEndNotice(partner, "З-ВЖНФ-008501");
            xml = xmlConverter.PickingEndNoticeToXml(notifocationOutbound1);
            Console.WriteLine(xml);

            Console.WriteLine(">GetOutboundNotification<");
            var notifocationOutbound2 = storage.GetShippingNotice(partner, "З-ВЖНФ-006624");
            xml = xmlConverter.ShippingNoticeToXml(notifocationOutbound2);
            Console.WriteLine(xml);


            Console.ReadLine();
        }
    }
}