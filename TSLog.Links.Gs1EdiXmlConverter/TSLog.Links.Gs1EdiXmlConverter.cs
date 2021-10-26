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
        public string InboundNotificationToXml(InboundNotification inboundNotification)
        {
            var xmlDocument = new XmlDocument();
            var xmlRoot = xmlDocument.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationMessage"));

            var xmlHeader = xmlRoot.AppendChild(xmlDocument.CreateElement("sh:StandardBusinessDocumentHeader"));
            xmlHeader.AppendChild(xmlDocument.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            var xmlSender = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Sender"));
            xmlSender.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = inboundNotification.Sender.Code;
            var xmlRecipient = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Receiver"));
            xmlRecipient.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = inboundNotification.Recipient.Code;

            var xmlBody = xmlRoot.AppendChild(xmlDocument.CreateElement("warehousingInboundNotification"));

            xmlBody.AppendChild(xmlDocument.CreateElement("creationDateTime")).InnerText = inboundNotification.Date.ToString();
            xmlBody.AppendChild(xmlDocument.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            xmlBody.AppendChild(xmlDocument.CreateElement("documentActionCode")).InnerText = "ADD";

            var xmlIdentification = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationIdentification"));
            xmlIdentification.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = inboundNotification.Id.ToString();

            var xmlServiceBuyer = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesBuyer"));
            xmlServiceBuyer.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = inboundNotification.ServiceBuyer.Code;

            var xmlServiceSeller = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesSeller"));
            xmlServiceSeller.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = inboundNotification.ServiceSeller.Code;

            var xmlShipment = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationShipment"));

            //<shipmentIdentification>
            //  <gsin>11111110000834053</gsin>
            //</shipmentIdentification>

            var xmlShipper = xmlShipment.AppendChild(xmlDocument.CreateElement("shipper"));
            xmlShipper.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = inboundNotification.Shipper.Code;

            var xmlReceiver = xmlShipment.AppendChild(xmlDocument.CreateElement("receiver"));
            xmlReceiver.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = inboundNotification.Receiver.Code;

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

            xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundStatusCode")).InnerText = inboundNotification.State;
            var xmlActualReceipt = xmlShipment.AppendChild(xmlDocument.CreateElement("actualReceipt"));
            // time
            xmlActualReceipt.AppendChild(xmlDocument.CreateElement("logisticEventDateTime")).AppendChild(xmlDocument.CreateElement("date")).InnerText =
                inboundNotification.FactInboundDeliveryDate.ToString();

            var xmlInboundInstruction = xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundInstruction"));
            xmlInboundInstruction.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = 
                inboundNotification.InboundInstructionNumber;

            // Items
            foreach (var line in inboundNotification.Lines)
            {
                var xmlLine = xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationShipmentItem"));

                //< xsd:element minOccurs = "0" name = "actualReceiptQuantity" type = "shared_common:QuantityType" />                
                var xmlPlanQuantity = xmlLine.AppendChild(xmlDocument.CreateElement("actualAcceptedQuantity"));
                xmlPlanQuantity.InnerText = line.PlanQuantity.ToString();
                xmlPlanQuantity.Attributes.Append(xmlDocument.CreateAttribute("measurementUnitCode")).Value = line.measureUnitCode;
                var xmlFactQuantity = xmlLine.AppendChild(xmlDocument.CreateElement("plannedReceiptQuantity"));
                xmlFactQuantity.InnerText = line.FactQuantity.ToString();
                xmlPlanQuantity.Attributes.Append(xmlDocument.CreateAttribute("measurementUnitCode")).Value = line.measureUnitCode;
                //< xsd:element minOccurs = "0" name = "communicatedDespatchQuantity" type = "shared_common:QuantityType" />
            }

            return xmlDocument.OuterXml;
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
            var xmlDocument = new XmlDocument();
            var xmlRoot = xmlDocument.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationMessage"));

            var xmlHeader = xmlRoot.AppendChild(xmlDocument.CreateElement("sh:StandardBusinessDocumentHeader"));
            xmlHeader.AppendChild(xmlDocument.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            var xmlSender = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Sender"));
            xmlSender.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = outboundNotification.Sender.Code;

            var xmlRecipient = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Receiver"));
            xmlRecipient.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = outboundNotification.Recipient.Code;

            var xmlBody = xmlRoot.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotification"));
            xmlBody.AppendChild(xmlDocument.CreateElement("creationDateTime")).InnerText = outboundNotification.Date.ToString();
            xmlBody.AppendChild(xmlDocument.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            xmlBody.AppendChild(xmlDocument.CreateElement("documentActionCode")).InnerText = "ADD";

            var xmlIdentification = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationIdentification"));
            xmlIdentification.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = outboundNotification.Id.ToString();

            var xmlServiceBuyer = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesBuyer"));
            xmlServiceBuyer.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlServiceSeller = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesSeller"));
            xmlServiceSeller.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlShippment = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationShipment"));
            var xmlShipper = xmlShippment.AppendChild(xmlDocument.CreateElement("shipper"));
            xmlShipper.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = 
                outboundNotification.Shipper.Code;
            var xmlReceiver = xmlShippment.AppendChild(xmlDocument.CreateElement("receiver"));
            xmlReceiver.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                outboundNotification.Shipper.Code;

            xmlShippment.AppendChild(xmlDocument.CreateElement("warehousingOutboundStatusCode")).InnerText = outboundNotification.State;

            var lineNumber = 1;
            foreach (var line in outboundNotification.Lines)
            {
                var xmlLine = xmlShippment.AppendChild(xmlDocument.CreateElement("warehousingOutboundInstructionShipmentItem"));
                xmlLine.AppendChild(xmlDocument.CreateElement("lineItemNumber")).InnerText = lineNumber.ToString();
                xmlLine.AppendChild(xmlDocument.CreateElement("transactionalTradeItem")).AppendChild(xmlDocument.CreateElement("gtin")).InnerText = 
                    line.Product.Gtin;

                xmlLine.AppendChild(xmlDocument.CreateElement("plannedDespatchQuantity")).InnerText = line.PlanQuantity.ToString();
                xmlLine.AppendChild(xmlDocument.CreateElement("actualDespatchQuantity")).InnerText = line.FactQuantity.ToString();

                lineNumber++;
            }
            return xmlDocument.OuterXml;
        }

        #endregion
    }
}
