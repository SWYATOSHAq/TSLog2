using TSLog.BO;

namespace TSLog.Links
{
    public interface ILinksStorage : ILinksProductsStorage, ILinksClientsStorage, ILinksIncomesStorage, ILinksOutgoesStorage
    {
    }

    public interface ILinksProductsStorage : ILinksBase
    {
        public Product GetProduct(string id);

        public Product GetProductByGtin(string gtin);

        public Product GetProduct(LegalEntity partner, string partnerProductId);

        public Product[] GetProducts(LegalEntity[] partners = null);
    }

    public interface ILinksClientsStorage : ILinksBase
    {
        public LegalEntity GetPartner(string id);

        public LegalEntity[] GetPartners();       

        public LegalEntity GetLegalEntity(string id);

        public LegalEntity[] GetLegalEntities(LegalEntity[] partners = null);

        public LegalEntity Tslog { get; }
    }

    public interface ILinksIncomesStorage : ILinksBase
    {
        public Entity GetIncomeState(string stateCode);

        public ReceiptEndNotice GetReceiptEndNotice(string incomeId);

        public ReceiptEndNotice GetReceiptEndNotice(LegalEntity partner, string partnerInboundId);

        public ReceiptEndNotice[] GetReceiptEndNotices(LegalEntity[] partners = null);
    }

    public interface ILinksOutgoesStorage : ILinksBase
    {
        public Entity GetOutgoState(string stateCode);

        public PickingBeginNotice GetPickingBeginNotice(string outgoId);

        public PickingBeginNotice GetPickingBeginNotice(LegalEntity partner, string partnerOutgoId);

        public PickingBeginNotice[] GetPickingBeginNotices(LegalEntity[] partners = null);

        public PickingEndNotice GetPickingEndNotice(string outgoId);

        public PickingEndNotice GetPickingEndNotice(LegalEntity partner, string partnerOutgoId);

        public PickingEndNotice[] GetPickingEndNotices(LegalEntity[] partners = null);

        public ShippingNotice GetShippingNotice(string outgoId);

        public ShippingNotice GetShippingNotice(LegalEntity partner, string partnerOutgoId);

        public ShippingNotice[] GetShippingNotices(LegalEntity[] partners = null);
    }

    public interface ILinksBase : ILinksGenIdentifier // pass ILinksGenIdentifier to every part of ILinksStorage
    {
        public Entity GetCurrency(string currencyCode);

        public MeasureUnit GetMeasureUnit(string measureUnitCode);
    }

    public interface ILinksGenIdentifier
    {
        public string GenerateTransactionId();

        public string GenerateObjectId(string objectName, LegalEntity partner);
    }
}