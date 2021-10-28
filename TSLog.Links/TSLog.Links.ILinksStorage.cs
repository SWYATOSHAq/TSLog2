using TSLog.BO;

namespace TSLog.Links
{
    public interface ILinksStorage : ILinksProductsStorage, ILinksClientsStorage, ILinksInboundsStorage, ILinksOutboundsStorage
    {
        // Lite colkhozing))
        public Entity GetInboundState(string stateCode);
        public Entity GetOutboundState(string stateCode);
        public Entity GetCurrency(string currencyCode);
        public MeasureUnit GetMeasureUnit(string measureUnitCode);
        public LegalEntity GetPartner(string code);
        public LegalEntity[] GetPartners();
        public Product GetProduct(string code);
    }

    // Split ILinksStorage to several interfaces to cust ILinksStorage to 
    public interface ILinksProductsStorage
    {
        public ProductsList GetProductsList(LegalEntity client);
    }
    public interface ILinksClientsStorage
    {
        public ClientsList GetClientsList(bool loadPartners = false);
        public LegalEntity Tslog { get; }
    }
    public interface ILinksInboundsStorage
    {
        public InboundNotification GetInboundNotification(string inboundId);
        public InboundNotification GetInboundNotification(LegalEntity partner, string partnerInboundId);
        public InboundNotification[] GetInboundNotifications(LegalEntity[] partners = null);
    }
    public interface ILinksOutboundsStorage
    {
        public OutboundNotification GetOutboundNotification(string outboundId);
        public OutboundNotification GetOutboundNotification(LegalEntity partner, string partnerOutboundId);
        public OutboundNotification[] GetOutboundNotifications(LegalEntity[] partners = null, Entity[] states = null);
        public OutboundNotification[] GetOrderResponses();
        public OutboundNotification[] GetPickingStartNotifications();
        public OutboundNotification[] GetPickingFinishNotifications();
        public OutboundNotification[] GetShippingNotifications();
    }
}
