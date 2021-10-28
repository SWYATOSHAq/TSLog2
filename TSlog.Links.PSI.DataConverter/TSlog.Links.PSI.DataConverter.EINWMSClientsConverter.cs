using System;
using TSLog.BO;
using TSLog.Links;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSClientsConverter : EINWMSBaseConverter
    {
        const string NAME = "GP"; 
        public override string MessageName()
        {
            return  $"{base.MessageName()}.{NAME}";
        }

        /*
         |MsgNam=WMS.GP|VersionsNr=1.9.3|TrxId=Tr100067170|Werk=0000|MndNr=0014|GpNam=fd018462-9d47-11e7-bf0a-681729c65808|GpBez=Сковородников Михаил Витальевич ИП|GpAdr1=...|GpInfo1=Факт. адрес:|GpInfo2=|
         */
        public string Convert(string transactionId, ClientsList list, string warehouseCode = "0000", bool canAddNewLineSymbol = true)
        {
            LegalEntity client = list.Partner;

            string ret = "";
            foreach (var vendor in list.Clients)
            {
                ret += Convert(transactionId, client, vendor, warehouseCode, canAddNewLineSymbol);
            }
            return ret;
        }

        public string Convert(string transactionId, LegalEntity client, LegalEntity vendor, string warehouseCode = "0000", bool canAddNewLineSymbol = true)
        {
            return $"|MsgNam={PrepStrPar(MessageName(), true)}|VersionsNr={PrepStrPar(WMSAPIVersion(), true)}|TrxId={PrepStrPar(transactionId)}|Werk={PrepStrPar(warehouseCode)}|MndNr={PrepStrPar(client.Code)}|GpNam={PrepStrPar((vendor.Code))}|GpBez={PrepStrPar(vendor.Name, true)}|GpAdr1={ PrepStrPar(vendor.Address, true, "...")}|GpInfo1={PrepStrPar(vendor.Info1, true)}|GpInfo2={PrepStrPar(vendor.Info2, true)}|{(canAddNewLineSymbol?'\n':"")}";
        }
    }
}
