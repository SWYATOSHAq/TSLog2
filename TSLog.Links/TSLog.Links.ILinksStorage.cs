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
        public Entity GetIncomeState(string stateCode);
        public ReceiptEndNotice GetReceiptEndNotice(string incomeId);

        public ReceiptEndNotice GetReceiptEndNotice(LegalEntity partner, string partnerInboundId);

        public ReceiptEndNotice[] GetReceiptEndNoticies(LegalEntity[] partners = null);        
    }

    public interface ILinksOutboundsStorage : ILinksBase
    {
        public Entity GetOutgoState(string stateCode);
        public PickingBeginNotice GetPickingBeginNotice(string outgoId);

        public PickingBeginNotice GetPickingBeginNotice(LegalEntity partner, string partnerOutgoId);

        public PickingBeginNotice[] GetPickingBeginNotices(LegalEntity[] partners = null, Entity[] states = null);


        public PickingEndNotice GetPickingEndNotice(string outgoId);

        public PickingEndNotice GetPickingEndNotice(LegalEntity partner, string partnerOutgoId);

        public PickingEndNotice[] GetPickingEndNotices(LegalEntity[] partners = null, Entity[] states = null);


        public ShippingNotice GetShippingNotice(string outgoId);

        public ShippingNotice GetShippingNotice(LegalEntity partner, string partnerOutgoId);

        public ShippingNotice[] GetShippingNotices(LegalEntity[] partners = null, Entity[] states = null);
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