using System.Text;
using System.Text.Json;
using TSLog.Links.BLCtobacco.Interfaces;
using TSLog.Links.IntegrationModels;

namespace TSLog.Links.BLCtobacco.Services;

public class DataProcessor : IDataProcessor
{
    public string CreateRecievedGoodsMessage(ReceivedGoodsModel receivedGoodsModel)
    {
        throw new NotImplementedException();
    }

    public ReceivedGoodsModel CreateRecievedGoodsModel(string httpResponce)
    {
        throw new NotImplementedException();
    }

    public ShippedGoodsModel CreateShippedGoodsMessage(string httpResponce)
    {
        throw new NotImplementedException();
    }

    public string CreateShippedGoodsMessage(ShippedGoodsModel shippedGoodsModel)
    {
        throw new NotImplementedException();
    }

    public ShippedGoodsModel CreateShippedGoodsModel(string httpResponce)
    {
        throw new NotImplementedException();
    }

    public string CreateSkuCatalogMessage(List<SkuCatalogModel> skuCatalogModels)
    {
        var sb = new StringBuilder();

        foreach (var model in skuCatalogModels)
        {
            sb.Append("|MsgNam=ART");

            if (!string.IsNullOrEmpty(model.MndNr))
                sb.Append($"|MndNr={model.MndNr}");
            if (!string.IsNullOrEmpty(model.ItemId))
                sb.Append($"|ItemId={model.ItemId}");
            if (!string.IsNullOrEmpty(model.ArtBez))
                sb.Append($"|ArtBez={model.ArtBez}");
            if (!string.IsNullOrEmpty(model.ArtEan))
                sb.Append($"|ArtEan={model.ArtEan}");
            if (!string.IsNullOrEmpty(model.ArtNetWeight))
                sb.Append($"|ArtNetWeight={model.ArtNetWeight}");
            if (!string.IsNullOrEmpty(model.ArtGrossWeight))
                sb.Append($"|ArtGrossWeight={model.ArtGrossWeight}");
            if (!string.IsNullOrEmpty(model.ArtLng))
                sb.Append($"|ArtLng={model.ArtLng}");
            if (!string.IsNullOrEmpty(model.ArtBre))
                sb.Append($"|ArtBre={model.ArtBre}");
            if (!string.IsNullOrEmpty(model.ArtHoe))
                sb.Append($"|ArtHoe={model.ArtHoe}");
            if (!string.IsNullOrEmpty(model.ArtAmbEh))
                sb.Append($"|ArtAmbEh={model.ArtAmbEh}");
            if (!string.IsNullOrEmpty(model.ArtVolEh))
                sb.Append($"|ArtVolEh={model.ArtVolEh}");
            if (!string.IsNullOrEmpty(model.ArtWeiEh))
                sb.Append($"|ArtWeiEh={model.ArtWeiEh}");
            if (!string.IsNullOrEmpty(model.ArtLttMax))
                sb.Append($"|ArtLttMax={model.ArtLttMax}");
            if (!string.IsNullOrEmpty(model.ArtMgEh))
                sb.Append($"|ArtMgEh={model.ArtMgEh}");
            if (!string.IsNullOrEmpty(model.DayOfExpiration))
                sb.Append($"|DayOfExpiration={model.DayOfExpiration}");
            if (!string.IsNullOrEmpty(model.ArtInfo1))
                sb.Append($"|ArtInfo1={model.ArtInfo1}");

            sb.AppendLine("|");
        }

        return sb.ToString();
    }

    public List<SkuCatalogModel> CreateSkuCatalogModel(string httpResponse)
    {
        var skuCatalogModels = new List<SkuCatalogModel>();

        using (JsonDocument doc = JsonDocument.Parse(httpResponse))
        {
            JsonElement root = doc.RootElement;
            JsonElement results = root.GetProperty("d").GetProperty("results");

            foreach (JsonElement element in results.EnumerateArray())
            {
                var sku = element.GetProperty("sku").GetString();
                var skuCatalog = new SkuCatalogModel(msgNam: "ART", mndNr: "0055", itemId: sku, artGrpNam: "BAT", artEan: "1111")
                {
                    ArtBez = "Наименование артикула у поклажедателя",
                    //ArtNetWeight = "",
                    //ArtGrossWeight = "",
                    //ArtLng = "",
                    //ArtBre = "",
                    //ArtHoe = "",
                    //ArtVol = element.GetProperty("position").GetString(),
                    ArtMgEh = element.GetProperty("unitOfMeasure").GetString(), //переспросить!!!
                    //ArtAmbEh = "",
                    //ArtVolEh = "",
                    //ArtWeiEh = "",
                    //ArtLttMax = "",
                    //ArtLttMgPack = "",
                    //DayOfExpiration = "",
                    //ArtPrice = "",
                    //ArtInfo1 = ""
                };

                skuCatalogModels.Add(skuCatalog);
            }
        }

        return skuCatalogModels;
    }

    public TargetShippingClientsModel CreateTargetShippingClientsMessage(string httpResponce)
    {
        throw new NotImplementedException();
    }

    public string CreateTargetShippingClientsModel(TargetShippingClientsModel targetShippingClientsModel)
    {
        throw new NotImplementedException();
    }

    public void ParseResponse(string response)
    {
        throw new NotImplementedException();
    }
}
