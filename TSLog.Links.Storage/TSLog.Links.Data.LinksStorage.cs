using Microsoft.Extensions.Logging;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Links.Data;
using TSLog.Partners.BO;
using TSLog.Products.BO;
using TSLog.Yard.BO;

namespace TSLog.Links.Storage;

public class LinksStorage : ILinksStorage, IDisposable
{
    private const int DefaultMaxRowsCount = 16;
    private readonly LinksOracleStorage _oracleStorage;
    private readonly LinksPgSqlStorage _pgsqlStorage;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Максимальное количество строк для вывода.
    /// </summary>
    public int MaxRowsCount
    {
        set
        {
            _oracleStorage.MaxRowsCount = value > 0 ? value : DefaultMaxRowsCount;
        }
    }

    public Partner Tslog => throw new NotImplementedException();

    public LinksStorage(string oracleDataSource, string pgsqlHost, string pgsqlDatabase, ILoggerFactory loggerFactory,
        IDictionary<string, object>? pgsqlExtensionParameters = null)
    {
        _oracleStorage = new LinksOracleStorage(oracleDataSource);
        _loggerFactory = loggerFactory;
        _pgsqlStorage = new LinksPgSqlStorage(pgsqlHost, pgsqlDatabase, _loggerFactory, pgsqlExtensionParameters);
    }

    public void ConnectOracle(string userName, string password)
    {
        _oracleStorage.Connect(userName, Utils.StringToSecureString(password));
    }
    public void SetOracleCommonSchema(string commonSchema)
    {
        _oracleStorage.CommonSchema = commonSchema;
    }
    public void SetOracleStaffSchema(string staffShcema)
    {
        _oracleStorage.StaffSchema = staffShcema;
    }
    public void SetOracleLegalEntitiesSchema(string legalEntitiesSchema)
    {
        _oracleStorage.LegalEntitiesSchema = legalEntitiesSchema;
    }
    public void SetOraclePartnersSchema(string partnersSchema)
    {
        _oracleStorage.PartnersSchema = partnersSchema;
    }
    public void DisconnectOracle()
    {
        _oracleStorage.Disconnect();
    }
    public void ConnectPgSql(string userName, string password)
    {
        _pgsqlStorage.Connect(userName, Utils.StringToSecureString(password));
    }
    public void SetPgsqlYardSchema(string linksSchema)
    {
        _pgsqlStorage.LinksSchema = linksSchema;
    }
    public void DisconnectPgsql()
    {
        _pgsqlStorage.Disconnect();
    }

    public Operation GetOperation(long id)
    {
        throw new NotImplementedException();
    }

    public Product GetProduct(string id)
    {
        throw new NotImplementedException();
    }

    public Product GetProductByGtin(string gtin)
    {
        throw new NotImplementedException();
    }

    public Product GetProduct(Partner partner, string partnerProductId)
    {
        throw new NotImplementedException();
    }

    public Product[] GetProducts(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public Entity GetIncomeState(string stateCode)
    {
        throw new NotImplementedException();
    }

    public ReceiptEndNotice GetReceiptEndNotice(string incomeId)
    {
        throw new NotImplementedException();
    }

    public ReceiptEndNotice GetReceiptEndNotice(Partner partner, string partnerInboundId)
    {
        throw new NotImplementedException();
    }

    public ReceiptEndNotice[] GetReceiptEndNotices(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public Entity GetOutgoState(string stateCode)
    {
        throw new NotImplementedException();
    }

    public PickingBeginNotice GetPickingBeginNotice(string outgoId)
    {
        throw new NotImplementedException();
    }

    public PickingBeginNotice GetPickingBeginNotice(Partner partner, string partnerOutgoId)
    {
        throw new NotImplementedException();
    }

    public PickingBeginNotice[] GetPickingBeginNotices(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public PickingEndNotice GetPickingEndNotice(string outgoId)
    {
        throw new NotImplementedException();
    }

    public PickingEndNotice GetPickingEndNotice(Partner partner, string partnerOutgoId)
    {
        throw new NotImplementedException();
    }

    public PickingEndNotice[] GetPickingEndNotices(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public ShippingNotice GetShippingNotice(string outgoId)
    {
        throw new NotImplementedException();
    }

    public ShippingNotice GetShippingNotice(Partner partner, string partnerOutgoId)
    {
        throw new NotImplementedException();
    }

    public ShippingNotice[] GetShippingNotices(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public LegalEntity[] GetAllUsers()
    {
        throw new NotImplementedException();
    }

    public LegalEntity Authenticate(AuthenticateRequest model)
    {
        throw new NotImplementedException();
    }

    public Partner GetPartner(string id)
    {
        throw new NotImplementedException();
    }

    public Partner[] GetPartners()
    {
        throw new NotImplementedException();
    }

    public LegalEntity GetLegalEntity(string id)
    {
        throw new NotImplementedException();
    }

    public LegalEntity[] GetLegalEntities(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public Entity GetCurrency(string currencyCode)
    {
        throw new NotImplementedException();
    }

    public MeasureUnit GetMeasureUnit(string measureUnitCode)
    {
        throw new NotImplementedException();
    }

    public string GenerateTransactionId()
    {
        throw new NotImplementedException();
    }

    public string GenerateObjectId(string objectName, LegalEntity partner)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _oracleStorage.Dispose();
        _pgsqlStorage.Dispose();
        GC.SuppressFinalize(this);
    }
}
