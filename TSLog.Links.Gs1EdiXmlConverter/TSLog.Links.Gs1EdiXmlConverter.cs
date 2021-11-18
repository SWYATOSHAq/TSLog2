using System;
using System.Collections.Generic;
using System.Linq;
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
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                {
                    var side = _storage.GetPartner(id);
                    return side;
                }
                else
                    reader.Read();
            }
            return LegalEntity.Null;
        }

        private LegalEntity LegalEntityFromXml(XmlReader reader, bool createNew = false)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var id = string.Empty;
            var name = string.Empty;
            var note = string.Empty;
            var infoOne = string.Empty;
            var infoTwo = string.Empty;
            var fullAddress = string.Empty;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "gln":
                            reader.Read();
                            break;

                        case "additionalPartyIdentification":
                            id = reader.ReadElementContentAsString();
                            break;

                        case "address":
                            reader.Read();
                            break;

                        case "note":
                            note = reader.ReadElementContentAsString();
                            break;

                        case "infoOne":
                            infoOne = reader.ReadElementContentAsString();
                            break;

                        case "infoTwo":
                            infoTwo = reader.ReadElementContentAsString();
                            break;

                        case "entityName":
                            name = reader.ReadElementContentAsString();
                            break;

                        case "streetAddressOne":
                            fullAddress = reader.ReadElementContentAsString();
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                {
                    var legalEntity = LegalEntity.Null;
                    if (!createNew)
                    {
                        legalEntity = _storage.GetPartner(id);

                        if (legalEntity == null || legalEntity == LegalEntity.Null)
                            legalEntity = _storage.GetLegalEntity(id);

                        if (legalEntity == null || legalEntity == LegalEntity.Null)
                            throw new Exception($"Legal entity {id} not found");
                    }
                    else
                    {
                        legalEntity = new LegalEntity(id, name, note, infoOne, infoTwo)
                        {
                            Address = fullAddress
                        };
                    }
                    return legalEntity;
                }
                else
                    reader.Read();
            }
            return LegalEntity.Null;
        }

        private LogisticUnit LogisticUnitFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var sscc = string.Empty;
            var typeCode = string.Empty;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "sscc":
                            sscc = reader.ReadElementContentAsString();
                            break;

                        case "packageTypeCode":
                            typeCode = reader.ReadElementContentAsString();
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                {
                    var unit = new LogisticUnit(typeCode, sscc);
                    return unit;
                }
                else
                    reader.Read();
            }
            return null;
        }

        #region Клиенты (контрагенты)

        public ClientsList ClientsFromXml(XmlReader reader)
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
            var clients = new List<LegalEntity>();
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

                        case "entityDataNotificationLineItem":
                            var product = LegalEntityFromXml(reader, true);
                            clients.Add(product);
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }

            var clientsList = new ClientsList(id, number, version, date, sender, recipient, dataSource, dataRecipient, note);
            clientsList.AddClients(clients.ToArray());
            return clientsList;


            //// TODO
            //var clientsList = new ClientsList(-1, "", 0, DateTime.Now, LegalEntity.Null, LegalEntity.Null, LegalEntity.Null, LegalEntity.Null, "pizdec");

            //if (reader == null)
            //    throw new ArgumentNullException(nameof(reader));
            //var clients = new List<LegalEntity>();

            ////return clients.ToArray();
            //clientsList.AddClients(clients.ToArray()); // TODO ???

            //return clientsList;
        }

        #endregion Клиенты (контрагенты)

        #region Товары

        private Product ProductFromXml(XmlReader reader, string prodctCode)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var id = string.Empty;
            var gtin = string.Empty;
            var name = string.Empty;

            var depth = 0;
            var width = 0;
            var height = 0;

            var grossWeight = 0.0;
            var netWeight = 0.0;

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
                            gtin = reader.ReadElementContentAsString();
                            break;

                        case "depth":
                            depth = reader.ReadElementContentAsInt();
                            break;

                        case "height":
                            width = reader.ReadElementContentAsInt();
                            break;

                        case "width":
                            height = reader.ReadElementContentAsInt();
                            break;

                        case "netWeight":
                            netWeight = reader.ReadElementContentAsDouble();
                            break;

                        case "grossWeight":
                            grossWeight = reader.ReadElementContentAsDouble();
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                {

                    var product = new Product(id, prodctCode, gtin, name)
                    {
                        Width = width,
                        Height = height,
                        Length = depth,

                        Netto = netWeight,
                        Brutto = grossWeight
                    };

                    return product;
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
                            var code = _storage.GenerateObjectId(DocumentObjectTypes.ArticleWMS.ToString(), dataSource);
                            var product = ProductFromXml(reader, code);
                            products.Add(product);
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }

            var productsList = new ProductsList(id, number, version, date, sender, recipient, dataSource, dataRecipient, note);
            productsList.AddProducts(products.ToArray());
            return productsList;
        }

        #endregion Товары

        #region Приход товара на склад

        private string IncomeRequestLineLogisticUnitFromXml(XmlReader reader, out double quantity)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var closeTag = reader.Name;
            var sscc = string.Empty;
            quantity = 0.0;
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "sscc":
                            sscc = reader.ReadElementContentAsString();
                            break;

                        case "tradeItemQuantity":
                            quantity = reader.ReadElementContentAsDouble();
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                    return sscc;
                else
                    reader.Read();
            }
            return sscc;
        }

        private Income.Line IncomeRequestLineFromXml(XmlReader reader, IDictionary<string, LogisticUnit> logisticUnits)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (logisticUnits == null)
                throw new ArgumentNullException(nameof(logisticUnits));
            var closeTag = reader.Name;
            var number = -1;
            var gtin = string.Empty;
            var price = 0.0;
            var currencyCode = string.Empty;
            var planQuantity = 0.0;
            var factQuantity = 0.0;
            var measureUnitCode = string.Empty;
            var lineLogisticUnits = new Dictionary<LogisticUnit, double>();
            var note = string.Empty;
            var OKEI = string.Empty;
            var BatchNumber = string.Empty;
            var BatchNumber2 = string.Empty;

            var planQuantityByLogsiticUnits = 0.0;

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

                        case "associatedInvoicePrice":
                            price = reader.ReadElementContentAsDouble();
                            break;

                        case "lineItemNumber":
                            number = reader.ReadElementContentAsInt();
                            break;

                        case "plannedReceiptQuantity":
                            planQuantity = reader.ReadElementContentAsDouble();
                            break;

                        case "note":
                            note = reader.ReadElementContentAsString();
                            break;

                        case "eComStringAttributeValuePairList":
                            var tmpAttributeVal = reader.GetAttribute(0);
                            switch (tmpAttributeVal)
                            {
                                case "OKEI":
                                    OKEI = reader.ReadElementContentAsString();
                                    break;
                                case "BatchNumber":
                                    BatchNumber = reader.ReadElementContentAsString();
                                    break;
                                case "BatchNumber2":
                                    BatchNumber2 = reader.ReadElementContentAsString();
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case "logisticUnit":
                            var logisticUnitQuantity = 0.0;
                            var sscc = IncomeRequestLineLogisticUnitFromXml(reader, out logisticUnitQuantity);
                            if (!logisticUnits.ContainsKey(sscc))
                                throw new Exception($"Место по коду {sscc} не найдено");
                            if (!lineLogisticUnits.ContainsKey(logisticUnits[sscc]))
                                lineLogisticUnits.Add(logisticUnits[sscc], logisticUnitQuantity);
                            else
                                lineLogisticUnits[logisticUnits[sscc]] += logisticUnitQuantity;
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                {
                    var product = _storage.GetProduct(gtin);
                    if (product == Product.Null)
                        product = _storage.GetProduct(gtin.TrimStart('0')); // try again // TODO throw an eror becouse we CANT accept gtin with wrong length

                    var measureUnit = _storage.GetMeasureUnit(measureUnitCode);
                    var currency = _storage.GetCurrency(currencyCode);
                    var line = new Income.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit, note)
                    {
                        OKEI = OKEI,
                        BatchNumber = BatchNumber,
                        BatchNumber2 = BatchNumber2
                    };

                    foreach (var pair in lineLogisticUnits)
                    {
                        planQuantityByLogsiticUnits += pair.Value;
                        line.AddLogisticUnit(pair.Key, pair.Value);
                    }

                    if (lineLogisticUnits.Count == 0)
                        planQuantityByLogsiticUnits = planQuantity;

                    // check for data integrity (planQuantity)
                    if (planQuantityByLogsiticUnits != planQuantity)
                    {
                        throw new Exception($"data integrity exception: количество plannedReceiptQuantity ({planQuantity}) и количетво на logisticUnit ({planQuantityByLogsiticUnits}) отличается");
                    }

                    return line;
                }
                else
                    reader.Read();
            }
            return null;
        }

        public IncomeRequest IncomeRequestFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var incomeId = string.Empty;
            var incomeNumber = string.Empty;
            var incomeDate = DateTime.MinValue;
            var incomePartner = LegalEntity.Null;
            var incomePartnerNumber = string.Empty;
            var incomeStateCode = string.Empty;
            var incomeState = Entity.Null;
            var planAcceptanceBegin = DateTime.MinValue;
            var planAcceptanceEnd = DateTime.MinValue;
            var incomeType = Entity.Null;
            var incomeNote = string.Empty;
            var incomeLogisticUnits = new Dictionary<string, LogisticUnit>();
            var incomeLines = new List<Income.Line>();

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

            var operationType = OperationType.Undefined;
            var additionalGUID = string.Empty;

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
                            incomeDate = Convert.ToDateTime(reader.ReadElementContentAsString());
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
                            incomePartnerNumber = reader.ReadElementContentAsString();
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
                        //

                        case "operationType":
                            operationType = OperationConverter.Convert(reader.ReadElementContentAsString());
                            break;

                        case "eComStringAttributeValuePairList":
                            if (reader.GetAttribute(0) == "GUID")
                                additionalGUID = reader.ReadElementContentAsString();
                            break;

                        case "logisticUnit":
                            var inboundLogisticUnit = LogisticUnitFromXml(reader);
                            if (incomeLogisticUnits.ContainsKey(inboundLogisticUnit.Sscc))
                                throw new Exception($"SSCC код {inboundLogisticUnit.Sscc} места дублируется");
                            incomeLogisticUnits.Add(inboundLogisticUnit.Sscc, inboundLogisticUnit);
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
                            var inboundLine = IncomeRequestLineFromXml(reader, incomeLogisticUnits);
                            incomeLines.Add(inboundLine);
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }
            incomeNumber = _storage.GenerateObjectId(DocumentObjectTypes.DocWMS.ToString(), serviceBuyer);

            // TODO загружать тип из XML
            //inboundType = new Entity(-1, "loaded_from_xml");

            incomeState = _storage.GetIncomeState(incomeStateCode);
            var income = new Income(incomeId, incomeNumber, incomeDate,
                incomePartner, incomePartnerNumber, incomeState, incomeNote);
            planAcceptanceBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            planAcceptanceEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));
            income.PlanAcceptanceBegin = planAcceptanceBegin;
            income.PlanAcceptanceEnd = planAcceptanceEnd;
            income.AddLogisticUnits(incomeLogisticUnits.Values.ToArray());
            income.AddLines(incomeLines.ToArray());
            income.Type = incomeType;
            income.Vendor = shipper; // todo check if there is this Gp in our DB
            income.Operation = operationType;
            income.GUID = additionalGUID;

            var incomeRequest = new IncomeRequest(id, number, version, date, income,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);

            // TODO: сопоставить позиции с местами

            //inboundInstruction.Inbound.AddLogisticUnits(logisticUnits.ToArray());
            incomeRequest.Income.AddLines(incomeLines.ToArray());

            return incomeRequest;
        }

        public string ReceiptEndNoticeToXml(ReceiptEndNotice receiptEndNotice)
        {
            if (receiptEndNotice == null)
                throw new ArgumentNullException(nameof(receiptEndNotice));
            var xmlDocument = new XmlDocument();
            var xmlRoot = xmlDocument.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationMessage"));

            var xmlHeader = xmlRoot.AppendChild(xmlDocument.CreateElement("sh:StandardBusinessDocumentHeader"));
            xmlHeader.AppendChild(xmlDocument.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            var xmlSender = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Sender"));
            xmlSender.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = receiptEndNotice.Sender.Code;
            var xmlRecipient = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Receiver"));
            xmlRecipient.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = receiptEndNotice.Recipient.Code;

            var xmlBody = xmlRoot.AppendChild(xmlDocument.CreateElement("warehousingInboundNotification"));

            xmlBody.AppendChild(xmlDocument.CreateElement("creationDateTime")).InnerText = receiptEndNotice.Date.ToString();
            xmlBody.AppendChild(xmlDocument.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            xmlBody.AppendChild(xmlDocument.CreateElement("documentActionCode")).InnerText = "ADD";

            var xmlIdentification = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationIdentification"));
            xmlIdentification.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = receiptEndNotice.Income.Code;// inboundNotification.Id.ToString(); // TODO idk what to use as entityIdentification

            var xmlServiceBuyer = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesBuyer"));
            xmlServiceBuyer.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = receiptEndNotice.ServiceBuyer.Code;

            var xmlServiceSeller = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesSeller"));
            xmlServiceSeller.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = receiptEndNotice.ServiceSeller.Code;

            var xmlShipment = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationShipment"));
            var xmlShipmentIdentification = xmlShipment.AppendChild(xmlDocument.CreateElement("shipmentIdentification"));
            xmlShipmentIdentification.AppendChild(xmlDocument.CreateElement("gsin")).InnerText = receiptEndNotice.Income.Id;

            var xmlShipper = xmlShipment.AppendChild(xmlDocument.CreateElement("shipper"));
            xmlShipper.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = receiptEndNotice.Shipper.Code;

            var xmlReceiver = xmlShipment.AppendChild(xmlDocument.CreateElement("receiver"));
            xmlReceiver.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText = receiptEndNotice.Receiver.Code;

            foreach (var logisticUnit in receiptEndNotice.Income.LogisticUnits)
            {
                var xmlLogisticUnit = xmlShipment.AppendChild(xmlDocument.CreateElement("logisticUnit"));
                xmlLogisticUnit.AppendChild(xmlDocument.CreateElement("sscc")).InnerText = logisticUnit.Sscc;
                //<packageTypeCode>AF</packageTypeCode>
            }
            /*<packageTotal>
                <packageTypeCode>AF</packageTypeCode>
                <totalPackageQuantity>2</totalPackageQuantity>
            </packageTotal> */

            xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundStatusCode")).InnerText = receiptEndNotice.State;
            var xmlActualReceipt = xmlShipment.AppendChild(xmlDocument.CreateElement("actualReceipt"));
            // time
            xmlActualReceipt.AppendChild(xmlDocument.CreateElement("logisticEventDateTime")).AppendChild(xmlDocument.CreateElement("date")).InnerText =
                receiptEndNotice.Income.FactAcceptanceEnd.ToString();

            var xmlInboundInstruction = xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundInstruction"));
            xmlInboundInstruction.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText =
                receiptEndNotice.InstructionNumber;

            // Items
            foreach (var line in receiptEndNotice.Income.Lines)
            {
                var xmlLine = xmlShipment.AppendChild(xmlDocument.CreateElement("warehousingInboundNotificationShipmentItem"));

                //< xsd:element minOccurs = "0" name = "actualReceiptQuantity" type = "shared_common:QuantityType" />
                var xmlPlanQuantity = xmlLine.AppendChild(xmlDocument.CreateElement("actualAcceptedQuantity"));
                xmlPlanQuantity.InnerText = line.PlanQuantity.ToString();
                xmlPlanQuantity.Attributes.Append(xmlDocument.CreateAttribute("measurementUnitCode")).Value = line.MeasureUnit.Code;
                var xmlFactQuantity = xmlLine.AppendChild(xmlDocument.CreateElement("plannedReceiptQuantity"));
                xmlFactQuantity.InnerText = line.FactQuantity.ToString();
                xmlPlanQuantity.Attributes.Append(xmlDocument.CreateAttribute("measurementUnitCode")).Value = line.MeasureUnit.Code;
                //<xsd:element minOccurs = "0" name = "communicatedDespatchQuantity" type = "shared_common:QuantityType"/>
            }
            return xmlDocument.OuterXml;
        }

        #endregion Приход товара на склад

        #region Убытие товара со склада

        private Outgo.Line OutgoRequestLineFromXml(XmlReader reader)
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

            var OKEI = string.Empty;
            var BatchNumber = string.Empty;
            var BatchNumber2 = string.Empty;
            var note = string.Empty;

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
                            var tmpMeasureUnit = reader.GetAttribute("measurementUnitCode");
                            planQuantity = reader.ReadElementContentAsDouble();

                            if (tmpMeasureUnit  != null)
                                measureUnitCode = tmpMeasureUnit;
                            break;

                        case "note":
                            note = reader.ReadElementContentAsString();
                            break;

                        case "eComStringAttributeValuePairList":
                            var tmpAttributeVal = reader.GetAttribute(0);
                            switch (tmpAttributeVal)
                            {
                                case "OKEI":
                                    OKEI = reader.ReadElementContentAsString();
                                    break;
                                case "BatchNumber":
                                    BatchNumber = reader.ReadElementContentAsString();
                                    break;
                                case "BatchNumber2":
                                    BatchNumber2 = reader.ReadElementContentAsString();
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case "lineItemNumber":
                            number = reader.ReadElementContentAsInt();
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == closeTag)
                {
                    var product = _storage.GetProduct(gtin); 
                    if (product == Product.Null)
                        product = _storage.GetProduct(gtin.TrimStart('0')); // try again // TODO throw an eror becouse we CANT accept gtin with wrong length

                    var currency = _storage.GetCurrency(currencyCode);
                    var measureUnit = _storage.GetMeasureUnit(measureUnitCode);
                    var line = new Outgo.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit)
                    {
                        OKEI = OKEI,
                        BatchNumber = BatchNumber,
                        BatchNumber2 = BatchNumber2,
                        Info = note
                    };

                    return line;
                }
                else
                    reader.Read();
            }
            return null;
        }

        public OutgoRequest OutgoRequestFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var outgoId = string.Empty;
            var outgoNumber = string.Empty;
            var outgoDate = DateTime.MinValue;
            var outgoPartner = LegalEntity.Null;
            var outgoPartnerNumber = string.Empty;
            var outgoPlanShippingBegin = DateTime.MinValue;
            var outgoPlanShippingEnd = DateTime.MinValue;
            var outgoStateCode = string.Empty;
            var outgoState = Entity.Null;
            var outgoNote = string.Empty;

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
            var lines = new List<Outgo.Line>();

            var operationType = OperationType.Undefined;
            var additionalGUID = string.Empty;

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
                            outgoDate = Convert.ToDateTime(reader.ReadElementContentAsString());
                            break;

                        case "entityIdentification":
                            outgoPartnerNumber = reader.ReadElementContentAsString();
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

                        case "operationType":
                            operationType = OperationConverter.Convert(reader.ReadElementContentAsString());
                            break;

                        case "eComStringAttributeValuePairList":
                            if (reader.GetAttribute(0) == "GUID")
                                additionalGUID = reader.ReadElementContentAsString();
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
                            lines.Add(OutgoRequestLineFromXml(reader));
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }

            outgoNumber = _storage.GenerateObjectId(DocumentObjectTypes.DocWMS.ToString(), serviceBuyer);

            outgoPartner = serviceBuyer; // Партнёр = покупатель наших складских и логистических услуг
            outgoPlanShippingBegin = Convert.ToDateTime(string.Format("{0} {1}", beginDate, beginTime));
            outgoPlanShippingEnd = Convert.ToDateTime(string.Format("{0} {1}", endDate, endTime));
            outgoState = _storage.GetOutgoState(outgoStateCode);
            var outbound = new Outgo(outgoId, outgoNumber, outgoDate,
                outgoPartner, outgoPartnerNumber, outgoState, outgoNote);

            outbound.AddLines(lines.ToArray());

            outbound.PlanShippingBegin = outgoPlanShippingBegin;
            outbound.PlanShippingEnd = outgoPlanShippingEnd;
            outbound.Vendor = receiver;
            outbound.DeliveryAddress1 = receiver.Address;

            outbound.Operation = operationType;
            outbound.GUID = additionalGUID;

            note = outgoNote;
            var outboundInstruction = new OutgoRequest(id, number, version, date, outbound,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);
            return outboundInstruction;
        }

        public string PickingBeginNoticeToXml(PickingBeginNotice pickingBeginNotice)
        {
            if (pickingBeginNotice == null)
                throw new ArgumentNullException(nameof(pickingBeginNotice));
            var xmlDocument = new XmlDocument();
            var xmlRoot = xmlDocument.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationMessage"));

            var xmlHeader = xmlRoot.AppendChild(xmlDocument.CreateElement("sh:StandardBusinessDocumentHeader"));
            xmlHeader.AppendChild(xmlDocument.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            var xmlSender = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Sender"));
            xmlSender.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = pickingBeginNotice.Sender.Code;

            var xmlRecipient = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Receiver"));
            xmlRecipient.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = pickingBeginNotice.Recipient.Code;

            var xmlBody = xmlRoot.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotification"));
            xmlBody.AppendChild(xmlDocument.CreateElement("creationDateTime")).InnerText = pickingBeginNotice.Date.ToString();
            xmlBody.AppendChild(xmlDocument.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            xmlBody.AppendChild(xmlDocument.CreateElement("documentActionCode")).InnerText = "ADD";

            var xmlIdentification = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationIdentification"));
            xmlIdentification.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = pickingBeginNotice.Outgo.Code;// outboundNotification.Id.ToString(); // TODO idk what tu use in entityIdentification

            var xmlServiceBuyer = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesBuyer"));
            xmlServiceBuyer.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlServiceSeller = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesSeller"));
            xmlServiceSeller.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlShippment = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationShipment"));
            var xmlShipper = xmlShippment.AppendChild(xmlDocument.CreateElement("shipper"));
            xmlShipper.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                pickingBeginNotice.Shipper.Code;
            var xmlReceiver = xmlShippment.AppendChild(xmlDocument.CreateElement("receiver"));
            xmlReceiver.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                pickingBeginNotice.Shipper.Code;

            xmlShippment.AppendChild(xmlDocument.CreateElement("warehousingOutboundStatusCode")).InnerText =
                pickingBeginNotice.Outgo.State.Code;

            var lineNumber = 1;
            foreach (var line in pickingBeginNotice.Outgo.Lines)
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

        public string PickingEndNoticeToXml(PickingEndNotice pickingEndNotice)
        {
            if (pickingEndNotice == null)
                throw new ArgumentNullException(nameof(pickingEndNotice));
            var xmlDocument = new XmlDocument();
            var xmlRoot = xmlDocument.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationMessage"));

            var xmlHeader = xmlRoot.AppendChild(xmlDocument.CreateElement("sh:StandardBusinessDocumentHeader"));
            xmlHeader.AppendChild(xmlDocument.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            var xmlSender = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Sender"));
            xmlSender.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = pickingEndNotice.Sender.Code;

            var xmlRecipient = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Receiver"));
            xmlRecipient.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = pickingEndNotice.Recipient.Code;

            var xmlBody = xmlRoot.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotification"));
            xmlBody.AppendChild(xmlDocument.CreateElement("creationDateTime")).InnerText = pickingEndNotice.Date.ToString();
            xmlBody.AppendChild(xmlDocument.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            xmlBody.AppendChild(xmlDocument.CreateElement("documentActionCode")).InnerText = "ADD";

            var xmlIdentification = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationIdentification"));
            xmlIdentification.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = pickingEndNotice.Outgo.Code;// outboundNotification.Id.ToString(); // TODO idk what tu use in entityIdentification

            var xmlServiceBuyer = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesBuyer"));
            xmlServiceBuyer.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlServiceSeller = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesSeller"));
            xmlServiceSeller.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlShippment = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationShipment"));
            var xmlShipper = xmlShippment.AppendChild(xmlDocument.CreateElement("shipper"));
            xmlShipper.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                pickingEndNotice.Shipper.Code;
            var xmlReceiver = xmlShippment.AppendChild(xmlDocument.CreateElement("receiver"));
            xmlReceiver.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                pickingEndNotice.Shipper.Code;

            xmlShippment.AppendChild(xmlDocument.CreateElement("warehousingOutboundStatusCode")).InnerText =
                pickingEndNotice.Outgo.State.Code;

            var lineNumber = 1;
            foreach (var line in pickingEndNotice.Outgo.Lines)
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

        public string ShippingNoticeToXml(PickingEndNotice shippingNoticeToXml)
        {
            if (shippingNoticeToXml == null)
                throw new ArgumentNullException(nameof(shippingNoticeToXml));
            var xmlDocument = new XmlDocument();
            var xmlRoot = xmlDocument.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationMessage"));

            var xmlHeader = xmlRoot.AppendChild(xmlDocument.CreateElement("sh:StandardBusinessDocumentHeader"));
            xmlHeader.AppendChild(xmlDocument.CreateElement("sh:HeaderVersion")).InnerText = "1.0";

            var xmlSender = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Sender"));
            xmlSender.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = shippingNoticeToXml.Sender.Code;

            var xmlRecipient = xmlHeader.AppendChild(xmlDocument.CreateElement("sh:Receiver"));
            xmlRecipient.AppendChild(xmlDocument.CreateElement("sh:Identifier")).InnerText = shippingNoticeToXml.Recipient.Code;

            var xmlBody = xmlRoot.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotification"));
            xmlBody.AppendChild(xmlDocument.CreateElement("creationDateTime")).InnerText = shippingNoticeToXml.Date.ToString();
            xmlBody.AppendChild(xmlDocument.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            xmlBody.AppendChild(xmlDocument.CreateElement("documentActionCode")).InnerText = "ADD";

            var xmlIdentification = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationIdentification"));
            xmlIdentification.AppendChild(xmlDocument.CreateElement("entityIdentification")).InnerText = shippingNoticeToXml.Outgo.Code;// outboundNotification.Id.ToString(); // TODO idk what tu use in entityIdentification

            var xmlServiceBuyer = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesBuyer"));
            xmlServiceBuyer.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlServiceSeller = xmlBody.AppendChild(xmlDocument.CreateElement("logisticServicesSeller"));
            xmlServiceSeller.AppendChild(xmlDocument.CreateElement("gln")).InnerText = "";

            var xmlShippment = xmlBody.AppendChild(xmlDocument.CreateElement("warehousingOutboundNotificationShipment"));
            var xmlShipper = xmlShippment.AppendChild(xmlDocument.CreateElement("shipper"));
            xmlShipper.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                shippingNoticeToXml.Shipper.Code;
            var xmlReceiver = xmlShippment.AppendChild(xmlDocument.CreateElement("receiver"));
            xmlReceiver.AppendChild(xmlDocument.CreateElement("additionalPartyIdentification")).InnerText =
                shippingNoticeToXml.Shipper.Code;

            xmlShippment.AppendChild(xmlDocument.CreateElement("warehousingOutboundStatusCode")).InnerText =
                shippingNoticeToXml.Outgo.State.Code;

            var lineNumber = 1;
            foreach (var line in shippingNoticeToXml.Outgo.Lines)
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