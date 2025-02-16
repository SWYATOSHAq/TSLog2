using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
using TSLog.Incomes.BO;
using TSLog.Outgoes.BO;
using TSLog.Partners.BO;
using TSLog.Products.BO;

namespace TSLog.Links.Connector;

public class Storage(string dataSource) : OracleStorage(dataSource), IStorage
{
    private const string DefaultLinksSchema = "links";
    private const string DefaultPartnersSchema = "partners";

    public string LinksSchema { protected get; set; } = DefaultLinksSchema;
    public string PartnersSchema { protected get; set; } = DefaultPartnersSchema;

    private readonly Catalog<Entity> _states = new("STATES", "STATES");
    private readonly Catalog<Entity> _currencies = new("CURRENCIES", "CURRENCIES");
    private readonly ProductCatalog _products = new();
    private readonly Catalog<Partner> _partners = new("PARTNERS", "Партнёры");

    public void SetDataSource(string dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource))
            throw new ArgumentException($"{nameof(dataSource)} is empty.");
        _dataSource = dataSource.Trim();
    }

    public Document[] GetDocumentsToSend()
    {
        var sql = $"select * from {LinksSchema}.v_sending_document sd where sd.send_date is null";
        var documents = new List<Document>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var kind = Utils.DbValueToString(reader["kind"]) ?? string.Empty;
                var id = Utils.DbValueToString(reader["id"]) ?? string.Empty;
                var document = new Document()
                {
                    Kind = kind,
                    Id = id
                };
                documents.Add(document);
            }
        }
        return [.. documents];
    }
    public void MarkSendDocument(Document document)
    {
        // TODO: use stored procedure.
        var sql = $"insert into {LinksSchema}.sending_document (kind, id) values (:kind, :id)";
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("kind", OracleDbType.NVarchar2).Value = document.Kind;
        command.Parameters.Add("id", OracleDbType.NVarchar2).Value = document.Id;
        command.ExecuteScalar();
    }

    public Entity GetState(string id)
    {
        id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
        var state = _states[id];
        if (state != null && state != Entity.Null)
            return state;
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $"select * from {CommonSchema}.v_state s where s.id = :id";
            command.Parameters.Add(new OracleParameter("id", id));
            using var reader = command.ExecuteReader();
            if (reader.Read())
                state = new Entity(Utils.DbValueToInt(reader["id"]),
                    Utils.DbValueToString(reader["code"]),
                    Utils.DbValueToString(reader["name"]),
                    Utils.DbValueToString(reader["short_name"]),
                    Utils.DbValueToString(reader["ge_name"]));
            reader.Close();
        }
        return state ?? Entity.Null;
    }
    public Entity GetCurrency(string code)
    {
        var SelectCurrencyByCodeSql =
            $"select c.id, c.code, c.name, c.short_name, c.alt_name, c.display_name, c.is_active, c.display_order, c.note " +
            $"  from {CommonSchema}.v_currency c " +
            $" where c.code = :code";
        var currency = GetEntity(code, SelectCurrencyByCodeSql, _currencies);
        return currency;
    }
    public Product GetProduct(string id)
    {
        id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
        var product = _products.GetByInternalCode(id);
        if (product != null && product != Product.Null)
            return product;
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $"select * from {CommonSchema}.v_product p where p.id = :id";
            command.Parameters.Add(new OracleParameter("id", id));
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                product = new Product(Utils.DbValueToString(reader["id"]),
                    Utils.DbValueToString(reader["code"]),
                    Utils.DbValueToString(reader["gtin"]),
                    Utils.DbValueToString(reader["name"]),
                    note: Utils.DbValueToString(reader["note1"]))
                {
                    Length = Utils.DbValueToInt(reader["length"]),
                    Width = Utils.DbValueToInt(reader["width"]),
                    Height = Utils.DbValueToInt(reader["height"]),
                    UnitLinerial = Utils.DbValueToString(reader["length_measure"]),

                    NetWeight = Utils.DbValueToInt(reader["net_weight"])
                };
                product.NetWeight = Utils.DbValueToInt(reader["gross_weight"]);
                product.UnitWeight = Utils.DbValueToString(reader["weight_measure"]);

                product.Volume = Utils.DbValueToInt(reader["volume"]);
                product.UnitVolume = Utils.DbValueToString(reader["volume_measure"]);

                if (!Convert.IsDBNull(reader[nameof(product.Price)]))
                    product.Price = Utils.DbValueToDouble(reader["price"]);

                _products.Add(product);
            }
            reader.Close();
        }
        if (product != null && product != Product.Null)
            _products.Add(product);
        return product ?? Product.Null;
    }
    
    private static Partner CreatePartner(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var id = Utils.DbValueToString(reader["id"]);
        var name = Utils.DbValueToString(reader["name"]);
        var isActive = Utils.DbValueToLong(reader["display_order"]) < 9999;
        var displayOrder = Utils.DbValueToLong(reader["display_order"]);
        var partner = new Partner(id,
            name,
            name, // note.
            string.Empty,
            string.Empty,
            isActive,
            displayOrder);
        return partner;
    }
    /// <summary>
    /// Возвращает клиента (мандант) по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Клиент (мандант).</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="PartnerNotFoundException"></exception>
    public Partner GetPartner(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id is empty");
        id = id.Trim().ToUpper();
        var partner = _partners[id];
        if (partner != null && partner != Partner.Null && partner != Partner.Empty)
            return partner;
        using (var command = _connection.CreateCommand())
        {
            var SelectPartnerByIdSql =
                $"select p.id, p.name, p.display_order\n" +
                $"  from {PartnersSchema}.par_partner#v p\n" +
                $" where upper(trim(p.id)) = :id";
            command.CommandText = SelectPartnerByIdSql;
            command.Parameters.Add(new OracleParameter("id", id));
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                partner = CreatePartner(reader);
                _partners.Add(partner);
            }
            reader.Close();
        }
        if (partner != null && partner != Partner.Null && partner != Partner.Empty)
            return partner;
        throw new PartnerNotFoundException(id);
    }
    private Income.Line CreateIncomeLine(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var number = -1;
        var product = GetProduct(Utils.DbValueToString(reader["product_id"]));
        var price = 0.0;
        var planQuantity = Utils.DbValueToDouble(reader["plan_quantity"]);
        var factQuantity = Utils.DbValueToDouble(reader["fact_quantity"]);
        var measureUnitCode = Utils.DbValueToString(reader["quantity_measure_unit_code"]);
        var measureUnit = GetMeasureUnit(measureUnitCode);
        var line = new Income.Line(number, product, price, null, planQuantity, factQuantity, measureUnit);
        return line;
    }
    private Income.Line[] GetIncomeLines(Income income)
    {
        ArgumentNullException.ThrowIfNull(income);
        var lines = new List<Income.Line>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $"select * from {CommonSchema}.v_income_line il where il.income_id = :income_id";
            command.Parameters.Add(new OracleParameter("income_id", income.Id));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var line = CreateIncomeLine(reader);
                lines.Add(line);
            }
            reader.Close();
        }
        return [.. lines];
    }
    private Income CreateIncome(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var id = Utils.DbValueToString(reader["id"]);
        var number = Utils.DbValueToString(reader["number"]);
        var date = Utils.DbValueToDateTime(reader["date"]);
        
        var partnerId = Utils.DbValueToString(reader["partner_id"]);
        var partner = GetPartner(partnerId);
        var partnerNumber = Utils.DbValueToString(reader["partner_number"]);
        
        var stateCode = Utils.DbValueToString(reader["state_code"]);
        var state = GetState(stateCode);

        Income income = new(id, number, date, partner, partnerNumber, state);
        var lines = GetIncomeLines(income);
        income.AddLines(lines);
        return income;
    }
    /// <summary>
    /// Возвращает приход по заданному идентификатору прихода.
    /// </summary>
    /// <param name="id">Id прихода.</param>
    /// <returns>Объект прихода.</returns>
    /// <exception cref="Exception"></exception>
    public Income GetIncome(string id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = $"select * from {CommonSchema}.v_income i where i.id = :id";
        command.Parameters.Add(new OracleParameter("id", id));
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            reader.Close();
            return CreateIncome(reader);
        }
        else
        {
            reader.Close();
            throw new Exception($"Income not found by id {id}.");
        }
    }
    private Outgo.Line CreateOutgoLine(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var number = -1;
        var product = GetProduct(Utils.DbValueToString(reader["product_id"]));
        var price = 0.0;
        var planQuantity = Utils.DbValueToDouble(reader["plan_quantity"]);
        var factQuantity = Utils.DbValueToDouble(reader["fact_quantity"]);
        var measureUnitCode = Utils.DbValueToString(reader["quantity_measure_unit_code"]);
        var measureUnit = GetMeasureUnit(measureUnitCode);
        var line = new Outgo.Line(number, product, price, null, planQuantity, factQuantity, measureUnit)
        {
            FactQuantity = factQuantity
        };
        return line;
    }
    private Outgo.Line[] GetOutgoLines(Outgo outgo)
    {
        ArgumentNullException.ThrowIfNull(outgo);
        var lines = new List<Outgo.Line>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $"select * from {CommonSchema}.v_outgo_line ol where ol.outgo_id = :outgo_id";
            command.Parameters.Add(new OracleParameter("outgo_id", outgo.Id));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var line = CreateOutgoLine(reader);
                lines.Add(line);
            }
            reader.Close();
        }
        return [.. lines];
    }
    private Outgo CreateOutgo(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var id = Utils.DbValueToString(reader["id"]);
        var number = Utils.DbValueToString(reader["number"]);
        var date = Utils.DbValueToDateTime(reader["create_date"]);

        var partnerId = Utils.DbValueToString(reader["partner_id"]);
        var partner = GetPartner(partnerId);
        var partnerNumber = Utils.DbValueToString(reader["partner_outgo_number"]);

        var stateCode = Utils.DbValueToString(reader["state_id"]);
        var state = GetState(stateCode);

        Outgo outgo = new(id, number, date, partner, partnerNumber, state);
        var lines = GetOutgoLines(outgo);
        outgo.AddLines(lines);
        return outgo;
    }
    /// <summary>
    /// Возвращает расход по заданному идентификатору расхода.
    /// </summary>
    /// <param name="id">Id расхода.</param>
    /// <returns>Объект расхода.</returns>
    /// <exception cref="Exception"></exception>
    public Outgo GetOutgo(string id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = 
            $"select o.id, o.\"number\", o.create_date,\n" +
            $"       o.partner_id, o.partner_outgo_number, o.state_id\n" +
            $"  from {CommonSchema}.v_outgo o where o.id = :id";
        command.Parameters.Add(new OracleParameter("id", id));
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var outgo = CreateOutgo(reader);
            reader.Close();
            return outgo;
        }
        else
        {
            reader.Close();
            throw new Exception($"Outgo not found by id {id}.");
        }
    }
}
