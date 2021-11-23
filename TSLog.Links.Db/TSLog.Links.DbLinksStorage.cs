using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using TSLog.BO;
using TSLog.Common;
using TSLog.Db;

namespace TSLog.Links.Db
{
    /// <summary>
    /// Класс для работы с хранилищем БД Oracle
    /// </summary>
    public class DbLinksStorage : DbStorage, ILinksStorage
    {
        public DbLinksStorage(string dataSource)
            : base(dataSource)
        {
            CommonSchema = "ecco";
            LinksSchema = "links";
            WmsSchema = "ecco2_s1_pdba";
            instance = this;
        }
        public static DbLinksStorage instance = null; // TODO: wrong decision


        /// <summary>
        /// Схема общими объектами БД (ecco)
        /// </summary>
        public string CommonSchema { private get; set; }

        /// <summary>
        /// Схема с объектами для азличной интеграции (links)
        /// </summary>
        public string LinksSchema { private get; set; }

        /// <summary>
        /// Схема с объектами WMS
        /// </summary>
        public string WmsSchema { private get; set; }

        #region "Constant" domains

        /// <summary>
        /// Статусы (состояния) приходов
        /// </summary>
        public Catalog<Entity> IncomeStates { get; set; } = null;

        /// <summary>
        /// Типы отгрузок
        /// </summary>
        public Catalog<Entity> OutgoTypes { get; set; } = null;

        /// <summary>
        /// Статусы (состояния) расходов
        /// </summary>
        public Catalog<Entity> OutgoStates { get; set; } = null;

        /// <summary>
        /// Валюты
        /// </summary>
        public Catalog<Entity> Currencies { get; set; } = null;

        /// <summary>
        /// Единицы измерения
        /// </summary>
        public Catalog<MeasureUnit> MeasureUnits { get; set; } = null;

        public Entity GetIncomeState(string stateCode)
        {
            stateCode = !string.IsNullOrWhiteSpace(stateCode) ? stateCode.Trim() : string.Empty;
            var incomeState = IncomeStates[stateCode];
            if (incomeState != null && incomeState != Entity.Null)
                return incomeState;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_state s where s.code = :code";
                command.Parameters.Add(new OracleParameter("code", stateCode));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    incomeState = new Entity(Convert.ToInt32(reader["id"]),
                        Convert.ToString(reader["code"]),
                        Convert.ToString(reader["name"]),
                        Convert.ToString(reader["short_name"]),
                        Convert.ToString(reader["ge_name"]));
                reader.Close();
            }
            return incomeState ?? Entity.Null;
        }

        public Entity[] GetIncomeStates()
        {
            var incomeStates = new List<Entity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_state s";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var state = new Entity(Convert.ToInt32(reader["id"]),
                    Convert.ToString(reader["code"]),
                    Convert.ToString(reader["name"]),
                    Convert.ToString(reader["short_name"]),
                    Convert.ToString(reader["ge_name"]));
                    incomeStates.Add(state);
                }
                reader.Close();
            }
            return incomeStates.ToArray();
        }

        public Entity GetOutgoType(string typeCode)
        {
            typeCode = !string.IsNullOrWhiteSpace(typeCode) ? typeCode : string.Empty;
            var outgoType = OutgoTypes[typeCode];
            if (outgoType != null && outgoType != Entity.Null)
                return outgoType;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_outgo_type ot where ot.code = :code";
                command.Parameters.Add(new OracleParameter("code", typeCode));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    outgoType = new Entity(Convert.ToInt32(reader["id"]),
                        Convert.ToString(reader["code"]),
                        Convert.ToString(reader["name"]));
                reader.Close();
            }
            return outgoType ?? Entity.Null;
        }

        public Entity[] GetOutgoTypes()
        {
            var outgoTypes = new List<Entity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_outgo_type ot";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var outgoType = new Entity(Convert.ToInt32(reader["id"]),
                        Convert.ToString(reader["code"]),
                        Convert.ToString(reader["name"]));
                    outgoTypes.Add(outgoType);
                }
                reader.Close();
            }
            return outgoTypes.ToArray();
        }

        public Entity GetOutgoState(string stateCode)
        {
            return GetIncomeState(stateCode);
        }

        public Entity[] GetOutgoStates()
        {
            return GetIncomeStates();
        }

        public Entity GetCurrency(string currencyCode)
        {
            currencyCode = !string.IsNullOrWhiteSpace(currencyCode) ? currencyCode.Trim() : string.Empty;
            var currency = Currencies[currencyCode];
            if (currency != null && currency != Entity.Null)
                return currency;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_currency c where c.code = :code";
                command.Parameters.Add(new OracleParameter("code", currencyCode));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        currency = new Entity(Convert.ToInt32(reader["is"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["name"]),
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            Convert.ToString(reader["note"]));
                    reader.Close();
                }
            }
            return currency ?? Entity.Null;
        }

        public Entity[] GetCurrncies()
        {
            var currencies = new List<Entity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_currency";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var currency = new Entity(Convert.ToInt32(reader["id"]),
                        Convert.ToString(reader["code"]),
                        Convert.ToString(reader["name"]),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        Convert.ToString(reader["note"]));
                    currencies.Add(currency);
                }
                reader.Close();
            }
            return currencies.ToArray();
        }

        public MeasureUnit GetMeasureUnit(string measureUnitCode)
        {
            measureUnitCode = !string.IsNullOrWhiteSpace(measureUnitCode) ? measureUnitCode.Trim() : string.Empty;
            var measureUnit = MeasureUnits[measureUnitCode];
            if (measureUnit != null && measureUnit != MeasureUnit.Null)
                return measureUnit;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_measure_unit m where m.code = :code";
                command.Parameters.Add(new OracleParameter("code", measureUnitCode));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        measureUnit = new MeasureUnit(Convert.ToInt64(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToString(reader["short_name"]),
                            Convert.ToString(reader["note"]),
                            Convert.ToString(reader["category"]),
                            Convert.ToInt32(reader["is_basic"]) == 1,
                            Convert.ToDouble(reader["coefficient"]),
                            Convert.ToInt32(reader["is_default"]) == 1);
                    reader.Close();
                }
            }
            return measureUnit ?? MeasureUnit.Null;
        }

        public MeasureUnit[] GetMeasureUnits()
        {
            var measureUnits = new List<MeasureUnit>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_measure_unit";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var measureUnit = new MeasureUnit(Convert.ToInt64(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToString(reader["short_name"]),
                            Convert.ToString(reader["note"]),
                            Convert.ToString(reader["category"]),
                            Convert.ToInt32(reader["is_basic"]) == 1,
                            Convert.ToDouble(reader["coefficient"]),
                            Convert.ToInt32(reader["is_default"]) == 1);
                        measureUnits.Add(measureUnit);
                    }
                    reader.Close();
                }
            }
            return measureUnits.ToArray();
        }

        #endregion "Constant" domains

        /// <summary>
        /// Партнёры (покупатели наших складских и логистических услуг)
        /// </summary>
        public Catalog<LegalEntity> Partners { private get; set; } = null;

        private LegalEntity _tslog = null;

        /// <summary>
        /// ТехСкладЛогистик
        /// </summary>
        public LegalEntity Tslog
        {
            get
            {
                if (_tslog == null)
                    _tslog = new LegalEntity("TSLOG", "TSL", "ТехСкладЛогистик");
                return _tslog;
            }
        }

        /// <summary>
        /// Товары
        /// </summary>
        public ProductCatalog<Product> Products { private get; set; } = null;

        public string GenerateTransactionId()
        {
            var transactionId = string.Empty;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select {LinksSchema}.common.gen_transaction_id id from dual";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        transactionId = Convert.ToString(reader["id"]);
                    reader.Close();
                }
            }
            return transactionId;
        }

        public string GenerateObjectId(string objectName, LegalEntity partner)
        {
            var objectId = string.Empty;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select {LinksSchema}.common.gen_object_id(:obj, :partner_id) id from dual";
                command.Parameters.Add(new OracleParameter("obj", objectName));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        objectId = Convert.ToString(reader["id"]);
                    reader.Close();
                }
            }
            return objectId;
        }

        #region Clients

        public LegalEntity GetPartner(string id)
        {
            id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
            var partner = Partners[id];

            if (id == Tslog.Code) // check if Tslog
                partner = Tslog;

            if (partner != null && partner != LegalEntity.Null)
                return partner;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_partner p where p.id = :id";
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        partner = new LegalEntity(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToString(reader["note1"])
                            )
                        {
                            Address = Convert.ToString(reader["address"])
                        };

                        Partners.Add(partner);
                    }
                    reader.Close();
                }
            }
            if (partner != null && partner != LegalEntity.Null)
                Partners.Add(partner);
            return partner ?? LegalEntity.Null;
        }

        public LegalEntity[] GetPartners()
        {
            var partners = new List<LegalEntity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_partner p";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var partner = new LegalEntity(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToString(reader["note"]));
                        partners.Add(partner);
                    }
                    reader.Close();
                }
            }
            return partners.ToArray();
        }

        public LegalEntity GetLegalEntity(string id)
        {
            //var ret = new LegalEntity("VEND_ASKANIA(TODO)", "just a legal entity", "just a legal entity for debug");
            //ret.Address = "Odinsovo, ulitza TEST, 69";
            //return ret;

            id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
            var partner = Partners[id];

            if (id == Tslog.Code) // check if Tslog
                partner = Tslog;

            if (partner != null && partner != LegalEntity.Null)
                return partner;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_legal_entity p where p.id = :id";
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        partner = new LegalEntity(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToString(reader["note1"])
                            )
                        {
                            Address = Convert.ToString(reader["address1"])
                        };

                        Partners.Add(partner);
                    }
                    reader.Close();
                }
            }
            if (partner != null && partner != LegalEntity.Null)
                Partners.Add(partner);
            return partner ?? LegalEntity.Null;
        }

        public LegalEntity[] GetLegalEntities(LegalEntity[] partners = null)
        {
            return null;
        }

        #endregion

        #region Products
        public Product GetProduct(string id)
        {
            id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
            var product = Products.GetByInternalCode(id);
            if (product != null && product != Product.Null)
                return product;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_product p where p.id = :id";
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        product = new Product(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["gtin"]),
                            Convert.ToString(reader["name"]),
                            note: Convert.ToString(reader["note1"]));

                        product.Length = Convert.ToInt32(reader[nameof(product.Length)]);
                        product.Width = Convert.ToInt32(reader[nameof(product.Width)]);
                        product.Height = Convert.ToInt32(reader[nameof(product.Height)]);
                        product.UnitLinerial = Convert.ToString(reader["length_measure"]);

                        product.Netto = Convert.ToInt32(reader["net_weight"]);
                        product.Netto = Convert.ToInt32(reader["gross_weight"]);
                        product.UnitWeight = Convert.ToString(reader["weight_measure"]);

                        product.Volume = Convert.ToInt32(reader[nameof(product.Volume)]);
                        product.UnitVolume = Convert.ToString(reader["volume_measure"]);

                        if (!Convert.IsDBNull(reader[nameof(product.Price)]))
                            product.Price = Convert.ToDouble(reader[nameof(product.Price)]);

                        Products.Add(product);
                    }
                    reader.Close();
                }
            }
            if (product != null && product != Product.Null)
                Products.Add(product);
            return product ?? Product.Null;
        }

        public Product GetProductByGtin(string gtin)
        {
            gtin = !string.IsNullOrWhiteSpace(gtin) ? gtin.Trim() : string.Empty;
            var product = Products[gtin];
            if (product != null && product != Product.Null)
                return product;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_product p where p.gtin = :gtin";
                command.Parameters.Add(new OracleParameter("gtin", gtin));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        product = new Product(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["gtin"]),
                            Convert.ToString(reader["name"]),
                            note: Convert.ToString(reader["note1"]));

                        product.Length = Convert.ToInt32(reader[nameof(product.Length)]);
                        product.Width = Convert.ToInt32(reader[nameof(product.Width)]);
                        product.Height = Convert.ToInt32(reader[nameof(product.Height)]);
                        product.UnitLinerial = Convert.ToString(reader["length_measure"]);

                        product.Netto = Convert.ToInt32(reader["net_weight"]);
                        product.Netto = Convert.ToInt32(reader["gross_weight"]);
                        product.UnitWeight = Convert.ToString(reader["weight_measure"]);

                        product.Volume = Convert.ToInt32(reader[nameof(product.Volume)]);
                        product.UnitVolume = Convert.ToString(reader["volume_measure"]);

                        if (!Convert.IsDBNull(reader[nameof(product.Price)]))
                            product.Price = Convert.ToDouble(reader[nameof(product.Price)]);

                        Products.Add(product);
                    }
                    reader.Close();
                }
            }
            if (product != null && product != Product.Null)
                Products.Add(product);
            return product ?? Product.Null;
        }

        public Product GetProduct(LegalEntity partner, string partnerProductId)
        {
            // TODO: realize
            return null;
        }

        public Product[] GetProducts(LegalEntity[] partners = null)
        {
            var products = new List<Product>();
            using (var command = _connection.CreateCommand())
            {
                var sql = new StringBuilder($"select * from {CommonSchema}.v_product p");
                var where = new StringBuilder();
                if (partners != null && partners.Length > 0)
                {
                    foreach (var partner in partners)
                    {
                        if (where.Length > 0)
                            where.Append(", ");
                        where.Append(Utils.ToSql(partner.Id));
                    }
                    where.Insert(0, "\nwhere p.partner_id in (");
                    where.Append(')');
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var product = new Product(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["gtin"]),
                            Convert.ToString(reader["name"]));
                        Products.Add(product);
                    }
                    reader.Close();
                }
            }
            return products.ToArray();
        }

        #endregion

        #region Inbound instruction and notification

        private Income.Line CreateIncomeLine(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var number = -1;
            var product = GetProduct(Convert.ToString(reader["product_id"]));
            var price = 0.0;
            var currencyCode = string.Empty;
            var currency = GetCurrency(currencyCode);
            var planQuantity = Convert.ToDouble(reader["plan_quantity"]);
            var factQuantity = Convert.ToDouble(reader["fact_quantity"]);
            var measureUnitCode = Convert.ToString(reader["quantity_measure_unit_code"]);
            var measureUnit = GetMeasureUnit(measureUnitCode);
            var line = new Income.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit);
            return line;
        }

        private Income.Line[] GetIncomeLines(Income income)
        {
            if (income == null)
                throw new ArgumentNullException(nameof(income));
            var lines = new List<Income.Line>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_income_line il where il.income_id = :income_id";
                command.Parameters.Add(new OracleParameter("income_id", income.Code));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var line = CreateIncomeLine(reader);
                        lines.Add(line);
                    }
                    reader.Close();
                }
            }
            return lines.ToArray();
        }

        private ReceiptEndNotice CreateReceiptEndNotice(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var incomeId = Convert.ToString(reader["income_id"]);
            var incomeNumber = Convert.ToString(reader["income_number"]);
            var incomeDate = DateTime.MinValue;
            var incomePartnerId = Convert.ToString(reader["partner_id"]);
            var incomePartner = GetPartner(incomePartnerId);
            var incomePartnerNumber = Convert.ToString(reader["partner_income_number"]);
            var incomeStateCode = Convert.ToString(reader["state_code"]);
            var incomeState = GetIncomeState(incomeStateCode);
            var incomePlanReceiptBegin = reader["plan_delivery_date"] != DBNull.Value ? Convert.ToDateTime(reader["plan_delivery_date"]) : DateTime.MinValue;
            var incomePlanReceiptEnd = reader["plan_delivery_date"] != DBNull.Value ? Convert.ToDateTime(reader["plan_delivery_date"]) : DateTime.MinValue;
            var incomeFactReceiptBegin = reader["fact_delivery_date"] != DBNull.Value ? Convert.ToDateTime(reader["fact_delivery_date"]) : DateTime.MinValue;
            var incomeFactReceiptEnd = reader["fact_delivery_date"] != DBNull.Value ? Convert.ToDateTime(reader["fact_delivery_date"]) : DateTime.MinValue;
            var incomeNote = Convert.ToString(reader["note"]);

            var income = new Income(incomeId, incomeNumber, incomeDate, incomePartner, incomePartnerNumber, incomeState, incomeNote)
            {
                PlanAcceptanceBegin = incomePlanReceiptBegin,
                PlanAcceptanceEnd = incomePlanReceiptEnd,
                FactAcceptanceBegin = incomeFactReceiptBegin,
                FactAcceptanceEnd = incomeFactReceiptEnd
            };

            var id = -1;
            var number = string.Empty;
            var date = DateTime.Now; // Дата создания сообщения
            var version = (byte)1;
            var sender = Tslog;
            var recipient = incomePartner;
            var serviceSeller = Tslog;
            var serviceBuyer = incomePartner;
            var shipper = incomePartner;
            var receiver = Tslog;
            var note = incomeNote;

            var receiptEndNotice = new ReceiptEndNotice(id, number, version, date, income,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver,
                note);
            return receiptEndNotice;
        }

        public ReceiptEndNotice GetReceiptEndNotice(string incomeId)
        {
            ReceiptEndNotice receiptEndNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_receipt_end_notice ren  where ren.income_id = :income_id";
                command.Parameters.Add(new OracleParameter("income_id", incomeId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        receiptEndNotice = CreateReceiptEndNotice(reader);
                        var lines = GetIncomeLines(receiptEndNotice.Income);
                        receiptEndNotice.Income.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return receiptEndNotice;
        }

        public ReceiptEndNotice GetReceiptEndNotice(LegalEntity partner, string partnerIncomeId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            ReceiptEndNotice receiptEndNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_receipt_end_notice ren " + "\n"
                    + "where ren.partner_income_number = :partner_income_id and ren.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_income_id", partnerIncomeId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        receiptEndNotice = CreateReceiptEndNotice(reader);
                        var lines = GetIncomeLines(receiptEndNotice.Income);
                        receiptEndNotice.Income.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return receiptEndNotice;
        }

        public ReceiptEndNotice[] GetReceiptEndNotices(LegalEntity[] partners = null)
        {
            var receiptEndNotices = new List<ReceiptEndNotice>();
            using (var command = _connection.CreateCommand())
            {
                var sql = new StringBuilder($"select * from {LinksSchema}.v_receipt_end_notice ren");
                var where = new StringBuilder();
                if (partners != null && partners.Length > 0)
                {
                    foreach (var partner in partners)
                    {
                        if (where.Length > 0)
                            where.Append(", ");
                        where.Append(partner.Id);
                    }
                    where.Insert(0, "\nwhere ren.partner_id in (");
                    where.Append(')');
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var receiptEndNotice = CreateReceiptEndNotice(reader);
                        receiptEndNotice.Income.AddLines(GetIncomeLines(receiptEndNotice.Income));
                        receiptEndNotices.Add(receiptEndNotice);
                    }
                    reader.Close();
                }
            }
            return receiptEndNotices.ToArray();
        }

        #endregion Inbound instruction and notification

        #region Outbound instruction and notification

        private Outgo.Line CreateOutgoLine(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var number = -1;
            var product = GetProduct(Convert.ToString(reader["product_id"]));
            var price = 0.0;
            var currencyCode = string.Empty;
            var currency = GetCurrency(currencyCode);
            var planQuantity = Convert.ToDouble(reader["plan_quantity"]);
            var factQuantity = Convert.ToDouble(reader["fact_quantity"]);
            var measureUnitCode = string.Empty;
            var measureUnit = GetMeasureUnit(measureUnitCode);
            var line = new Outgo.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit)
            {
                FactQuantity = factQuantity
            };
            return line;
        }

        private Outgo.Line[] GetOutgoLines(Outgo outgo)
        {
            if (outgo == null)
                throw new ArgumentNullException(nameof(outgo));
            var lines = new List<Outgo.Line>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_outgo_line ol where ol.outgo_id = :outgo_id";
                command.Parameters.Add(new OracleParameter("outgo_id", outgo.Code));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var line = CreateOutgoLine(reader);
                        lines.Add(line);
                    }
                    reader.Close();
                }
            }
            return lines.ToArray();
        }

        #region PickingBeginNotice

        private PickingBeginNotice CreatePickingBeginNotice(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var outgoId = Convert.ToString(reader["outgo_id"]);
            var outgoNumber = Convert.ToString(reader["outgo_number"]);
            var outgoDate = DateTime.MinValue;
            var outgoPartnerId = Convert.ToString(reader["partner_id"]);
            var outgoPartner = GetPartner(outgoPartnerId);
            var outgoPartnerNumber = Convert.ToString(reader["partner_outgo_number"]);
            var outgoStateCode = Convert.ToString(reader["state_code"]);
            var outgoState = GetOutgoState(outgoStateCode);
            var outgoNote = string.Empty;
            var outgo = new Outgo(outgoId, outgoNumber, outgoDate, outgoPartner, outgoPartnerNumber, outgoState, outgoNote);

            var id = -1;
            var number = string.Empty;
            var date = DateTime.Now; // Теуцщая дата и время сообщения
            var version = (byte)1;
            var sender = Tslog;
            var recipient = outgoPartner;
            var serviceSeller = Tslog;
            var serviceBuyer = outgoPartner;
            var shipper = Tslog;
            var receiver = outgoPartner;
            var note = string.Empty;

            var pickingStartNotice = new PickingBeginNotice(id, number, version, date, outgo,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);
            return pickingStartNotice;
        }

        public PickingBeginNotice GetPickingBeginNotice(string outgoId)
        {
            PickingBeginNotice pickingBeginNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_picking_begin_notice pbn where pbn.outgo_id = :outgo_id";
                command.Parameters.Add(new OracleParameter("outgo_id", outgoId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pickingBeginNotice = CreatePickingBeginNotice(reader);
                        var lines = GetOutgoLines(pickingBeginNotice.Outgo);
                        pickingBeginNotice.Outgo.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return pickingBeginNotice;
        }

        public PickingBeginNotice GetPickingBeginNotice(LegalEntity partner, string partnerOutgoId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            PickingBeginNotice pickingBeginNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_picking_begin_notice pbn" + "\n"
                    + "where pbn.partner_outgo_number = :partner_outgo_id and pbn.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_outgo_id", partnerOutgoId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pickingBeginNotice = CreatePickingBeginNotice(reader);
                        var lines = GetOutgoLines(pickingBeginNotice.Outgo);
                        pickingBeginNotice.Outgo.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return pickingBeginNotice;
        }

        public PickingBeginNotice[] GetPickingBeginNotices(LegalEntity[] partners = null)
        {
            var pickingBeginNotices = new List<PickingBeginNotice>();
            using (var command = _connection.CreateCommand())
            {
                var partnerIds = new StringBuilder();
                var stateCodes = string.Empty;
                var sql = new StringBuilder($"select * from {LinksSchema}.v_picking_begin_notice pbn");
                var where = new StringBuilder();
                if (partners.Length > 0)
                {
                    foreach (var partner in partners)
                    {
                        if (partnerIds.Length > 0)
                            partnerIds.Append(", ");
                        partnerIds.Append(string.Format("\'{0}\'", partner.Id));
                    }
                    if (where.Length > 0)
                        where.Append("\n and ");
                    where.Append(string.Format("pbn.partner_id in ({0})", partnerIds));
                }
                if (where.Length > 0)
                {
                    where.Insert(0, "where ");
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    var pickingBeginNotice = CreatePickingBeginNotice(reader);
                    pickingBeginNotices.Add(pickingBeginNotice);
                    var lines = GetOutgoLines(pickingBeginNotice.Outgo);
                    pickingBeginNotice.Outgo.AddLines(lines);
                    reader.Close();
                }
            }
            return pickingBeginNotices.ToArray();
        }

        #endregion PickingBeginNotice

        #region PickingEndNotice

        private PickingEndNotice CreatePickingEndNotice(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var outgoId = Convert.ToString(reader["outgo_id"]);
            var outgoNumber = Convert.ToString(reader["outgo_number"]);
            var outgoDate = DateTime.MinValue;
            var outgoPartnerId = Convert.ToString(reader["partner_id"]);
            var outgoPartner = GetPartner(outgoPartnerId);
            var outgoPartnerNumber = Convert.ToString(reader["partner_outgo_number"]);
            var outgoStateCode = Convert.ToString(reader["state_code"]);
            var outgoState = GetOutgoState(outgoStateCode);
            var outgoNote = string.Empty;
            var outgo = new Outgo(outgoId, outgoNumber, outgoDate, outgoPartner, outgoPartnerNumber, outgoState, outgoNote);

            var id = -1;
            var number = string.Empty;
            var date = DateTime.Now; // Текущая дата и время сообщения
            var version = (byte)1;
            var sender = Tslog;
            var recipient = outgoPartner;
            var serviceSeller = Tslog;
            var serviceBuyer = outgoPartner;
            var shipper = Tslog;
            var receiver = outgoPartner;
            var note = string.Empty;

            var pickingStartNotice = new PickingEndNotice(id, number, version, date, outgo,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);
            return pickingStartNotice;
        }

        public PickingEndNotice GetPickingEndNotice(string outgoId)
        {
            PickingEndNotice pickingEndNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_picking_begin_notice pen where pen.outgo_id = :outgo_id";
                command.Parameters.Add(new OracleParameter("outgo_id", outgoId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pickingEndNotice = CreatePickingEndNotice(reader);
                        var lines = GetOutgoLines(pickingEndNotice.Outgo);
                        pickingEndNotice.Outgo.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return pickingEndNotice;
        }

        public PickingEndNotice GetPickingEndNotice(LegalEntity partner, string partnerOutgoId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            PickingEndNotice pickingEndNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_picking_end_notice pen" + "\n"
                    + "where pen.partner_outgo_number = :partner_outgo_id and pen.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_outgo_id", partnerOutgoId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pickingEndNotice = CreatePickingEndNotice(reader);
                        var lines = GetOutgoLines(pickingEndNotice.Outgo);
                        pickingEndNotice.Outgo.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return pickingEndNotice;
        }

        public PickingEndNotice[] GetPickingEndNotices(LegalEntity[] partners = null)
        {
            var pickingEndNotices = new List<PickingEndNotice>();
            using (var command = _connection.CreateCommand())
            {
                var partnerIds = new StringBuilder();
                var stateCodes = string.Empty;
                var sql = new StringBuilder($"select * from {LinksSchema}.v_picking_end_notice pen");
                var where = new StringBuilder();
                if (partners.Length > 0)
                {
                    foreach (var partner in partners)
                    {
                        if (partnerIds.Length > 0)
                            partnerIds.Append(", ");
                        partnerIds.Append(string.Format("\'{0}\'", partner.Id));
                    }
                    if (where.Length > 0)
                        where.Append("\n and ");
                    where.Append(string.Format("pen.partner_id in ({0})", partnerIds));
                }
                if (where.Length > 0)
                {
                    where.Insert(0, "where ");
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    var pickingEndNotice = CreatePickingEndNotice(reader);
                    pickingEndNotices.Add(pickingEndNotice);
                    var lines = GetOutgoLines(pickingEndNotice.Outgo);
                    pickingEndNotice.Outgo.AddLines(lines);
                    reader.Close();
                }
            }
            return pickingEndNotices.ToArray();
        }

        #endregion PickingEndNotice

        #region ShippingNotice

        private ShippingNotice CreateShippingNotice(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var outgoId = Convert.ToString(reader["outgo_id"]);
            var outgoNumber = Convert.ToString(reader["outgo_number"]);
            var outgoDate = DateTime.MinValue;
            var outgoPartnerId = Convert.ToString(reader["partner_id"]);
            var outgoPartner = GetPartner(outgoPartnerId);
            var outgoPartnerNumber = Convert.ToString(reader["partner_outgo_number"]);
            var outgoStateCode = Convert.ToString(reader["state_code"]);
            var outgoState = GetOutgoState(outgoStateCode);
            var outgoNote = string.Empty;
            var outgo = new Outgo(outgoId, outgoNumber, outgoDate, outgoPartner, outgoPartnerNumber, outgoState, outgoNote);

            var id = -1;
            var number = string.Empty;
            var date = DateTime.Now; // Теуцщая дата и время сообщения
            var version = (byte)1;
            var sender = Tslog;
            var recipient = outgoPartner;
            var serviceSeller = Tslog;
            var serviceBuyer = outgoPartner;
            var shipper = Tslog;
            var receiver = outgoPartner;
            var note = string.Empty;

            var shippingNotice = new ShippingNotice(id, number, version, date, outgo,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);
            return shippingNotice;
        }

        public ShippingNotice GetShippingNotice(string outgoId)
        {
            ShippingNotice shippingNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_shipping_notice sn where sn.outgo_id = :outgo_id";
                command.Parameters.Add(new OracleParameter("outgo_id", outgoId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        shippingNotice = CreateShippingNotice(reader);
                        var lines = GetOutgoLines(shippingNotice.Outgo);
                        shippingNotice.Outgo.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return shippingNotice;
        }

        public ShippingNotice GetShippingNotice(LegalEntity partner, string partnerOutgoId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            ShippingNotice shippingNotice = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_shipping_notice sn" + "\n"
                    + "where sn.partner_outgo_number = :partner_outgo_id and sn.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_outgo_id", partnerOutgoId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        shippingNotice = CreateShippingNotice(reader);
                        var lines = GetOutgoLines(shippingNotice.Outgo);
                        shippingNotice.Outgo.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return shippingNotice;
        }

        public ShippingNotice[] GetShippingNotices(LegalEntity[] partners = null)
        {
            var shippingNotices = new List<ShippingNotice>();
            using (var command = _connection.CreateCommand())
            {
                var partnerIds = new StringBuilder();
                var stateCodes = string.Empty;
                var sql = new StringBuilder($"select * from {LinksSchema}.v_shipping_notice sn");
                var where = new StringBuilder();
                if (partners.Length > 0)
                {
                    foreach (var partner in partners)
                    {
                        if (partnerIds.Length > 0)
                            partnerIds.Append(", ");
                        partnerIds.Append(string.Format("\'{0}\'", partner.Id));
                    }
                    if (where.Length > 0)
                        where.Append("\n and ");
                    where.Append(string.Format("sn.partner_id in ({0})", partnerIds));
                }
                if (where.Length > 0)
                {
                    where.Insert(0, "where ");
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    var shippingNotice = CreateShippingNotice(reader);
                    shippingNotices.Add(shippingNotice);
                    var lines = GetOutgoLines(shippingNotice.Outgo);
                    shippingNotice.Outgo.AddLines(lines);
                    reader.Close();
                }
            }
            return shippingNotices.ToArray();
        }


        #endregion ShippingNotice

        #endregion Outbound instruction and notification


        #region login

        public LegalEntity[] GetAllUsers()
        {
            var users = new List<LegalEntity>();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_service_user";
                //command.Parameters.Add(new OracleParameter("login", model.Login)); // TODO: make more safe
                //command.Parameters.Add(new OracleParameter("pass", model.Password));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = GetLegalEntity(Convert.ToString(reader["partner_id"]));
                        user.Note = $"{user.Note} [debug](login: [{Convert.ToString(reader["name"])}] pass: [{Convert.ToString(reader["password"])}])";

                        users.Add(user);
                    }
                    reader.Close();
                }
            }
            return users.ToArray();
        }

        public LegalEntity Authenticate(AuthenticateRequest model)
        {
            //throw new NotImplementedException();
            var user = LegalEntity.Null;

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_service_user p where p.name = :login and p.password = :pass and p.active = 1";
                command.Parameters.Add(new OracleParameter("login", model.Login)); // TODO: make more safe
                command.Parameters.Add(new OracleParameter("pass", model.Password));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var a = reader["partner_id"];
                        user = GetLegalEntity(Convert.ToString(reader["partner_id"]));
                    }
                    reader.Close();
                }
            }
            return user ?? LegalEntity.Null;
        }
        #endregion
    }
}