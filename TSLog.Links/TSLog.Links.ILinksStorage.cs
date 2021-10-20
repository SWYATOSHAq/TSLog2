using System;

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
        public void PostProductsList(ProductsList message);
    }
    //
    public interface ILinksClientsStorage
    {
        public void PostClientsList(ClientsList message);
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
