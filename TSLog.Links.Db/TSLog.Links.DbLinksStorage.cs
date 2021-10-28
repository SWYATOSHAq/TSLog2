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
        }

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
        public Catalog<Entity> InboundsStates { private get; set; } = null;
        /// <summary>
        /// Типы отгрузок
        /// </summary>
        public Catalog<Entity> OutboundsTypes { private get; set; } = null;
        /// <summary>
        /// Статусы (состояния) расходов
        /// </summary>
        public Catalog<Entity> OutboundsStates { private get; set; } = null;
        /// <summary>
        /// Валюты
        /// </summary>
        public Catalog<Entity> Currencies { private get; set; } = null;
        /// <summary>
        /// Единицы измерения
        /// </summary>
        public Catalog<MeasureUnit> MeasureUnits { private get; set; } = null;
        public Entity GetInboundState(string stateCode)
        {
            stateCode = !string.IsNullOrWhiteSpace(stateCode) ? stateCode.Trim() : string.Empty;
            var inboundState = InboundsStates[stateCode];
            if (inboundState != null && inboundState != Entity.Null)
                return inboundState;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_state s where s.code = :code";
                command.Parameters.Add(new OracleParameter("code", stateCode));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        inboundState = new Entity(Convert.ToInt32(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToString(reader["short_name"]),
                            Convert.ToString(reader["ge_name"]));
                    reader.Close();
                }
            }
            return inboundState ?? Entity.Null;
        }
        public Entity[] GetInboundsStates()
        {
            var inboundStates = new List<Entity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_state s";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var state = new Entity(Convert.ToInt32(reader["id"]),
                        Convert.ToString(reader["code"]),
                        Convert.ToString(reader["name"]),
                        Convert.ToString(reader["short_name"]),
                        Convert.ToString(reader["ge_name"]));
                        inboundStates.Add(state);
                    }
                    reader.Close();
                }
            }
            return inboundStates.ToArray();
        }
        public Entity GetOutboundType(string typeCode)
        {
            typeCode = !string.IsNullOrWhiteSpace(typeCode) ? typeCode : string.Empty;
            var outboundType = OutboundsTypes[typeCode];
            if (outboundType != null && outboundType != Entity.Null)
                return outboundType;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_outbound_type ot where ot.code = :code";
                command.Parameters.Add(new OracleParameter("code", typeCode));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        outboundType = new Entity(Convert.ToInt32(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["name"]));
                    reader.Close();
                }
            }
            return outboundType ?? Entity.Null;
        }
        public Entity[] GetOutboundsTypes()
        {
            var outboundTypes = new List<Entity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_outbound_type ot";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var outboundType = new Entity(Convert.ToInt32(reader["id"]), 
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["name"]));
                        outboundTypes.Add(outboundType);
                    }
                    reader.Close();
                }
            }
            return outboundTypes.ToArray();
        }
        public Entity GetOutboundState(string stateCode)
        {
            return GetInboundState(stateCode);
        }
        public Entity[] GetOutboundsStates()
        {
            return GetInboundsStates();
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
                using (var reader = command.ExecuteReader())
                {
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
        #endregion

        #region Products and clients
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
                    _tslog = new LegalEntity(1, "TSLOG", "ТехСкладЛогистик");
                return _tslog;
            }
        }
        /// <summary>
        /// Товары
        /// </summary>
        public Catalog<Product> Products { private get; set; } = null;
        public LegalEntity GetPartner(string partnerCode)
        {
            partnerCode = !string.IsNullOrWhiteSpace(partnerCode) ? partnerCode.Trim() : string.Empty;
            var partner = Partners[partnerCode];
            if (partner != null && partner != LegalEntity.Null)
                return partner;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_partner p where p.id = :id";
                command.Parameters.Add(new OracleParameter("id", partnerCode));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        partner = new LegalEntity(-1,
                            Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]));
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
                        var partner = new LegalEntity(-1,
                            Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]));
                        partners.Add(partner);
                    }
                    reader.Close();
                }
            }
            return partners.ToArray();
        }
        public Product GetProduct(string productCode)
        {
            productCode = !string.IsNullOrWhiteSpace(productCode) ? productCode.Trim() : string.Empty;
            var product = Products[productCode];
            if (product != null && product != Product.Null)
                return product;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_product p where p.id = :id";
                command.Parameters.Add(new OracleParameter("id", productCode));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        product = new Product(Convert.ToString(reader["id"]),
                            Convert.ToString(reader["code"]),
                            Convert.ToString(reader["gtin"]),
                            Convert.ToString(reader["name"]));
                        Products.Add(product);
                    }
                    reader.Close();
                }
            }
            if (product != null && product != Product.Null)
                Products.Add(product);
            return product ?? Product.Null;
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
        private Inbound.Line CreateInboundNotificationLine(DbDataReader reader)
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
            var line = new Inbound.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit);
            return line;
        }
        private InboundNotification CreateInboundNotification(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var inboundId = Convert.ToString(reader["inbound_id"]);
            var inboundNumber = Convert.ToString(reader["inbound_number"]);
            var inboundDate = DateTime.MinValue;
            var inboundPartnerId = Convert.ToString(reader["partner_id"]);
            var inboundPartner = GetPartner(inboundPartnerId);
            var inboundPartnerNumber = Convert.ToString(reader["partner_inbound_number"]); ;
            var inboundStateCode = Convert.ToString(reader["state_code"]);
            var inboundState = GetInboundState(inboundStateCode);
            var inboundPlanAcceptanceBegin = Convert.ToDateTime(reader["plan_delivery_date"]);
            var inboundPlanAcceptanceEnd = Convert.ToDateTime(reader["plan_delivery_date"]);
            var inboundFactAccpeptanceBegin = Convert.ToDateTime(reader["fact_delivery_date"]);
            var inboundFactAcceptanceEnd = Convert.ToDateTime(reader["fact_delivery_date"]);
            var inboundNote = Convert.ToString(reader["note"]);

            var inbound = new Inbound(inboundId, inboundNumber, inboundDate, inboundPartner, inboundPartnerNumber, inboundState, inboundNote);
            inbound.PlanAcceptanceBegin = inboundPlanAcceptanceBegin;
            inbound.PlanAcceptanceEnd = inboundPlanAcceptanceEnd;
            inbound.FactAcceptanceBegin = inboundFactAccpeptanceBegin;
            inbound.FactAcceptanceEnd = inboundFactAcceptanceEnd;

            var id = -1;
            var number = string.Empty;
            var date = DateTime.Now; // Дата создания сообщения
            var version = (byte)1;            
            var sender = Tslog;
            var recipient = inboundPartner;
            var serviceSeller = Tslog;
            var serviceBuyer = inboundPartner;
            var shipper = inboundPartner;
            var receiver = Tslog;
            var note = inboundNote;

            var inboundNotification = new InboundNotification(id, number, version, date, inbound,  
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver,
                note);
            return inboundNotification;
        }
        private Inbound.Line[] GetInboundNotificationLines(InboundNotification inboundNotification)
        {
            if (inboundNotification == null)
                throw new ArgumentNullException(nameof(inboundNotification));
            var lines = new List<Inbound.Line>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_inbound_notification_line inml where inml.inbound_id = :inbound_id";
                command.Parameters.Add(new OracleParameter("inbound_id", inboundNotification.Inbound.Code));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var line = CreateInboundNotificationLine(reader);
                        lines.Add(line);
                    }
                    reader.Close();
                }
            }
            return lines.ToArray();
        }
        public InboundNotification GetInboundNotification(string inboundId)
        {
            InboundNotification inboundNotification = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_inbound_notification inm  where inm.inbound_id = :inbound_id";
                command.Parameters.Add(new OracleParameter("inbound_id", inboundId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        inboundNotification = CreateInboundNotification(reader);
                        var lines = GetInboundNotificationLines(inboundNotification);
                        inboundNotification.Inbound.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return inboundNotification;
        }
        public InboundNotification GetInboundNotification(LegalEntity partner, string partnerInboundId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            InboundNotification inboundNotification = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_inbound_notification inm " + "\n"
                    + "where inm.partner_inbound_id = :partner_inbound_id and inm.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_inbound_id", partnerInboundId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        inboundNotification = CreateInboundNotification(reader);
                        var lines = GetInboundNotificationLines(inboundNotification);
                        inboundNotification.Inbound.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return inboundNotification;
        }
        public InboundNotification[] GetInboundNotifications(LegalEntity[] partners = null)
        {
            var inboundNotifications = new List<InboundNotification>();
            using (var command = _connection.CreateCommand())
            {
                var sql = new StringBuilder($"select * from {LinksSchema}.v_inbound_notification inm");
                var where = new StringBuilder();
                if (partners != null && partners.Length > 0) 
                {
                    foreach (var partner in partners)
                    {
                        if (where.Length > 0)
                            where.Append(", ");
                        where.Append(partner.Id);
                    }
                    where.Insert(0, "\nwhere inm.partner_id in (");
                    where.Append(")");
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var inboundNotification = CreateInboundNotification(reader);
                        inboundNotification.Inbound.AddLines(GetInboundNotificationLines(inboundNotification));
                        inboundNotifications.Add(inboundNotification);
                    }
                    reader.Close();
                }
            }
            return inboundNotifications.ToArray();
        }
        #endregion

        #region Outbound instruction and notification
        private Outbound.Line CreateOutboundNotificationLine(DbDataReader reader)
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
            var line = new Outbound.Line(number, product, price, currency, planQuantity, factQuantity, measureUnit);
            line.FactQuantity = factQuantity;
            return line;
        }
        private OutboundNotification CreateOutboundNotification(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var outboundId = Convert.ToString(reader["outbound_id"]);            
            var outboundNumber = Convert.ToString(reader["outbound_number"]);
            var outboundPartnerId = Convert.ToString(reader["partner_id"]);
            var outboundPartner = GetPartner(outboundPartnerId);
            var outboundPartnerNumber = Convert.ToString(reader["partner_outbound_number"]);
            var outboundStateCode = Convert.ToString(reader["state_code"]);
            var outboundState = GetOutboundState(outboundStateCode);
            var outboundNote = string.Empty;
            var outbound = new Outbound(outboundId, outboundNumber, outboundPartner, outboundPartnerNumber, outboundState, outboundNote);

            var id = -1;
            var number = string.Empty;
            var date = DateTime.Now; // Теуцщая дата и время сообщения
            var version = (byte)1;
            var sender = Tslog;
            var recipient = outboundPartner;
            var serviceSeller = Tslog;
            var serviceBuyer = outboundPartner;
            var shipper = Tslog;
            var receiver = outboundPartner;
            var note = string.Empty;

            var outboundNotification = new OutboundNotification(id, number, version, date, outbound,
                sender, recipient, serviceSeller, serviceBuyer, shipper, receiver, note);
            return outboundNotification;
        }
        private Outbound.Line[] GetOutboundNotificationLines(OutboundNotification outboundNotification)
        {
            if (outboundNotification == null)
                throw new ArgumentNullException(nameof(outboundNotification));
            var lines = new List<Outbound.Line>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_outbound_notification_line onml where onml.outbond_id = :outbond_id";
                command.Parameters.Add(new OracleParameter("outbond_id", outboundNotification.Outbound.Code));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var line = CreateOutboundNotificationLine(reader);
                        lines.Add(line);
                    }
                    reader.Close();
                }
            }
            return lines.ToArray();
        }
        public OutboundNotification GetOutboundNotification(string outboundId)
        {
            OutboundNotification outboundNotification = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_outbound_notification onm where onm.outbound_id = :outbound_id";
                command.Parameters.Add(new OracleParameter("outbound_id", outboundId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        outboundNotification = CreateOutboundNotification(reader);
                        var lines = GetOutboundNotificationLines(outboundNotification);
                        outboundNotification.Outbound.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return outboundNotification;
        }
        public OutboundNotification GetOutboundNotification(LegalEntity partner, string partnerOutboundId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            OutboundNotification outboundNotification = null;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {LinksSchema}.v_outbound_notification onm" + "\n"
                    + "where onm.partner_outbound_id = :partner_outbound_id and onm.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_outbound_id", partnerOutboundId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        outboundNotification = CreateOutboundNotification(reader);
                        var lines = GetOutboundNotificationLines(outboundNotification);
                        outboundNotification.Outbound.AddLines(lines);
                    }
                    reader.Close();
                }
            }
            return outboundNotification;
        }
        public OutboundNotification[] GetOutboundNotifications(LegalEntity[] partners = null, Entity[] states = null)
        {
            var outboundNotifications = new List<OutboundNotification>();
            using (var command = _connection.CreateCommand())
            {
                var partnerIds = new StringBuilder();
                var stateCodes = string.Empty;                
                var sql = new StringBuilder($"select * from {LinksSchema}.v_outbound_notification onm");
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
                    where.Append(string.Format("onm.partner_id in ({0})", partnerIds));
                }
                if (states.Length > 0)
                {
                    foreach (var state in states)
                    {
                        if (partnerIds.Length > 0)
                            partnerIds.Append(", ");
                        partnerIds.Append(string.Format("\'{0}\'", state.Code));
                    }
                    if (where.Length > 0)
                        where.Append("\n and ");
                    where.Append(string.Format("onm.state_code in ({0})", stateCodes));
                }
                if (where.Length > 0)
                {
                    where.Insert(0, "where ");
                    sql.Append(where);
                }
                command.CommandText = sql.ToString();
                using (var reader = command.ExecuteReader())
                {
                    var outboundNotification = CreateOutboundNotification(reader);
                    outboundNotifications.Add(outboundNotification);
                    var lines = GetOutboundNotificationLines(outboundNotification);
                    outboundNotification.Outbound.AddLines(lines);
                    reader.Close();
                }
            }
            return outboundNotifications.ToArray();
        }


        public OutboundNotification[] GetOrderResponses()
        {
            return null;
        }
        public OutboundNotification GetOrderResponse(LegalEntity partner, string partnerOrderId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            return null;
        }
        public OutboundNotification[] GetPickingStartNotifications()
        {
            return null;
        }
        public OutboundNotification[] GetPickingFinishNotifications()
        {
            return null;
        }
        public OutboundNotification[] GetShippingNotifications()
        {
            return null;
        }
        #endregion

        #region DEBUG
        public ProductsList GetProductsList(LegalEntity partner) // TODO: to delete (just for debug)
        {
            throw new NotImplementedException();
        } 
        public ClientsList GetClientsList(bool loadPartners = false) // TODO: to delete (just for debug)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}