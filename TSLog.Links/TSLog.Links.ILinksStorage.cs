using TSLog.BO;

namespace TSLog.Links
{
    public interface ILinksStorage : ILinksProductsStorage, ILinksClientsStorage, ILinksInboundsStorage, ILinksOutboundsStorage
    {

    }

    // Split ILinksStorage to several interfaces to cust ILinksStorage to
    public interface ILinksProductsStorage : ILinksBase
    {
        public ProductsList GetProductsList(LegalEntity client);
        public Product GetProduct(string code);
    }

    public interface ILinksClientsStorage : ILinksBase
    {
        public ClientsList GetClientsList(bool loadPartners = false);

        public LegalEntity GetPartner(string code);


        public LegalEntity GetLegalEntity(string partnerCode);

        public LegalEntity[] GetPartners();

        public LegalEntity Tslog { get; }
    }

    public interface ILinksInboundsStorage : ILinksBase
    {
        public InboundNotification GetInboundNotification(string inboundId);

        public InboundNotification GetInboundNotification(LegalEntity partner, string partnerInboundId);

        public InboundNotification[] GetInboundNotifications(LegalEntity[] partners = null);
        public Entity GetInboundState(string stateCode);
    }

    public interface ILinksOutboundsStorage : ILinksBase
    {
        public OutboundNotification GetOutboundNotification(string outboundId);

        public OutboundNotification GetOutboundNotification(LegalEntity partner, string partnerOutboundId);

        public OutboundNotification[] GetOutboundNotifications(LegalEntity[] partners = null, Entity[] states = null);

        public OutboundNotification[] GetOrderResponses();

        public OutboundNotification[] GetPickingStartNotifications();

        public OutboundNotification[] GetPickingFinishNotifications();

        public OutboundNotification[] GetShippingNotifications();
        public Entity GetOutboundState(string stateCode);
    }

    public interface ILinksBase : ILinksGenIdentifier // pass ILinksGenIdentifier to every part of ILinksStorage
    {
        public Entity GetCurrency(string currencyCode);

        public MeasureUnit GetMeasureUnit(string measureUnitCode);

    }

    public interface ILinksGenIdentifier
    {
        public string GenerateTransactionId();
        public string GenerateObjectId(string ObjectName, LegalEntity partner);
    }
}