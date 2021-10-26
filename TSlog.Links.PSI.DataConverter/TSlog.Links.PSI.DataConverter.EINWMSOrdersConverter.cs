using System;
using TSLog.BO;

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
        public string Convert(string transactionId, LegalEntity client, Order order, string warehouseCode = "0000")
        {
            var ret = "";

            // header
            ret += $"|MsgNam={MessageName().ToUpper()}|VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Id}|AufNr={order.ClientDocId}|Aktion=Ins|AuaNam=VKZH_HOR|LfaEmpfNam1={order.GUID}|AufWwsBeleg=ַ-ֲֶֽװ-008569|AufDzVsSoll=18.10.2021 0:00:00|LfaKdOrt1=|LfaKdOrt2=|AufInfo1=|AufInfo2=6b35df4f-2c21-11ec-80f4-00505691c78c|AufInfo3=187|AufInfo4=|\n";
            // body
            foreach (var row in order.Body)
                ret += $"|MsgNam={MessageRowName().ToUpper()}|VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Id}|AufNr={order.ClientDocId}|AupNr=2|AupMgSoll=1,00|ArtNr=AR00000062|BstQlf=10|AupInfo1=רע|AupInfo2=796|AupInfo6=|AupWert=240,00|BstChgNam=|BstChgNam2=|BstDzMhd=|AupMgEhWWS=ST|\n";
            // footer
            ret += $"|MsgNam={MessageName().ToUpper()}{POSTFIX}|VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Id}|AufNr={order.ClientDocId}|\n";

            return ret;
        }
    }
}
