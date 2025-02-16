using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Products.BO;

namespace TSLog.Links;

/// <summary>
/// Общий интерфейс для работы с БД Oracle и Postgresql.
/// </summary>
public interface ILinksStorage : ILinksCommonStorage
{
    public void ConnectOracle(string userName, string password);
    public void SetOracleCommonSchema(string commonSchema);
    public void SetOracleStaffSchema(string staffShcema);
    public void SetOracleLegalEntitiesSchema(string legalEntitiesSchema);
    public void SetOraclePartnersSchema(string partnersSchema);
    public void DisconnectOracle();
    public void ConnectPgSql(string userName, string password);
    public void SetPgsqlYardSchema(string yardSchema);
    public void DisconnectPgsql();
    public void Dispose();
}
public interface ILinksCommonStorage : ILinksProductsStorage, ILinksClientsStorage,
    ILinksIncomesStorage, ILinksOutgoesStorage, ILinksUsers
{ }
    
public interface ILinksProductsStorage : ILinksBase
{
    public Product GetProduct(string id);
    public Product GetProductByGtin(string gtin);
    public Product GetProduct(Partner partner, string partnerProductId);
    public Product[] GetProducts(Partner[] partners = null);
}

public interface ILinksUsers : ILinksClientsStorage
{
    public LegalEntity[] GetAllUsers();
    public LegalEntity Authenticate(AuthenticateRequest model);
}

public interface ILinksClientsStorage : ILinksBase
{
    public Partner GetPartner(string id);
    public Partner[] GetPartners();
    public LegalEntity GetLegalEntity(string id);
    public LegalEntity[] GetLegalEntities(Partner[] partners = null);
    public Partner Tslog { get; }
}

public interface ILinksIncomesStorage : ILinksBase
{
    public Entity GetIncomeState(string stateCode);
    public ReceiptEndNotice GetReceiptEndNotice(string incomeId);
    public ReceiptEndNotice GetReceiptEndNotice(Partner partner, string partnerInboundId);
    public ReceiptEndNotice[] GetReceiptEndNotices(Partner[] partners = null);
}

public interface ILinksOutgoesStorage : ILinksBase
{
    public Entity GetOutgoState(string stateCode);
    public PickingBeginNotice GetPickingBeginNotice(string outgoId);
    public PickingBeginNotice GetPickingBeginNotice(Partner partner, string partnerOutgoId);
    public PickingBeginNotice[] GetPickingBeginNotices(Partner[] partners = null);
    public PickingEndNotice GetPickingEndNotice(string outgoId);
    public PickingEndNotice GetPickingEndNotice(Partner partner, string partnerOutgoId);
    public PickingEndNotice[] GetPickingEndNotices(Partner[] partners = null);
    public ShippingNotice GetShippingNotice(string outgoId);
    public ShippingNotice GetShippingNotice(Partner partner, string partnerOutgoId);
    public ShippingNotice[] GetShippingNotices(Partner[] partners = null);
}

public interface ILinksBase : ILinksGenIdentifier // Pass ILinksGenIdentifier to every part of ILinksStorage.
{
    public Entity GetCurrency(string currencyCode);
    public MeasureUnit GetMeasureUnit(string measureUnitCode);
}

public interface ILinksGenIdentifier
{
    public string GenerateTransactionId();
    public string GenerateObjectId(string objectName, LegalEntity partner);
}
