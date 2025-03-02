using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Products.BO;

namespace TSLog.Links.Data;

/// <summary>
/// Класс для работы с данными в БД PostgreSQL.
/// </summary>
public class LinksPgSqlStorage : PgSqlStorage, ILinksCommonStorage
{
    private const int DefaultMaxRowsCount = 16;
    private const string DefaultCommonSchema = "ecco";
    private const string DefaultLinksSchema = "links";
    private const short DefaultWorkersCount = 1;
    private readonly ILoggerFactory _loggerFactory;
    /// <summary>
    /// Максимальное количество строк для вывода.
    /// </summary>
    public int MaxRowsCount { protected get; set; } = DefaultMaxRowsCount;
    /// <summary>
    /// Схема с объектами для различной интеграции (links).
    /// </summary>
    public string LinksSchema { private get; set; } = DefaultLinksSchema;

    public LinksPgSqlStorage(string host, string database, ILoggerFactory loggerFactory,
        IDictionary<string, object>? extensionParameters = null)
        : base(host, database, loggerFactory.CreateLogger<PgSqlStorage>(), extensionParameters)
    {
        _loggerFactory = loggerFactory;
    }

    private static NpgsqlParameter CreateParameter(string name, NpgsqlDbType type, object value)
    {
        var parameter = new NpgsqlParameter(name, type)
        {
            Value = value
        };
        return parameter;
    }

    public Partner Tslog => throw new NotImplementedException();

    public LegalEntity Authenticate(AuthenticateRequest model)
    {
        throw new NotImplementedException();
    }

    public string GenerateObjectId(string objectName, LegalEntity partner)
    {
        throw new NotImplementedException();
    }

    public string GenerateTransactionId()
    {
        throw new NotImplementedException();
    }

    public LegalEntity[] GetAllUsers()
    {
        throw new NotImplementedException();
    }

    public Entity GetCurrency(string currencyCode)
    {
        throw new NotImplementedException();
    }

    public Entity GetIncomeState(string stateCode)
    {
        throw new NotImplementedException();
    }

    public LegalEntity[] GetLegalEntities(Partner[]? partners = null)
    {
        throw new NotImplementedException();
    }

    public LegalEntity GetLegalEntity(string id)
    {
        throw new NotImplementedException();
    }

    public MeasureUnit GetMeasureUnit(string measureUnitCode)
    {
        throw new NotImplementedException();
    }

    public Entity GetOutgoState(string stateCode)
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

    public Product GetProduct(string id)
    {
        throw new NotImplementedException();
    }

    public Product GetProduct(Partner partner, string partnerProductId)
    {
        throw new NotImplementedException();
    }

    public Product GetProductByGtin(string gtin)
    {
        throw new NotImplementedException();
    }

    public Product[] GetProducts(Partner[]? partners = null)
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
}
