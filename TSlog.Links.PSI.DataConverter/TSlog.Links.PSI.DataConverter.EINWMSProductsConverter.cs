using System;
using System.Collections.Generic;
using TSLog.BO;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSProductsConverter : EINWMSBaseConverter
    {
        static string NAME = "ART";
        static string POSTFIX = "LTT";
        public override string MessageName()
        {
            return $"{base.MessageName()}.{NAME}";
        }

        /*
        |MsgNam=WMS.ART|VersionsNr=1.9.3|TrxId=Tr100067168|Werk=0000|MndNr=0014|ArtNr=AR00000119|ArtGrpNam=FOOD|ArtBez=Ананас сироп  1,0 ПЭТ|ArtInfo1=|ArtInfo2=|ArtInfo3=|ArtKnzSetDisas=0|ArtKnzWwsMld=1|ArtEan=4607091373967|ArtLng=27|ArtBre=18|ArtHoe=31|ArtVol=14376|ArtAbmEh=CM|ArtVolEh=CM3|ArtGewEh=KG|ArtMgEh=ST|ArtMgEhWWS=ST|ArtGewNet=0|ArtGewBru=1|ArtMhdTageWeDeflt=999|ArtWertVk=0|ArtWwsBeleg=03461f42-aa12-429c-8e26-40e74e|ArtRepNam=03461f42-aa12-429c-8e26-40e74e9be8f1|
        |MsgNam=WMS.ARTLTT|VersionsNr=1.9.3|TrxId=Tr100067168|Werk=0000|MndNr=0014|ArtNr=AR00000119|LttNam=EURO|ArtLttKnzDeflt=1|ArtLttMgMax=999999999|
        |MsgNam=WMS.ARTLTT|VersionsNr=1.9.3|TrxId=Tr100067168|Werk=0000|MndNr=0014|ArtNr=AR00000119|LttNam=STDCRT|ArtLttKnzDeflt=1|ArtLttMgMax=6|
         */
        public string Convert(string transactionId, LegalEntity client, Product product, string warehouseCode = "0000", string defaultTransportUnit = "1" /*EURO*/)
        {
            var ret = "";
            var dictMaxAmountPerPalletType = new Dictionary<string, int>();
            dictMaxAmountPerPalletType.Add("EURO", 999999999);
            dictMaxAmountPerPalletType.Add("STDCRT", 6);

            ret += $"|MsgNam={MessageName().ToUpper()}|VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Code}|ArtNr={product.Code}|ArtGrpNam={product.Group}|ArtBez={product.Name}|ArtInfo1={product.Info1}|ArtInfo2={product.Info2}|ArtInfo3={product.Info3}|ArtKnzSetDisas=0|ArtKnzWwsMld=1|ArtEan={product.Gtin}|ArtLng={product.Length}|ArtBre={product.Width}|ArtHoe={product.Heigth}|ArtVol={product.Volume}|ArtAbmEh={product.UnitLinerial}|ArtVolEh={product.UnitVolume}|ArtGewEh={product.UnitWeight}|ArtMgEh={product.UnitType}|ArtMgEhWWS=ST|ArtGewNet=0|ArtGewBru=1|ArtMhdTageWeDeflt=999|ArtWertVk=0|ArtWwsBeleg=03461f42-aa12-429c-8e26-40e74e|ArtRepNam=03461f42-aa12-429c-8e26-40e74e9be8f1|\n";

            foreach (var row in dictMaxAmountPerPalletType)
                ret += $"|MsgNam={MessageName().ToUpper()}{POSTFIX}|VersionsNr={WMSAPIVersion().ToUpper()}|TrxId={transactionId}|Werk={warehouseCode}|MndNr={client.Id}|ArtNr={product.Code}|LttNam={row.Key}|ArtLttKnzDeflt={defaultTransportUnit}|ArtLttMgMax={row.Value}|\n";

            return ret;
        }
    }
}
