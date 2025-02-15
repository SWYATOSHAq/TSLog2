using TSLog.Links.IntegrationModels;

namespace TSLog.Links.BLCtobacco.Interfaces;

public interface IDataProcessor
{
    public void ParseResponse(string response);
    public List<SkuCatalogModel> CreateSkuCatalogModel(string httpResponse);
    public ReceivedGoodsModel CreateRecievedGoodsModel(string httpResponse);
    public ShippedGoodsModel CreateShippedGoodsModel(string httpResponse);
    public TargetShippingClientsModel CreateTargetShippingClientsMessage(string httpResponcse);
    public string CreateSkuCatalogMessage(List<SkuCatalogModel> skuCatalogModels);
    public string CreateRecievedGoodsMessage(ReceivedGoodsModel receivedGoodsModel);    
    public string CreateShippedGoodsMessage(ShippedGoodsModel shippedGoodsModel);    
    public string CreateTargetShippingClientsModel(TargetShippingClientsModel targetShippingClientsModel);
}
