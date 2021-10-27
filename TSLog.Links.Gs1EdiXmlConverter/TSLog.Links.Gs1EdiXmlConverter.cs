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
                throw new ArgumentNullException(nameof(reader));
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
                throw new ArgumentNullException(nameof(reader));
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
                throw new ArgumentNullException(nameof(reader));
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
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var code = string.Empty;
            var name = string.Empty;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "tradeItemDescription":
                            name = reader.ReadElementContentAsString();
                            break;
                        case "tradeItemIdentification":
                            reader.Read();
                            break;
                        case "gtin":
                            code = reader.ReadElementContentAsString();
                            break;
                        default:
                            reader.Read();
                            break;
                    }
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
                throw new ArgumentNullException(nameof(reader));
            var productsList = new ProductsList(-1, 1);
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "sh:Sender":
                            productsList.Sender = SideFromXml(reader);
                            break;
                        case "sh:Receiver":
                            productsList.Recipient = SideFromXml(reader);
                            break;
                        case "dataSource":
                            productsList.DataSource = LegalEntityFromXml(reader);
                            break;
                        case "dataRecipient":
                            productsList.DataRecipient = LegalEntityFromXml(reader);
                            break;
                        case "itemDataNotificationLineItem":
                            productsList.AddProduct(ProductFromXml(reader));
                            break;
                        default:
                            reader.Read();
                            break;
                    }
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

        private Inbound.Line InboundInstructionLineFromXml(XmlReader reader, out string[] logisticUnitsCodes)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var gtin = string.Empty;
            var quantity = 0.0;
            logisticUnitsCodes = Array.Empty<string>();
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "transactionalTradeItem":
                            reader.Read();
                            break;
                        case "gtin":
                            gtin = reader.ReadElementContentAsString();
                            break;
                        case "plannedReceiptQuantity":
                            quantity = reader.ReadElementContentAsDouble();
                            break;
                        // TODO: parse logistic units
                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var product = _storage.GetProduct(gtin);
                        MeasureUnit measureUnit = null;
                        var line = new Inbound.Line(-1, product, 0.0, Entity.Null, quantity, measureUnit);
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
            var inboundInstruction = new InboundInstruction(1);

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
            var lines = new List<Inbound.Line>();

            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "sh:Sender":
                            sender = SideFromXml(reader);
                            break;
                        case "sh:Receiver":
                            recipient = SideFromXml(reader);
                            break;
                        case "creationDateTime":
                            inboundInstruction.Inbound.Date = Convert.ToDateTime(reader.ReadElementContentAsString());
                            break;
                        case "entityIdentification":
                            inboundInstruction.Number = reader.ReadElementContentAsString();
                            break;
                        case "logisticServicesBuyer":
                            serviceBuyer = LegalEntityFromXml(reader);
                            break;
                        case "logisticServicesSeller":
                            serviceSeller = LegalEntityFromXml(reader);
                            break;

                        // Global Shipment Identification Number,
                        // Глобальный номер для идентификации отгрузки
                        case "gsin":
                            inboundInstruction.Inbound.PartnerNumber = reader.ReadElementContentAsString();
                            break;
                        case "warehousingInboundInstructionShipment":
                            reader.Read();
                            break;
                        case "shipmentIdentification":
                            reader.Read();
                            break;
                        case "shipper":
                            shipper = LegalEntityFromXml(reader);
                            break;
                        case "receiver":
                            receiver = LegalEntityFromXml(reader);
                            break;
                        case "logisticUnit":
                            logisticUnits.Add(LogisticUnitFromXml(reader));
                            break;
                        case "plannedReceipt":
                            reader.Read();
                            break;
                        case "logisticEventPeriod":
                            reader.Read();
                            break;

                        /* TODO: parse this block
                        <packageTotal>
                            <packageTypeCode>AF</packageTypeCode>
                            <totalPackageQuantity>2</totalPackageQuantity>
                        </packageTotal>
                        <warehousingReceiptTypeCode>PRIORITY_RECEIPT</warehousingReceiptTypeCode> */

                        case "beginDate":
                            beginDate = reader.ReadElementContentAsString();
                            break;
                        case "beginTime":
                            beginTime = reader.ReadElementContentAsString();
                            break;
                        case "endDate":
                            endDate = reader.ReadElementContentAsString();
                            break;
                        case "endTime":
                            endTime = reader.ReadElementContentAsString();
                            break;
                        case "warehousingInboundInstructionShipmentItem":
                            lines.Add(InboundInstructionLineFromXml(reader, out logisticUnitsCodes));
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }
            inboundInstruction.Sender = sender;
            inboundInstruction.Recipient = recipient;
            inboundInstruction.ServiceBuyer = serviceBuyer;
            inboundInstruction.ServiceSeller = serviceSeller;
            inboundInstruction.Shipper = shipper;
            inboundInstruction.Receiver = receiver;
                        
            inboundInstruction.PlanDeliveryBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            inboundInstruction.PlanDeliveryEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));

            // TODO: сопоставить позиции с местами

            //inboundInstruction.Inbound.AddLogisticUnits(logisticUnits.ToArray());
            inboundInstruction.Inbound.AddLines(lines.ToArray());
                
            return inboundInstruction;
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
            foreach (var line in inboundNotification.Inbound.Lines)
            {
                var xmlLine = xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationShipmentItem"));

                //< xsd:element minOccurs = "0" name = "actualReceiptQuantity" type = "shared_common:QuantityType" />                
                var xmlPlanQuantity = xmlLine.AppendChild(xmlDocument.CreateElement("actualAcceptedQuantity"));
                xmlPlanQuantity.InnerText = line.PlanQuantity.ToString();
                xmlPlanQuantity.Attributes.Append(xmlDocument.CreateAttribute("measurementUnitCode")).Value = line.MeasureUnit.Code;
                var xmlFactQuantity = xmlLine.AppendChild(xmlDocument.CreateElement("plannedReceiptQuantity"));
                xmlFactQuantity.InnerText = line.FactQuantity.ToString();
                xmlPlanQuantity.Attributes.Append(xmlDocument.CreateAttribute("measurementUnitCode")).Value = line.MeasureUnit.Code;
                //< xsd:element minOccurs = "0" name = "communicatedDespatchQuantity" type = "shared_common:QuantityType" />
            }

            return xmlDocument.OuterXml;
        }
        #endregion

        #region Убытие товара со склада
        private Outbound.Line OutboundInstructionLineFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var closeTag = reader.Name;
            var gtin = string.Empty;
            var quantity = 0.0;

            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "transactionalTradeItem":
                            reader.Read();
                            break;
                        case "gtin":
                            gtin = reader.ReadElementContentAsString();
                            break;
                        case "plannedDespatchQuantity":
                            quantity = reader.ReadElementContentAsDouble();
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == closeTag)
                    {
                        var product = _storage.GetProduct(gtin);
                        MeasureUnit measureUnit = null;
                        var line = new Outbound.Line(-1, product, quantity, measureUnit);
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
            var lines = new List<Outbound.Line>();

            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "sh:Sender":
                            sender = SideFromXml(reader);
                            break;
                        case "sh:Receiver":
                            recipient = SideFromXml(reader);
                            break;
                        case "creationDateTime":
                            outboundInstruction.Date = Convert.ToDateTime(reader.ReadElementContentAsString());
                            break;
                        case "entityIdentification":
                            outboundInstruction.Number = reader.ReadElementContentAsString();
                            break;
                        case "logisticServicesBuyer":
                            serviceBuyer = LegalEntityFromXml(reader);
                            break;
                        case "logisticServicesSeller":
                            serviceSeller = LegalEntityFromXml(reader);
                            break;
                        case "shipper":
                            shipper = LegalEntityFromXml(reader);
                            break;
                        case "receiver":
                            receiver = LegalEntityFromXml(reader);
                            break;
                        case "plannedDelivery":
                            reader.Read();
                            break;
                        case "logisticEventPeriod":
                            reader.Read();
                            break;
                        case "beginDate":
                            beginDate = reader.ReadElementContentAsString();
                            break;
                        case "beginTime":
                            beginTime = reader.ReadElementContentAsString();
                            break;
                        case "endDate":
                            endDate = reader.ReadElementContentAsString();
                            break;
                        case "endTime":
                            endTime = reader.ReadElementContentAsString();
                            break;
                        case "warehousingOutboundInstructionShipmentItem":
                            lines.Add(OutboundInstructionLineFromXml(reader));
                            break;
                        default:
                            reader.Read();
                            break;
                    }
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
            outboundInstruction.Outbound.AddLines(lines.ToArray());

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
            foreach (var line in outboundNotification.Outbound.Lines)
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
