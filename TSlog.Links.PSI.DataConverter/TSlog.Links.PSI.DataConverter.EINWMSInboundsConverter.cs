using TSLog.Links;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSInboundsConverter : EINWMSBaseConverter
    {
        private const string NAME = "WEA";
        private const string ROWNAME = "WEP";
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
        |MsgNam=WMS.WEA|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|Aktion=Ins|WeaKlaNam=NORMAL|GpNamLief=VEND_ASKANIA|WeaWwsBeleg=ASK521|WeaDzAnliefSoll=|WeaInfo2=|WeaInfo3=|
        |MsgNam=WMS.WEP|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|WepNr=1|WepMgSoll=2592|ArtNr=AR00000001|BstQlf=10|WepInfo1=27|WepInfo2=796|WepInfo3=|BstChgNam=|BstChgNam2=|BstDzMhd=|WepMgEhWWS=ST|
        |MsgNam=WMS.WEP|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|WepNr=2|WepMgSoll=4320|ArtNr=AR00000002|BstQlf=10|WepInfo1=27|WepInfo2=796|WepInfo3=|BstChgNam=|BstChgNam2=|BstDzMhd=|WepMgEhWWS=ST|
        |MsgNam=WMS.WEAANLABSCHL|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|
        */

        public string Convert(string transactionId, InboundInstruction instruction,
            string warehouseCode = "0000", string action = "Ins", string defaultQualification = "10", int maxInfoSize = 200)
        {
            var partner = instruction.ServiceBuyer;
            var inbound = instruction.Inbound;

            // TODO: read how to serealize fields from external struct (for example from ini file)

            var common = $"VersionsNr={PrepStrPar(WMSAPIVersion(), true)}" +
                $"|TrxId={PrepStrPar(transactionId)}" +
                $"|Werk={PrepStrPar(warehouseCode)}" +
                $"|MndNr={PrepStrPar(partner.Code)}";
            var ret = "";

            // header
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}" +
                $"|{common}" +
                $"|WeaNr={PrepStrPar(inbound.Number)}" +
                $"|Aktion={PrepStrPar(action)}" +
                $"|WeaKlaNam={PrepStrPar(inbound.Type?.Code, true, "NORMAL")}" +
                $"|GpNamLief={PrepStrPar(inbound.Vendor.Code, true)}" +
                $"|WeaWwsBeleg={PrepStrPar(inbound.PartnerNumber, true)}" +
                $"|WeaDzAnliefSoll={/* ? */PrepStrPar(inbound.PlanAcceptanceBegin.ToString("dd.MM.yyyy HH:mm:ss"), true)  /* ? */}" +
                $"|WeaInfo2={PrepStrPar(inbound.GUID, true)}" +
                $"|WeaInfo3={PrepStrPar(inbound.Operation.ToString(), true)}" +
                $"|\n";
            // body
            foreach (var row in inbound.Lines)
                ret += $"|MsgNam={PrepStrPar(MessageRowName(), true)}" +
                    $"|{common}" +
                    $"|WeaNr={PrepStrPar(inbound.Number)}" +
                    $"|WepNr={PrepStrPar(row.Number.ToString())}" +
                    $"|WepMgSoll={PrepStrPar(row.Quantity.ToString())}" +
                    $"|ArtNr={PrepStrPar(row.Product.Code)}" +
                    $"|BstQlf={PrepStrPar(defaultQualification)}" +
                    $"|WepInfo1={PrepStrPar(row.Price.ToString(), true)}" +
                    $"|WepInfo2={PrepStrPar(row.OKEI, true)}" +
                    $"|WepInfo3={PrepStrPar(TruncateLongString(row.Info, maxInfoSize), true)}" +
                    $"|BstChgNam={PrepStrPar(row.BatchNumber, true)}" +
                    $"|BstChgNam2={PrepStrPar(row.BatchNumber2, true)}" +
                    $"|BstDzMhd={PrepStrPar(row.Product.ExpirationDate.ToString("dd.MM.yyyy HH:mm:ss"), true)}" +
                    $"|WepMgEhWWS={PrepStrPar(TranslateMeasurmentUnit(GetDefaultMeasurmentUnit(partner)), true)}" +
                    $"|\n";
            // footer
            ret += $"|MsgNam={PrepStrPar(MessageName(), true)}{PrepStrPar(POSTFIX, true)}" +
                $"|{common}" +
                $"|WeaNr={PrepStrPar(inbound.Number)}" +
                $"|\n";
            return ret;
        }
    }
}