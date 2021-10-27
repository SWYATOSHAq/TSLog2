using System;
using TSLog.BO;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSClientsConverter : EINWMSBaseConverter
    {
        static string NAME = "GP"; 
        public override string MessageName()
        {
            return  $"{base.MessageName()}.{NAME}";
        }

        /*
         |MsgNam=WMS.GP|VersionsNr=1.9.3|TrxId=Tr100067170|Werk=0000|MndNr=0014|GpNam=fd018462-9d47-11e7-bf0a-681729c65808|GpBez=Сковородников Михаил Витальевич ИП|GpAdr1=...|GpInfo1=Факт. адрес:|GpInfo2=|
         */
        public string Convert(string transactionId, LegalEntity client, LegalEntity vendor, string warehouseCode = "0000", bool canAddNewLineSymbol = true)
        {
            return $"|MsgNam={PrepStrPar(MessageName(), true)}|VersionsNr={PrepStrPar(WMSAPIVersion(), true)}|TrxId={PrepStrPar(transactionId)}|Werk={PrepStrPar(warehouseCode)}|MndNr={PrepStrPar(client.Code)}|GpNam={PrepStrPar((vendor.Code))}|GpBez={vendor.Name}|GpAdr1={(vendor.Address == null ? "...": vendor.Address)}|GpInfo1={(vendor.Info1 == null ? "" : vendor.Info1)}|GpInfo2={(vendor.Info2 == null ? "" : vendor.Info2)}|{(canAddNewLineSymbol?'\n':"")}";
        }
    }
}
