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
        public string Convert(string transactionId, LegalEntity client, LegalEntity vendor, string warehouseCode = "0000")
        {
            return $"|MsgNam={MessageName().ToUpper()}|VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Id}|GpNam={vendor.Id}|GpBez={vendor.Name}|GpAdr1={(vendor.Zip == null ? "...": vendor.Zip)}|GpInfo1={(vendor.Address == null ? "" : vendor.Address)}|GpInfo2={(vendor.Info2 == null ? "" : vendor.Info2)}|\n";
        }
    }
}
