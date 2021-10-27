using System;
using TSLog.BO;
using TSLog.Links;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSOrdersConverter : EINWMSBaseConverter
    {
        static string NAME = "AUF";
        static string ROWNAME = "AUP";
        static string POSTFIX = "ANLABSCHL";

        public override string MessageName()
        {
            return $"{base.MessageName()}.{NAME}";
        }
        public string MessageRowName()
        {
            return $"{base.MessageName()}.{ROWNAME}";
        }
        /*
        |MsgNam=WMS.AUF|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|Aktion=Ins|AuaNam=NORMAL|LfaEmpfNam1=WB|AufWwsBeleg=WB15102021125741|AufDzVsSoll=16.10.2021 00:00:00|LfaKdOrt1=|LfaKdOrt2=|AufInfo1=|AufInfo2=|AufInfo3=128|AufInfo4=|
        |MsgNam=WMS.AUP|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|AupNr=2|AupMgSoll=1|ArtNr=A0002N0202509|BstQlf=10|AupInfo1=ST|AupInfo2=|AupInfo6={OrderNum=99575939}{OrderBarCode=!vyzeoYGR}{OrderCode=407358 1080}|AupWert=391|BstChgNam=|BstChgNam2=|BstDzMhd=|AupMgEhWWS=ST|
        |MsgNam=WMS.AUP|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|AupNr=9|AupMgSoll=1|ArtNr=AR00172722|BstQlf=10|AupInfo1=ST|AupInfo2=|AupInfo6={OrderNum=99583046}{OrderBarCode=!vyzsAzch}{OrderCode=407363 5891}|AupWert=306|BstChgNam=|BstChgNam2=|BstDzMhd=|AupMgEhWWS=ST|
        |MsgNam=WMS.AUFANLABSCHL|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|
        */

        static string TruncateLongString(string str, int maxLength, int start = 0)
        {
            if (string.IsNullOrEmpty(str)) return str;

            return str.Substring(start, Math.Min(str.Length, maxLength));
        }

        public string Convert(string transactionId, OutboundInstruction instruction, string warehouseCode = "0000", string action = "Ins", int deliveryAdderssLenCut = 50, string defaultQualification = "10", int maxInfoSize = 200)
        {
            LegalEntity client = instruction.ServiceBuyer;
            Order order = instruction.Order;

            var common = $"VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Code}";
            var ret = "";

            // header
            ret += $"|MsgNam={MessageName().ToUpper()}|{common}|AufNr={order.ClientDocId}|Aktion={action}|AuaNam={(order.OrderKind == null ? "NORMAL" : order.OrderKind)}|LfaEmpfNam1={order.Vendor.Code}|AufWwsBeleg=ַ{order.ClientDocId}|AufDzVsSoll={order.ShipDate.ToString("dd.MM.yyyy HH:mm:ss")}|LfaKdOrt1={TruncateLongString(order.DeliveryAddress1, deliveryAdderssLenCut)}|LfaKdOrt2={TruncateLongString(order.DeliveryAddress1, order.DeliveryAddress1.Length, deliveryAdderssLenCut) /* cut from deliveryAdderssLenCut to end */}|AufInfo1={order.DeliveryComment}|AufInfo2={order.GUID}|AufInfo3={order.Operation}|AufInfo4={order.DeliveryAddress1}|\n";
            // body
            foreach (var row in order.Body)
                ret += $"|MsgNam={MessageRowName().ToUpper()}|{common}|AufNr={order.ClientDocId}|AupNr={row.Number}|AupMgSoll={row.Quantity}|ArtNr={row.Product.Code}|BstQlf={defaultQualification}|AupInfo1={row.Unit}|AupInfo2={row.OKEI}|AupInfo6={TruncateLongString(row.Info, 200)}|AupWert={row.Product.Price}|BstChgNam={row.Product.BatchNumber}|BstChgNam2={row.Product.BatchNumber}|BstDzMhd={row.Product.ExpirationDate.ToString("dd.MM.yyyy")}|AupMgEhWWS={TranslateMeasurmentUnit(GetDefaultMeasurmentUnit(client))}|\n";
            // footer
            ret += $"|MsgNam={MessageName().ToUpper()}{POSTFIX}|{common}|AufNr={order.ClientDocId}|\n";

            return ret;
        }

    }
}
