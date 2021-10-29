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

        /// <summary>
        /// Хранилище бизнес-объектов
        /// </summary>
        public ILinksStorage LinksStorage
        {
            set
            {
                _storage = value ?? throw new ArgumentNullException(nameof(value));
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
                            throw new Exception($"Legal entity {code} not found");
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
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var clients = new List<LegalEntity>();
            return clients.ToArray();
        }

        #endregion Клиенты (контрагенты)

        #region Товары

        private Product ProductFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var id = string.Empty;
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
                        var product = new Product(id, code, name, name);
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
            var id = -1;
            var number = string.Empty;
            var version = (byte)1;
            var date = DateTime.MinValue;
            var sender = LegalEntity.Null;
            var recipient = LegalEntity.Null;
            var dataSource = LegalEntity.Null;
            var dataRecipient = LegalEntity.Null;
            var note = string.Empty;
            var products = new List<Product>();
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

                        case "dataSource":
                            dataSource = LegalEntityFromXml(reader);
                            break;

                        case "dataRecipient":
                            dataRecipient = LegalEntityFromXml(reader);
                            break;

                        case "itemDataNotificationLineItem":
                            var product = ProductFromXml(reader);
                            products.Add(product);
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
            var productsList = new ProductsList(id, number, version, date, sender, recipient, dataSource, dataRecipient, note);
            productsList.AddProducts(products.ToArray());
            return productsList;
        }

        #endregion Товары

        #region Приход товара на склад

        private Inbound.Line InboundInstructionLineFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var gtin = string.Empty;
            var price = 0.0;
            var currencyCode = string.Empty;
            var planQuantity = 0.0;
            var factQuantity = 0.0;
            var measureUnitCode = string.Empty;
            var logisticUnits = new List<LogisticUnit>();
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
                            planQuantity = reader.ReadElementContentAsDouble();
                            break;

                        case "logisticUnit":
                            var logisticUnit = LogisticUnitFromXml(reader);
                            logisticUnits.Add(logisticUnit);
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
                        var measureUnit = _storage.GetMeasureUnit(measureUnitCode);
                        var currency = _storage.GetCurrency(currencyCode);
                        var line = new Inbound.Line(-1, product, price, currency, planQuantity, factQuantity, measureUnit);
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
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var inboundId = string.Empty;
            var inboundNumber = string.Empty;
            var inboundDate = DateTime.MinValue;
            var inboundPartner = LegalEntity.Null;
            var inboundPartnerNumber = string.Empty;
            var inboundStateCode = string.Empty;
            var inboundState = Entity.Null;
            var planAcceptanceBegin = DateTime.MinValue;
            var planAcceptanceEnd = DateTime.MinValue;
            var inboundType = Entity.Null;
            var inboundNote = string.Empty;
            var inboundLogisticUnits = new List<LogisticUnit>();
            var inboundLines = new List<Inbound.Line>();

            var id = -1;
            var version = (byte)1;
            var number = string.Empty;
            var date = DateTime.MinValue;
            var sender = LegalEntity.Null;
            var recipient = LegalEntity.Null;
            var serviceBuyer = LegalEntity.Null;
            var serviceSeller = LegalEntity.Null;
            var shipper = LegalEntity.Null;
            var receiver = LegalEntity.Null;
            var note = string.Empty;

            var beginDate = string.Empty;
            var beginTime = string.Empty;
            var endDate = string.Empty;
            var endTime = string.Empty;

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
                            inboundDate = Convert.ToDateTime(reader.ReadElementContentAsString());
                            break;

                        case "entityIdentification":
                            number = reader.ReadElementContentAsString();
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
                            inboundPartnerNumber = reader.ReadElementContentAsString();
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
                            inboundLogisticUnits.Add(LogisticUnitFromXml(reader));
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
                            inboundLines.Add(InboundInstructionLineFromXml(reader));
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }
            // load from db (number dispenser idk)
            // TODO добавить в Oracle функцию генерации разнородных ID
            inboundNumber = "Tr100067168";

            // TODO загружать тип из XML
            //inboundType = new Entity(-1, "loaded_from_xml");

            var inbound = new Inbound(inboundId, inboundNumber, inboundDate,
                inboundPartner, inboundPartnerNumber, inboundState, inboundNote);
            planAcceptanceBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            planAcceptanceEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));
            inbound.PlanAcceptanceBegin = planAcceptanceBegin;
            inbound.PlanAcceptanceEnd = planAcceptanceEnd;
            inbound.Type = inboundType;
            inbound.Vendor = shipper; // todo check if there is this Gp in our DB 

            var inboundInstruction = new InboundInstruction(id, number, version, date, inbound,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);

            // TODO: сопоставить позиции с местами

            //inboundInstruction.Inbound.AddLogisticUnits(logisticUnits.ToArray());
            inboundInstruction.Inbound.AddLines(inboundLines.ToArray());

            return inboundInstruction;
        }

        public string InboundNotificationToXml(InboundNotification inboundNotification)
        {
            if (inboundNotification == null)
                throw new ArgumentNullException(nameof(inboundNotification));
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

            foreach (var logisticUnit in inboundNotification.Inbound.LogisticUnits)
            {
                var xmlLogisticUnit = xmlShipment.AppendChild(xmlDocument.CreateElement("logisticUnit"));
                xmlLogisticUnit.AppendChild(xmlDocument.CreateElement("sscc")).InnerText = logisticUnit.Sscc;
                //<packageTypeCode>AF</packageTypeCode>
            }
            /*<packageTotal>
                <packageTypeCode>AF</packageTypeCode>
                <totalPackageQuantity>2</totalPackageQuantity>
            </packageTotal> */

            xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundStatusCode")).InnerText = inboundNotification.State;
            var xmlActualReceipt = xmlShipment.AppendChild(xmlDocument.CreateElement("actualReceipt"));
            // time
            xmlActualReceipt.AppendChild(xmlDocument.CreateElement("logisticEventDateTime")).AppendChild(xmlDocument.CreateElement("date")).InnerText =
                inboundNotification.Inbound.FactAcceptanceEnd.ToString();

            var xmlInboundInstruction = xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundInstruction"));
            xmlInboundInstruction.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText =
                inboundNotification.InstructionNumber;

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

        #endregion Приход товара на склад

        #region Убытие товара со склада

        private Outbound.Line OutboundInstructionLineFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var number = -1;
            var gtin = string.Empty;
            var price = 0.0;
            var currencyCode = string.Empty;
            var planQuantity = 0.0;
            var factQuantity = 0.0;
            var measureUnitCode = string.Empty;
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
                            planQuantity = reader.ReadElementContentAsDouble();
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
                        var currency = _storage.GetCurrency(currencyCode);
                        var measureUnit = _storage.GetMeasureUnit(measureUnitCode);
                        var line = new Outbound.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit);
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
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var outboundId = string.Empty;
            var outboundNumber = string.Empty;
            var outboundDate = DateTime.MinValue;
            var outboundPartner = LegalEntity.Null;
            var outboundPartnerNumber = string.Empty;
            var outboundPlanShippingBegin = DateTime.MinValue;
            var outboundPlanShippingEnd = DateTime.MinValue;
            var outboundStateCode = string.Empty;
            var outboundState = Entity.Null;
            var outboundNote = string.Empty;

            var id = -1;
            var number = string.Empty;
            var version = (byte)1;
            var date = DateTime.MinValue;
            var sender = LegalEntity.Null;
            var recipient = LegalEntity.Null;
            var serviceBuyer = LegalEntity.Null;
            var serviceSeller = LegalEntity.Null;
            var shipper = LegalEntity.Null;
            var receiver = LegalEntity.Null;
            var note = string.Empty;

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
                            outboundDate = Convert.ToDateTime(reader.ReadElementContentAsString());
                            break;

                        case "entityIdentification":
                            outboundPartnerNumber = reader.ReadElementContentAsString();
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

            // load from db (number dispenser idk)
            // TODO добавить в Oracle функцию генерации разнородных ID
            outboundNumber = "Tr100067168";

            outboundPartner = serviceBuyer; // Партнёр = покупатель наших складских и логистических услуг
            outboundPlanShippingBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            outboundPlanShippingEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));
            outboundState = _storage.GetOutboundState(outboundStateCode);
            var outbound = new Outbound(outboundId, outboundNumber, outboundPartner, outboundPartnerNumber, outboundState, outboundNote);
            outbound.PlanShippingBegin = outboundPlanShippingBegin;
            outbound.PlanShippingEnd = outboundPlanShippingEnd;
            outbound.Vendor = receiver;
            outbound.DeliveryAddress1 = receiver.Address;

            note = outboundNote;
            var outboundInstruction = new OutboundInstruction(id, number, version, date, outbound,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);
            return outboundInstruction;
        }

        public string OutboundNotificationToXml(OutboundNotification outboundNotification)
        {
            if (outboundNotification == null)
                throw new ArgumentNullException(nameof(outboundNotification));
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

            xmlShippment.AppendChild(xmlDocument.CreateElement("warehousingOutboundStatusCode")).InnerText =
                outboundNotification.Outbound.State.Code;

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

        #endregion Убытие товара со склада
    }
}