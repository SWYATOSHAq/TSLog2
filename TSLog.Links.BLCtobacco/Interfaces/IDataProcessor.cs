using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Links.IntegrationModels;

namespace TSLog.Links.BLCtobacco.Interfaces;

public interface IDataProcessor
{
    public void ParseResponse(string response);
    public SkuCatalogMessage CreateSkuCatalogMessage(string httpResponce);
    public ReceivedGoodsMessage CreateRecievedGoodsMessage(string httpResponce);
    public ShippedGoodsMessage CreateShippedGoodsMessage(string httpResponce);
    public TargetShippingClientsMessage CreateTargetShippingClientsMessage(string httpResponce);
}
