using TSLog.Links;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSOutboundsConverter : EINWMSBaseConverter
    {
        private const string NAME = "AUF";
        private const string ROWNAME = "AUP";
        private const string POSTFIX = "ANLABSCHL";

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
        |MsgNam=WMS.AUP|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|AupNr=1|AupMgSoll=1|ArtNr=A0002N0202509|BstQlf=10|AupInfo1=ST|AupInfo2=|AupInfo6={OrderNum=99575939}{OrderBarCode=!vyzeoYGR}{OrderCode=407358 1080}|AupWert=391|BstChgNam=|BstChgNam2=|BstDzMhd=|AupMgEhWWS=ST|
        |MsgNam=WMS.AUP|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|AupNr=2|AupMgSoll=1|ArtNr=AR00172722|BstQlf=10|AupInfo1=ST|AupInfo2=|AupInfo6={OrderNum=99583046}{OrderBarCode=!vyzsAzch}{OrderCode=407363 5891}|AupWert=306|BstChgNam=|BstChgNam2=|BstDzMhd=|AupMgEhWWS=ST|
        |MsgNam=WMS.AUFANLABSCHL|VersionsNr=1.9.3|TrxId=Tr100067139|Werk=0000|MndNr=0002|AufNr=WM00022398|
        */

        public string Convert(string transactionId, OutboundInstruction instruction, string warehouseCode = "0000", string action = "Ins", int deliveryAdderssLenCut = 50, string defaultQualification = "10", int maxInfoSize = 200)
        {
            var partner = instruction.ServiceBuyer;
            var outbound = instruction.Outbound;

            var common = $"VersionsNr={PrepStrPar(WMSAPIVersion(), true)}" +
                $"|TrxId={PrepStrPar(transactionId)}" +
                $"|Werk={PrepStrPar(warehouseCode)}" +
                $"|MndNr={PrepStrPar(partner.Code)}";
            var ret = "";

            // header
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}" +
                $"|{common}" +
                $"|AufNr={PrepStrPar(outbound.Number)}" +
                $"|Aktion={PrepStrPar(action)}" +
                $"|AuaNam={PrepStrPar(outbound.Type?.Code, true, "NORMAL")}" +
                $"|LfaEmpfNam1={PrepStrPar(outbound.Vendor.Code, true)}" +
                $"|AufWwsBeleg={PrepStrPar(outbound.PartnerNumer)}" +
                $"|AufDzVsSoll={PrepStrPar(outbound.PlanShippingBegin, template: "dd.MM.yyyy HH:mm:ss")}" +
                $"|LfaKdOrt1={PrepStrPar(TruncateLongString(outbound.DeliveryAddress1, deliveryAdderssLenCut), true)}" +
                $"|LfaKdOrt2={PrepStrPar(TruncateLongString(outbound.DeliveryAddress1, outbound.DeliveryAddress1?.Length ?? 0, deliveryAdderssLenCut), true) /* cut from deliveryAdderssLenCut to end */}" +
                $"|AufInfo1={PrepStrPar(TruncateLongString(outbound.DeliveryComment, maxInfoSize), true)}" +
                $"|AufInfo2={PrepStrPar(outbound.GUID, true)}" +
                $"|AufInfo3={PrepStrPar(outbound.Operation)}" + // ?
                $"|AufInfo4={PrepStrPar(outbound.DeliveryAddress1, true)}" +
                $"|\n";
            // body
            foreach (var row in outbound.Lines)
                ret += $"|MsgNam={PrepStrPar(MessageRowName(), true)}" +
                    $"|{common}" +
                    $"|AufNr={PrepStrPar(outbound.Number)}" +
                    $"|AupNr={PrepStrPar(row.Number)}" +
                    $"|AupMgSoll={PrepStrPar(row.PlanQuantity)}" +
                    $"|ArtNr={PrepStrPar(row.Product.Code, true)}" +
                    $"|BstQlf={defaultQualification}" +
                    $"|AupInfo1={PrepStrPar(row.MeasureUnit.Code, true)}" + // ?
                    $"|AupInfo2={PrepStrPar(row.OKEI, true)}" + // ?
                    $"|AupInfo6={PrepStrPar(TruncateLongString(row.Info, maxInfoSize), true)}" + // ?
                    $"|AupWert={PrepStrPar(row.Product.Price.ToString(), true)}" +
                    $"|BstChgNam={PrepStrPar(row.BatchNumber, true)}" + // ?
                    $"|BstChgNam2={PrepStrPar(row.BatchNumber, true)}" + // ?
                    $"|BstDzMhd={PrepStrPar(row.Product.ExpirationDate, template: "dd.MM.yyyy")}" + // ?  
                    $"|AupMgEhWWS={PrepStrPar(TranslateMeasurmentUnit(GetDefaultMeasurmentUnit(partner)), true)}" +
                    $"|\n";
            // footer
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}{POSTFIX}" +
                $"|{common}" +
                $"|AufNr={PrepStrPar(outbound.Number)}" +
                $"|\n";

            return ret;
        }
    }
}