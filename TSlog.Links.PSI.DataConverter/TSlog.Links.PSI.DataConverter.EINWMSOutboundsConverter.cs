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

        public string Convert(string transactionId, OutgoRequest instruction, string warehouseCode = "0000", string action = "Ins", int deliveryAdderssLenCut = 50, string defaultQualification = "10", int maxInfoSize = 200)
        {
            var partner = instruction.ServiceBuyer;
            var outgo = instruction.Outgo;

            var common = $"VersionsNr={PrepStrPar(WMSAPIVersion(), true)}" +
                $"|TrxId={PrepStrPar(transactionId)}" +
                $"|Werk={PrepStrPar(warehouseCode)}" +
                $"|MndNr={PrepStrPar(partner.Code)}";
            var ret = "";

            // header
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}" +
                $"|{common}" +
                $"|AufNr={PrepStrPar(outgo.Number)}" +
                $"|Aktion={PrepStrPar(action)}" +
                $"|AuaNam={PrepStrPar(outgo.Type?.Code, true, "NORMAL")}" +
                $"|LfaEmpfNam1={PrepStrPar(outgo.Vendor.Code, true)}" +
                $"|AufWwsBeleg={PrepStrPar(outgo.PartnerNumber)}" +
                $"|AufDzVsSoll={PrepStrPar(outgo.PlanShippingBegin, template: "dd.MM.yyyy HH:mm:ss")}" +
                $"|LfaKdOrt1={PrepStrPar(TruncateLongString(outgo.DeliveryAddress1, deliveryAdderssLenCut), true)}" +
                $"|LfaKdOrt2={PrepStrPar(TruncateLongString(outgo.DeliveryAddress1, outgo.DeliveryAddress1?.Length ?? 0, deliveryAdderssLenCut), true) /* cut from deliveryAdderssLenCut to end */}" +
                $"|AufInfo1={PrepStrPar(TruncateLongString(outgo.DeliveryComment, maxInfoSize), true)}" +
                $"|AufInfo2={PrepStrPar(outgo.GUID, true)}" + // <eComStringAttributeValuePairList attributeName="GUID">
                $"|AufInfo3={PrepStrPar(outgo.Operation)}" + // <operationType>
                $"|AufInfo4={PrepStrPar(outgo.DeliveryAddress1, true)}" +
                $"|\n";
            // body
            var defmeasureUnit = TranslateMeasurmentUnit(GetDefaultMeasurmentUnit(partner));

            foreach (var row in outgo.Lines)
                ret += $"|MsgNam={PrepStrPar(MessageRowName(), true)}" +
                    $"|{common}" +
                    $"|AufNr={PrepStrPar(outgo.Number)}" +
                    $"|AupNr={PrepStrPar(row.Number)}" +
                    $"|AupMgSoll={PrepStrPar(row.PlanQuantity)}" +
                    $"|ArtNr={PrepStrPar(row.Product.Code, true)}" +
                    $"|BstQlf={defaultQualification}" +
                    $"|AupInfo1={PrepStrPar(row.QuantityMeasureUnit, defmeasureUnit)}" + // <plannedDespatchQuantity measurementUnitCode="ST">
                    $"|AupInfo2={PrepStrPar(row.OKEI, true)}" + // <eComStringAttributeValuePairList attributeName="OKEI">
                    $"|AupInfo6={PrepStrPar(TruncateLongString(row.Info, maxInfoSize), true)}" + // <note>
                    $"|AupWert={PrepStrPar(row.Product.Price.ToString(), true)}" +
                    $"|BstChgNam={PrepStrPar(row.BatchNumber, true)}" + // <eComStringAttributeValuePairList attributeName="BatchNumber">
                    $"|BstChgNam2={PrepStrPar(row.BatchNumber, true)}" + // <eComStringAttributeValuePairList attributeName="BatchNumber2">
                    $"|BstDzMhd={PrepStrPar(row.Product.ExpirationDate, template: "dd.MM.yyyy")}" +
                    $"|AupMgEhWWS={PrepStrPar(defmeasureUnit, true)}" +
                    $"|\n";
            // footer
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}{POSTFIX}" +
                $"|{common}" +
                $"|AufNr={PrepStrPar(outgo.Number)}" +
                $"|\n";

            return ret;
        }
    }
}