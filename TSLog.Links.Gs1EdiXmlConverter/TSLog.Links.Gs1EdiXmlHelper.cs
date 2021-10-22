using System;
using System.Collections.Generic;
using System.Xml;
using TSLog.BO;

namespace TSLog.Links.Gs1EdiXmlConverter
{
    public static class Gs1EdiXmlHelper
    {
        public static Catalog<LegalEntity> LegalEntitites { get; set; }
        public static Catalog<Product> Products { get; set; }

        private static LegalEntity LegalEntityFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var code = string.Empty;
            var closeTag = reader.Name;
            while (reader.Read())
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
                        var legalEntity = LegalEntitites[code];
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
        private static LogisticUnit LogisticUnitFromXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var closeTag = reader.Name;
            var code = string.Empty;
            var typeCode = string.Empty;
            while (reader.Read())
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

        #region Clients
        public static string ToGs1ExiXml(this ClientsList clientsList)
        {
            return string.Empty;
        }
        #endregion

        #region Products
        public static string ToGs1ExiXml(this ProductsList productsList)
        {
            return string.Empty;
        }
        #endregion

        #region Inbound

        private static InboundInstruction.Line InboundInstructionLineFromXml(XmlReader reader)
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
            while (reader.Read())
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
                    if (reader.Name == "closeTag")
                    {
                        var product = Products[gtin];
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
        public static InboundInstruction InboundInstructionFromXml(XmlReader reader)
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

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "creationDateTime")
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

        public static string ToGs1ExiXml(this InboundNotification inboundResponse)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement("warehousingInboundNotificationMessage"));

            XmlNode docHeader = root.AppendChild(doc.CreateElement("sh:StandardBusinessDocumentHeader"));

            XmlNode body = root.AppendChild(doc.CreateElement("warehousingInboundNotification"));

            body.AppendChild(doc.CreateElement("creationDateTime")).InnerText = "";
            body.AppendChild(doc.CreateElement("documentStatusCode")).InnerText = "ORIGINAL";
            body.AppendChild(doc.CreateElement("documentActionCode")).InnerText = "ADD";

            XmlNode ident = body.AppendChild(doc.CreateElement("warehousingInboundNotificationIdentification"));
            ident.AppendChild(doc.CreateElement("entityIdentification")).InnerText = inboundResponse.Id.ToString();

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

        #region Outbound
        public static OutboundInstruction ToGs1ExiXml(XmlReader reader)
        {
            return null;
        }

        public static string ToGs1ExiXml(this OutboundNotification orderResponse)
        {
            return string.Empty;
        }

        #endregion
    }
}
