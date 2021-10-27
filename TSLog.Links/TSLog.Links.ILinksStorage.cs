using TSLog.BO;

namespace TSLog.Links
{
    public interface ILinksStorage : ILinksProductsStorage, ILinksClientsStorage, ILinksInboundsStorage, ILinksOrdersStorage
    {
        // Lite colkhozing))
        public LegalEntity GetPartner(string code);
        public LegalEntity[] GetPartners();
        public Product GetProduct(string code);
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
        public ClientsList GetClientsList(bool loadPartners = false);

        public LegalEntity Tslog { get; }
    }
    //
    public interface ILinksInboundsStorage
    {
        public void PostInboundInstruction(InboundInstruction inboundInstruction);
        public InboundNotification GetInboundNotification(string inboundId);
        public InboundNotification GetInboundNotification(LegalEntity partner, string partnerInboundId);
        public InboundNotification[] GetInboundNotifications(LegalEntity[] partners = null);

    }
    //
    public interface ILinksOrdersStorage
    {
        public void PostOutboundInstruction(OutboundInstruction outboundInstruction);
        public OutboundNotification GetOutboundNotification(string outboundId);
        public OutboundNotification GetOutboundNotification(LegalEntity partner, string partnerOutboundId);
        public OutboundNotification[] GetOutboundNotifications(LegalEntity[] partners = null, Entity[] states = null);
        public OutboundNotification[] GetOrderResponses();
        public OutboundNotification[] GetPickingStartNotifications();
        public OutboundNotification[] GetPickingFinishNotifications();
        public OutboundNotification[] GetShippingNotifications();
    }

}
