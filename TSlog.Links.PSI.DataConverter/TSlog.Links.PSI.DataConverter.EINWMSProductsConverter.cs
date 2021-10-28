using System;
using System.Collections.Generic;
using TSLog.BO;
using TSLog.Links;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSProductsConverter : EINWMSBaseConverter
    {
        const string NAME = "ART";
        const string POSTFIX = "LTT";
        public override string MessageName()
        {
            return $"{base.MessageName()}.{NAME}";
        }

        /*
        |MsgNam=WMS.ART|VersionsNr=1.9.3|TrxId=Tr100067168|Werk=0000|MndNr=0014|ArtNr=AR00000119|ArtGrpNam=FOOD|ArtBez=Ананас сироп  1,0 ПЭТ|ArtInfo1=|ArtInfo2=|ArtInfo3=|ArtKnzSetDisas=0|ArtKnzWwsMld=1|ArtEan=4607091373967|ArtLng=27|ArtBre=18|ArtHoe=31|ArtVol=14376|ArtAbmEh=CM|ArtVolEh=CM3|ArtGewEh=KG|ArtMgEh=ST|ArtMgEhWWS=ST|ArtGewNet=0|ArtGewBru=1|ArtMhdTageWeDeflt=999|ArtWertVk=0|ArtWwsBeleg=03461f42-aa12-429c-8e26-40e74e|ArtRepNam=03461f42-aa12-429c-8e26-40e74e9be8f1|
        |MsgNam=WMS.ARTLTT|VersionsNr=1.9.3|TrxId=Tr100067168|Werk=0000|MndNr=0014|ArtNr=AR00000119|LttNam=EURO|ArtLttKnzDeflt=1|ArtLttMgMax=999999999|
        |MsgNam=WMS.ARTLTT|VersionsNr=1.9.3|TrxId=Tr100067168|Werk=0000|MndNr=0014|ArtNr=AR00000119|LttNam=STDCRT|ArtLttKnzDeflt=1|ArtLttMgMax=6|
         */
        private string SpecialAssignedTransportUnittypes()
        {
            return "";
        }

        public string Convert(string transactionId, ProductsList list)
        {
            LegalEntity client = list.DataSource;

            string ret = "";
            foreach (var product in list.Products)
            {
                ret += Convert(transactionId, client, product);
            }
            return ret;
        }

        public string Convert(string transactionId, LegalEntity partner, Product product, 
            string warehouseCode = "0000", string defaultTransportUnit = "1" /*EURO*/, bool forceUseProductIdFromBox = false, 
            bool verboseToWWSOnChange = true)
        {
            var common = $"VersionsNr={PrepStrPar(WMSAPIVersion(), true)}" +
                $"|TrxId={PrepStrPar(transactionId)}" +
                $"|Werk={PrepStrPar(warehouseCode)}" +
                $"|MndNr={PrepStrPar(partner.Code)}";

            var ret = "";
            var dictMaxAmountPerPalletType = new Dictionary<string, int>();
            dictMaxAmountPerPalletType.Add("EURO", 999999999);
            dictMaxAmountPerPalletType.Add("STDCRT", 6);

            string defMeasurment = PrepStrPar(TranslateMeasurmentUnit(PrepStrPar(product.UnitType, defaultValue: GetDefaultMeasurmentUnit(partner))), true);

            ret += $"|MsgNam={PrepStrPar(MessageName().ToUpper(), true)}" +
                $"|{common}|ArtNr={PrepStrPar(product.Code)}" +
                $"|ArtGrpNam={PrepStrPar(product.Group, true)}" +
                $"|ArtBez={PrepStrPar(product.Name)}" +
                $"|ArtInfo1={PrepStrPar(TruncateLongString(product.Info1, 200),true)}" +
                $"|ArtInfo2={PrepStrPar(TruncateLongString(product.Info2, 200), true)}" +
                $"|ArtInfo3={PrepStrPar(TruncateLongString(product.Info3, 200), true)}" +
                $"|ArtKnzSetDisas={(forceUseProductIdFromBox ? 1 : 0)}" +
                $"|ArtKnzWwsMld={(verboseToWWSOnChange ? 1 : 0)}" +
                $"|ArtEan={PrepStrPar(product.Gtin)}" +
                $"|ArtLng={PrepStrPar(product.Length)}" +
                $"|ArtBre={PrepStrPar(product.Width)}" +
                $"|ArtHoe={PrepStrPar(product.Heigth)}" +
                $"|ArtVol={PrepStrPar(product.Volume)}" +
                $"|ArtAbmEh={TranslateMeasurmentUnit(PrepStrPar(product.UnitLinerial, defaultValue: "см"))}" +
                $"|ArtVolEh={TranslateMeasurmentUnit(PrepStrPar(product.UnitVolume, defaultValue: "СМ3"))}" +
                $"|ArtGewEh={TranslateMeasurmentUnit(PrepStrPar(product.UnitWeight, defaultValue: "кг"))}" +
                $"|ArtMgEh={defMeasurment}" +
                $"|ArtMgEhWWS={defMeasurment}" +
                $"|ArtGewNet={PrepStrPar(product.Netto)}" +
                $"|ArtGewBru={PrepStrPar(product.Brutto)}" +
                $"|ArtMhdTageWeDeflt={PrepStrPar((product.ExpirationDate - DateTime.Today).Days, 999) }" +
                $"|ArtWertVk={PrepStrPar(product.Price)}" +
                $"|ArtWwsBeleg={PrepStrPar(TruncateLongString(product.Code, 30), true)}" +
                $"|ArtRepNam={PrepStrPar(TruncateLongString(product.Code, 200), true)}" +
                $"|\n";

            foreach (var row in dictMaxAmountPerPalletType)
                ret += $"|MsgNam={PrepStrPar(MessageName().ToUpper(), true)}{PrepStrPar(POSTFIX)}" +
                    $"|{common}" +
                    $"|ArtNr={PrepStrPar(product.Code)}" +
                    $"|LttNam={PrepStrPar(row.Key)}" +
                    $"|ArtLttKnzDeflt={PrepStrPar(defaultTransportUnit, true)}" +
                    $"|ArtLttMgMax={PrepStrPar(row.Value.ToString())}" +
                    $"|\n";

            ret += SpecialAssignedTransportUnittypes();
            return ret;
        }
    }
}
