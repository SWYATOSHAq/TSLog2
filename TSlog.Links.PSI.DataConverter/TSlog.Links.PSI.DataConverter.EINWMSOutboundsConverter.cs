using TSLog.BO;
using TSLog.Links;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSOutboundsConverter : EINWMSBaseConverter
    {
        const string NAME = "AUF";
        const string ROWNAME = "AUP";
        const string POSTFIX = "ANLABSCHL";

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


        public string Convert(string transactionId, OutboundInstruction instruction, string warehouseCode = "0000", string action = "Ins", int deliveryAdderssLenCut = 50, string defaultQualification = "10", int maxInfoSize = 200)
        {
            var client = instruction.ServiceBuyer;
            var outbound = instruction.Outbound;

            var common = $"VersionsNr={PrepStrPar(WMSAPIVersion(), true)}|TrxId={PrepStrPar(transactionId)}|Werk={PrepStrPar(warehouseCode)}|MndNr={PrepStrPar(client.Code)}";
            var ret = "";

            // header
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}|{common}|AufNr={PrepStrPar(outbound.PartnerNumer)}|Aktion={PrepStrPar(action)}|AuaNam={PrepStrPar(outbound.Type.Code, true,"NORMAL")}|LfaEmpfNam1={PrepStrPar(outbound.Vendor.Code, true)}|AufWwsBeleg=ַ{PrepStrPar(outbound.PartnerNumer, true)}|AufDzVsSoll={PrepStrPar(outbound.PlanShippingBegin.ToString("dd.MM.yyyy HH:mm:ss"), true)}|LfaKdOrt1={PrepStrPar(TruncateLongString(outbound.DeliveryAddress1, deliveryAdderssLenCut), true)}|LfaKdOrt2={PrepStrPar(TruncateLongString(outbound.DeliveryAddress1, outbound.DeliveryAddress1.Length, deliveryAdderssLenCut), true) /* cut from deliveryAdderssLenCut to end */}|AufInfo1={PrepStrPar(TruncateLongString(outbound.DeliveryComment, maxInfoSize), true)}|AufInfo2={PrepStrPar(outbound.GUID, true)}|AufInfo3={PrepStrPar(outbound.Operation.ToString(), true)}|AufInfo4={PrepStrPar(outbound.DeliveryAddress1, true)}|\n";
            // body
            foreach (var row in outbound.Lines)
                ret += $"|MsgNam={PrepStrPar(MessageRowName(), true)}|{common}|AufNr={PrepStrPar(outbound.PartnerNumer)}|AupNr={PrepStrPar(row.Number.ToString())}|AupMgSoll={PrepStrPar(row.Quantity.ToString(), true)}|ArtNr={PrepStrPar(row.Product.Code, true)}|BstQlf={defaultQualification}|AupInfo1={PrepStrPar(row.MeasureUnit.Code, true)}|AupInfo2={PrepStrPar(row.OKEI, true)}|AupInfo6={PrepStrPar(TruncateLongString(row.Info, maxInfoSize), true)}|AupWert={PrepStrPar(row.Product.Price.ToString(), true)}|BstChgNam={PrepStrPar(row.BatchNumber, true)}|BstChgNam2={PrepStrPar(row.BatchNumber, true)}|BstDzMhd={PrepStrPar(row.Product.ExpirationDate.ToString("dd.MM.yyyy"), true)}|AupMgEhWWS={PrepStrPar(TranslateMeasurmentUnit(GetDefaultMeasurmentUnit(client)), true)}|\n";
            // footer
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}{POSTFIX}|{common}|AufNr={PrepStrPar(outbound.PartnerNumer)}|\n";

            return ret;
        }

    }
}
