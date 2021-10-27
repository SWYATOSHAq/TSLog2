using System;
using TSLog.BO;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSInboundsConverter : EINWMSBaseConverter
    {
        static string NAME = "WEA";
        static string ROWNAME = "WEP";
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
        |MsgNam=WMS.WEA|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|Aktion=Ins|WeaKlaNam=NORMAL|GpNamLief=VEND_ASKANIA|WeaWwsBeleg=ASK521|WeaDzAnliefSoll=|WeaInfo2=|WeaInfo3=|
        |MsgNam=WMS.WEP|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|WepNr=1|WepMgSoll=2592|ArtNr=AR00000001|BstQlf=10|WepInfo1=27|WepInfo2=796|WepInfo3=|BstChgNam=|BstChgNam2=|BstDzMhd=|WepMgEhWWS=ST|
        |MsgNam=WMS.WEP|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|WepNr=2|WepMgSoll=4320|ArtNr=AR00000002|BstQlf=10|WepInfo1=27|WepInfo2=796|WepInfo3=|BstChgNam=|BstChgNam2=|BstDzMhd=|WepMgEhWWS=ST|
        |MsgNam=WMS.WEAANLABSCHL|VersionsNr=1.9.3|TrxId=Tr100067674|Werk=0000|MndNr=0050|WeaNr=WM00125009|
        */

        public string Convert(string transactionId, LegalEntity client, Inbound inbound, string warehouseCode = "0000", string action = "Ins", string defaultQualification = "10")
        {
            // TODO: read how to serealize fields from external struct (for example from ini file)

            var common = $"VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Id}";
            var ret = "";

            // header
            ret += $"|MsgNam={MessageName().ToUpper()}|{common}|WeaNr={inbound.Id}|Aktion={action}|WeaKlaNam={(inbound.InboundKind == null ? "NORMAL" : inbound.InboundKind)}|GpNamLief={inbound.Vendor.Id}|WeaWwsBeleg={inbound.ClientDocId}|WeaDzAnliefSoll={/* ? */ inbound.PotentialInboundDate.ToString("dd.MM.yyyy HH:mm:ss") /* ? */}|WeaInfo2={inbound.GUID}|WeaInfo3={inbound.Operation.ToString()}|\n";
            // body
            foreach (var row in inbound.Body)
                ret += $"|MsgNam={MessageRowName().ToUpper()}|{common}|WeaNr={inbound.Id}|WepNr={row.Number}|WepMgSoll={row.Quantity}|ArtNr={row.Product.Id}|BstQlf={defaultQualification}|WepInfo1={row.Price}|WepInfo2={row.OKEI}|WepInfo3={row.Info}|BstChgNam={row.BatchNumber}|BstChgNam2={row.BatchNumber2}|BstDzMhd={row.Product.ExpirationDate.ToString("dd.MM.yyyy HH:mm:ss")}|WepMgEhWWS={TranslateMeasurmentUnit(GetDefaultMeasurmentUnit(client))}|\n";
            // footer
            ret += $"|MsgNam={MessageName().ToUpper()}{POSTFIX}|{common}|WeaNr={inbound.Id}|\n";

            return ret;
        }
    }
}
