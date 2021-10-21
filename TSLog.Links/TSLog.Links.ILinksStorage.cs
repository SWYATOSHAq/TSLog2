using System;
using TSLog.BO;

namespace TSLog.Links
{
    public interface ILinksStorage : ILinksProductsStorage, ILinksClientsStorage, ILinksInboundsStorage, ILinksOrdersStorage
    {
        //ILinksProductsStorage
        //ILinksClientsStorage 
        //ILinksInboundsStorage 
        //ILinksOrdersStorage
    }

    // split ILinksStorage to several interfaces to cust ILinksStorage to 

    public interface ILinksProductsStorage
    {
        public void PostProductsList(ProductsList message, Partner client);
        public ProductsList GetProductsList(Partner client);
    }
    //
    public interface ILinksClientsStorage
    {
        public void PostClientsList(ClientsList message, Partner client);
        public ClientsList GetClientsList(bool loadPartners = false);
    }
    //
    public interface ILinksInboundsStorage
    {
        public void PostInboundNotification(InboyndNotification message);
        public InboundResponse[] GetInboundResponses();

    }
    //
    public interface ILinksOrdersStorage
    {
        public void PostOrderNotification(OrderNotofocation message);
        public OrderResponse[] GetOrderResponses();
        public PickingStartNotification[] GetPickingStartNotifications();
        public PickingFinishNotification[] GetPickingFinishNotifications();
        public ShippingNotification[] GetShippingNotifications();
    }

}
