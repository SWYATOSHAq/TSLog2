using System;
using System.Collections.Generic;
using System.Xml;
using TSLog.BO;

namespace TSLog.Links.Gs1EdiXml
{
    /// <summary>
    /// Класс для работы с GS1 EDI XML форматом
    /// </summary>
    public class Gs1EdiXmlConverter
    {
        private ILinksStorage _storage = null;
        public ILinksStorage LinksStorage
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _storage = value;
            }
        }

        private LegalEntity SideFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var id = string.Empty;
            var closeTag = reader.Name;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "sh:Identifier")
                        id = reader.ReadElementContentAsString();
                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var side = _storage.GetPartner(id);
                        return side;
                    }
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            return LegalEntity.Null;
        }
        private LegalEntity LegalEntityFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var code = string.Empty;
            var closeTag = reader.Name;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "gln")
                        reader.Read();
                    else if (reader.Name == "additionalPartyIdentification")
                        code = reader.ReadElementContentAsString();
                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var legalEntity = _storage.GetPartner(code);
                        if (legalEntity == null)
                            throw new Exception("");
                        return legalEntity;
                    }
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            return null;
        }
        private LogisticUnit LogisticUnitFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var closeTag = reader.Name;
            var code = string.Empty;
            var typeCode = string.Empty;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "sscc")
                        code = reader.ReadElementContentAsString();
                    else if (reader.Name == "packageTypeCode")
                        typeCode = reader.ReadElementContentAsString();
                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var unit = new LogisticUnit(typeCode, code);
                        return unit;
                    }
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            return null;
        }

        #region Клиенты (контрагенты)
        public LegalEntity[] ClientsFromXml(XmlReader reader)
        {
            var clients = new List<LegalEntity>();
            return clients.ToArray();
        }
        #endregion

        #region Товары
        private Product ProductFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var closeTag = reader.Name;
            var code = string.Empty;
            var name = string.Empty;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "tradeItemDescription")
                        name = reader.ReadElementContentAsString();
                    else if (reader.Name == "tradeItemIdentification")
                        reader.Read();
                    else if (reader.Name == "gtin")
                        code = reader.ReadElementContentAsString();
                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var product = new Product(-1, code, name, name);
                        return product;
                    }
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            return Product.Null;
        }
        public ProductsList ProductsListFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var productsList = new ProductsList(-1, 1);
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "sh:Sender")
                        productsList.Sender = SideFromXml(reader);
                    else if (reader.Name == "sh:Sender")
                        productsList.Recipient = SideFromXml(reader);

                    else if (reader.Name == "dataSource")
                        productsList.DataSource = LegalEntityFromXml(reader);
                    else if (reader.Name == "dataRecipient")
                        productsList.DataRecipient = LegalEntityFromXml(reader);

                    else if (reader.Name == "itemDataNotificationLineItem")
                        productsList.AddProduct(ProductFromXml(reader));
                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                    reader.Read();
                else
                    reader.Read();
            }
            return productsList;
        }
        #endregion

        #region Приход товара на склад

        private InboundInstruction.Line InboundInstructionLineFromXml(XmlReader reader)
        {
            //< lineItemNumber > 1 </ lineItemNumber >
            //< transactionalTradeItem >
            //< gtin > 02222222003333 </ gtin >
            if (reader == null)
                throw new ArgumentNullException("reader");

            var closeTag = reader.Name;

            var gtin = string.Empty;
            var quantity = 0.0;
            var logisticUnits = new List<LogisticUnit>();
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "transactionalTradeItem")
                        reader.Read();
                    else if (reader.Name == "gtin")
                        gtin = reader.ReadElementContentAsString();

                    else if (reader.Name == "plannedReceiptQuantity")
                        quantity = reader.ReadElementContentAsDouble();

                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var product = _storage.GetProduct(gtin);
                        var line = new InboundInstruction.Line(-1, product, quantity, logisticUnits.ToArray());
                        return line;
                    }
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            return null;
        }
        public InboundInstruction InboundInstructionFromXml(XmlReader reader)
        {
            var message = new InboundInstruction(1);

            LegalEntity sender = null;
            LegalEntity recipient = null;

            LegalEntity serviceBuyer = null;
            LegalEntity serviceSeller = null;
            LegalEntity shipper = null;
            LegalEntity receiver = null;

            var beginDate = string.Empty;
            var beginTime = string.Empty;
            var endDate = string.Empty;
            var endTime = string.Empty;

            var logisticUnits = new List<LogisticUnit>();
            var lines = new List<InboundInstruction.Line>();

            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "sh:Sender")
                        sender = SideFromXml(reader);
                    else if (reader.Name == "sh:Receiver")
                        recipient = SideFromXml(reader);

                    else if (reader.Name == "creationDateTime")
                        message.InboundDate = Convert.ToDateTime(reader.ReadElementContentAsString());
                    else if (reader.Name == "entityIdentification")
                        message.Number = reader.ReadElementContentAsString();

                    else if (reader.Name == "logisticServicesBuyer")
                        serviceBuyer = LegalEntityFromXml(reader);
                    else if (reader.Name == "logisticServicesSeller")
                        serviceSeller = LegalEntityFromXml(reader);

                    // Global Shipment Identification Number,
                    // Глобальный номер для идентификации отгрузки
                    else if (reader.Name == "gsin")
                        message.PartnerInboundNumber = reader.ReadElementContentAsString();

                    else if (reader.Name == "warehousingInboundInstructionShipment")
                        reader.Read();
                    else if (reader.Name == "shipmentIdentification")
                        reader.Read();

                    else if (reader.Name == "shipper")
                        shipper = LegalEntityFromXml(reader);
                    else if (reader.Name == "receiver")
                        receiver = LegalEntityFromXml(reader);

                    else if (reader.Name == "logisticUnit")
                        logisticUnits.Add(LogisticUnitFromXml(reader));

                    /* TODO: parse this block
                    <packageTotal>
                        <packageTypeCode>AF</packageTypeCode>
                        <totalPackageQuantity>2</totalPackageQuantity>
                    </packageTotal>
                    <warehousingReceiptTypeCode>PRIORITY_RECEIPT</warehousingReceiptTypeCode>
                    */
                    else if (reader.Name == "plannedReceipt")
                        reader.Read();
                    else if (reader.Name == "logisticEventPeriod")
                        reader.Read();

                    else if (reader.Name == "beginDate")
                        beginDate = reader.ReadElementContentAsString();
                    else if (reader.Name == "beginTime")
                        beginTime = reader.ReadElementContentAsString();
                    else if (reader.Name == "endDate")
                        endDate = reader.ReadElementContentAsString();
                    else if (reader.Name == "endTime")
                        endTime = reader.ReadElementContentAsString();

                    else if (reader.Name == "warehousingInboundInstructionShipmentItem")
                        lines.Add(InboundInstructionLineFromXml(reader));

                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                    reader.Read();
                else
                    reader.Read();
            }
            message.Sender = sender;
            message.Recipient = recipient;
            message.ServiceBuyer = serviceBuyer;
            message.ServiceSeller = serviceSeller;
            message.Shipper = shipper;
            message.Receiver = receiver;
                        
            message.PlanDeliveryBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            message.PlanDeliveryEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));

            message.AddLogisticUnits(logisticUnits.ToArray());
            message.AddLines(lines.ToArray());
                
            return message;
        }
        public string ToGs1ExiXml(InboundNotification inboundNotification)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement("warehousingInboundNotificationMessage"));

            XmlNode docHeader = root.AppendChild(doc.CreateElement("sh:StandardBusinessDocumentHeader"));

            XmlNode body = root.AppendChild(doc.CreateElement("warehousingInboundNotification"));

            body.AppendChild(doc.CreateElement("creationDateTime")).InnerText = "";
            body.AppendChild(doc.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            body.AppendChild(doc.CreateElement("documentActionCode")).InnerText = "ADD";

            XmlNode ident = body.AppendChild(doc.CreateElement("warehousingInboundNotificationIdentification"));
            ident.AppendChild(doc.CreateElement("entityIdentification")).InnerText = inboundNotification.Id.ToString();

            XmlNode lsb = body.AppendChild(doc.CreateElement("logisticServicesBuyer"));
            lsb.AppendChild(doc.CreateElement("gln")).InnerText = "";

            XmlNode lss = body.AppendChild(doc.CreateElement("logisticServicesSeller"));
            lss.AppendChild(doc.CreateElement("gln")).InnerText = "";

            /*
            -<shipmentIdentification>
                <gsin>11111110000834053</gsin>
            </shipmentIdentification>
            -<shipper>
                <gln>1111111000007</gln>
            </shipper>
            -<receiver>
                <gln>3333333000018</gln>
            </receiver>
             */

            /*
            <logisticUnit>
                <sscc>311111110000123455</sscc>
                <packageTypeCode>AF</packageTypeCode>
            </logisticUnit>
            <logisticUnit>
                <sscc>311111110000123554</sscc>
                <packageTypeCode>AF</packageTypeCode>
            </logisticUnit>
            <packageTotal>
                <packageTypeCode>AF</packageTypeCode>
                <totalPackageQuantity>2</totalPackageQuantity>
            </packageTotal>
             */

            body.AppendChild(doc.CreateElement("warehousingInboundStatusCode")).InnerText = "UNLOADING_FINISHED";

            /*
            < actualReceipt >
                < logisticEventDateTime >
                    < date > 2012 - 07 - 12 </ date >
                    < time > 11:30:00 </ time >
                </ logisticEventDateTime >
            </ actualReceipt >
            */

            /*
            < warehousingInboundInstruction >
                < entityIdentification > INSREC00123 </ entityIdentification >
            </ warehousingInboundInstruction >
            */

            // Items
            //foreach (var item in inboundResponse.Lines)
            //{
            // <warehousingInboundNotificationShipmentItem></warehousingInboundNotificationShipmentItem>
            //}

            return doc.OuterXml;
        }
        #endregion

        #region Убытие товара со склада
        private OutboundInstruction.Line OutboundInstructionLineFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            var closeTag = reader.Name;
            var gtin = string.Empty;
            var quantity = 0.0;

            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "transactionalTradeItem")
                        reader.Read();
                    else if (reader.Name == "gtin")
                        gtin = reader.ReadElementContentAsString();

                    else if (reader.Name == "plannedDespatchQuantity")
                        quantity = reader.ReadElementContentAsDouble();
                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var product = _storage.GetProduct(gtin);
                        var line = new OutboundInstruction.Line(-1, product, quantity);
                        return line;
                    }
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            return null;
        }
        public OutboundInstruction OutboundInstructionFromXml(XmlReader reader)
        {
            var outboundInstruction = new OutboundInstruction(-1, 1);

            LegalEntity sender = null;
            LegalEntity recipient = null;

            LegalEntity serviceBuyer = null;
            LegalEntity serviceSeller = null;
            LegalEntity shipper = null;
            LegalEntity receiver = null;

            var beginDate = string.Empty;
            var beginTime = string.Empty;
            var endDate = string.Empty;
            var endTime = string.Empty;

            var lines = new List<OutboundInstruction.Line>();

            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "sh:Sender")
                        sender = SideFromXml(reader);
                    else if (reader.Name == "sh:Receiver")
                        recipient = SideFromXml(reader);

                    else if (reader.Name == "creationDateTime")
                        outboundInstruction.Date = Convert.ToDateTime(reader.ReadElementContentAsString());
                    else if (reader.Name == "entityIdentification")
                        outboundInstruction.Number = reader.ReadElementContentAsString();

                    else if (reader.Name == "logisticServicesBuyer")
                        serviceBuyer = LegalEntityFromXml(reader);
                    else if (reader.Name == "logisticServicesSeller")
                        serviceSeller = LegalEntityFromXml(reader);

                    else if (reader.Name == "shipper")
                        shipper = LegalEntityFromXml(reader);
                    else if (reader.Name == "receiver")
                        receiver = LegalEntityFromXml(reader);

                    else if (reader.Name == "plannedDelivery")
                        reader.Read();
                    else if (reader.Name == "logisticEventPeriod")
                        reader.Read();
                    else if (reader.Name == "beginDate")
                        beginDate = reader.ReadElementContentAsString();
                    else if (reader.Name == "beginTime")
                        beginTime = reader.ReadElementContentAsString();
                    else if (reader.Name == "endDate")
                        endDate = reader.ReadElementContentAsString();
                    else if (reader.Name == "endTime")
                        endTime = reader.ReadElementContentAsString();

                    else if (reader.Name == "warehousingOutboundInstructionShipmentItem")
                        lines.Add(OutboundInstructionLineFromXml(reader));

                    else
                        reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                    reader.Read();
                else
                    reader.Read();
            }

            outboundInstruction.Sender = sender;
            outboundInstruction.Recipient = recipient;
            outboundInstruction.ServiceBuyer = serviceBuyer;
            outboundInstruction.ServiceSeller = serviceSeller;
            outboundInstruction.Shipper = shipper;
            outboundInstruction.Receiver = receiver;

            outboundInstruction.PlanDeliveryBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            outboundInstruction.PlanDeliveryEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));

            outboundInstruction.AddLines(lines.ToArray());

            return outboundInstruction;
        }
        public string OutboundNotificationToXml(OutboundNotification outboundNotification)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement("warehousingOutboundNotificationMessage"));

            XmlNode docHeader = root.AppendChild(doc.CreateElement("sh:StandardBusinessDocumentHeader"));
            docHeader.AppendChild(doc.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            XmlNode xmlSender = docHeader.AppendChild(doc.CreateElement("sh:Sender"));
            xmlSender.AppendChild(doc.CreateElement("sh:Identifier")).InnerText = outboundNotification.Sender.Code;

            XmlNode xmlReceiver = docHeader.AppendChild(doc.CreateElement("sh:Receiver"));
            xmlSender.AppendChild(doc.CreateElement("sh:Identifier")).InnerText = outboundNotification.Recipient.Code;

            XmlNode body = root.AppendChild(doc.CreateElement("warehousingOutboundNotification"));

            body.AppendChild(doc.CreateElement("creationDateTime")).InnerText = "";
            body.AppendChild(doc.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            body.AppendChild(doc.CreateElement("documentActionCode")).InnerText = "ADD";

            XmlNode ident = body.AppendChild(doc.CreateElement("warehousingOutboundNotificationIdentification"));
            ident.AppendChild(doc.CreateElement("entityIdentification")).InnerText = outboundNotification.Id.ToString();

            XmlNode lsb = body.AppendChild(doc.CreateElement("logisticServicesBuyer"));
            lsb.AppendChild(doc.CreateElement("gln")).InnerText = "";

            XmlNode lss = body.AppendChild(doc.CreateElement("logisticServicesSeller"));
            lss.AppendChild(doc.CreateElement("gln")).InnerText = "";

            var lineNumber = 1;
            foreach (var line in outboundNotification.Lines)
            {
                var xmlLine = body.AppendChild(doc.CreateElement("warehousingOutboundInstructionShipmentItem"));
                xmlLine.AppendChild(doc.CreateElement("lineItemNumber")).InnerText = lineNumber.ToString();
                xmlLine.AppendChild(doc.CreateElement("transactionalTradeItem")).AppendChild(doc.CreateElement("gtin")).InnerText = 
                    line.Product.Gtin;
                
                
                lineNumber++;
            }

            return doc.OuterXml;
        }

        #endregion
    }
}
