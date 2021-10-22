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
        public void PostProductsList(ProductsList message, LegalEntity client);
        public ProductsList GetProductsList(LegalEntity client);
    }
    //
    public interface ILinksClientsStorage
    {
        public void PostClientsList(ClientsList message, LegalEntity client);
        public ClientsList GetClientsList(LegalEntity client);
    }
    //
    public interface ILinksInboundsStorage
    {
        public void PostInboundNotification(InboundInstruction message);
        public InboundNotification[] GetInboundResponses();

    }
    //
    public interface ILinksOrdersStorage
    {
        public void PostOrderNotification(OutboundInstruction message);
        public OutboundNotification[] GetOrderResponses();
        public OutboundNotification[] GetPickingStartNotifications();
        public OutboundNotification[] GetPickingFinishNotifications();
        public OutboundNotification[] GetShippingNotifications();
    }

}
