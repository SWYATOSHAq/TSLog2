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
        { }

        public string CommonSchema { private get; set; }
        public string LinksSchema { private get; set; }
        public string WmsSchema { private get; set; }

        #region TODO: Разнести по разным сборкам!!! (Colkhozing)
        public Catalog<Entity> InboundsStaes { get; set; }
        public Catalog<Entity> OutboundStates { get; set; }
        public Catalog<MeasureUnit> MeasureUnits { get; set; }
        public Catalog<LegalEntity> Partners { get; set; }
        private LegalEntity _tslog = null;
        public LegalEntity Tslog
        {
            get
            {
                if (_tslog == null)
                    _tslog = new LegalEntity(1, "TSLOG", "ТехСкладЛогистик");
                return _tslog;
            }
        }
        public Catalog<Product> Products { get; set; }

        #region "Общие" бизнес объекты
        public Entity[] GetInboundStates()
        {
            return null;
        }
        public Entity GetOutboundStates()
        {
            return null;
        }
        public MeasureUnit[] GetMeasureUnits()
        {
            var measureUnits = new List<MeasureUnit>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"select * from {CommonSchema}.v_measure_unit";
                using (var reader = command.ExecuteReader())
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
            }
            return measureUnits.ToArray();
        }
        public LegalEntity GetPartner(string code) 
        {
            var partner = Partners[code];
            if (partner == null)
            {
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = string.Format("select * from ecco.v_partner p where p.id = {0}", Utils.ToSql(code));
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
            }
            return partner != null ? partner : LegalEntity.Null;
        }
        public LegalEntity[] GetPartners()
        {
            var partners = new List<LegalEntity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "select * from ecco.v_partner p";
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
        public Product GetProduct(string code)
        {
            code = !string.IsNullOrWhiteSpace(code) ? code.Trim() : string.Empty;
            var product = Products[code];
            if (product == null)
            {
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = string.Format("select * from ecco.v_product p where p.id = :id");
                    command.Parameters.Add(new OracleParameter("id", code));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Product(-1,
                                Convert.ToString(reader["id"]),
                                Convert.ToString(reader["name"]));
                            Products.Add(product);
                        }
                        reader.Close();
                    }
                }
                if (product != null && product != Product.Null)
                    Products.Add(product);
            }
            return product != null ? product : Product.Null;
        }
        public Product[] GetProducts(LegalEntity partner)
        {
            var products = new List<Product>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = string.Format("select * from ecco.v_product p where p.partner_id = :partner_id");
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var product = new Product(-1,
                            Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]));
                        Products.Add(product);
                    }
                    reader.Close();
                }
            }
            return products.ToArray();
        }
        public Product[] GetProducts()
        {
            var products = new List<Product>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = string.Format("select * from ecco.v_product p");
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var product = new Product(-1,
                            Convert.ToString(reader["id"]),
                            Convert.ToString(reader["name"]));
                        Products.Add(product);
                    }
                    reader.Close();
                }
            }
            return products.ToArray();
        }
        #endregion
        #endregion

        public void PostProductsList(ProductsList productsList, LegalEntity partner)
        {
            if (productsList == null)
                throw new ArgumentNullException(nameof(productsList));
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "";
                command.ExecuteNonQuery();
            }
        }
        public void PostClientsList(ClientsList clientsList, LegalEntity partner)
        {
            if (clientsList == null)
                throw new ArgumentNullException(nameof(clientsList));
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "";
                command.ExecuteNonQuery();
            }
        }

        #region Inbound instruction and notification
        public void PostInboundInstruction(InboundInstruction inboundInstruction)
        {
            if (inboundInstruction == null)
                throw new ArgumentNullException("inboundInstruction");
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "";
                command.ExecuteNonQuery();
            }
        }
        private InboundNotification.Line CreateInboundNotificationLine(DbDataReader reader)
        {
            var product = GetProduct(Convert.ToString(reader["product_id"]));
            var planQuantity = Convert.ToDouble(reader["plan_quantity"]);
            var factQuantity = Convert.ToDouble(reader["fact_quantity"]);
            var line = new InboundNotification.Line(-1, product, planQuantity);
            line.FactQuantity = factQuantity;
            return line;
        }
        private InboundNotification CreateInboundNotification(DbDataReader reader)
        {
            var id = Convert.ToString(reader["inbound_id"]);
            var inboundNotification = new InboundNotification(-1, 1);
            inboundNotification.InboundId = id;
            inboundNotification.Partner = GetPartner(Convert.ToString(reader["partner_id"]));
            inboundNotification.State = Convert.ToString(reader["state_code"]);
            inboundNotification.PartnerInboundNumber = Convert.ToString(reader["partner_inbound_number"]);
            inboundNotification.InboundPlanDeliveryDate = Convert.ToDateTime(reader["plan_delivery_date"]);
            inboundNotification.FactInboundDeliveryDate = Convert.ToDateTime(reader["fact_delivery_date"]);
            inboundNotification.Note = Convert.ToString(reader["note"]);
            return inboundNotification;
        }
        private InboundNotification.Line[] GetInboundNotificationLines(InboundNotification inboundNotification)
        {
            if (inboundNotification == null)
                throw new ArgumentNullException(nameof(inboundNotification));
            var lines = new List<InboundNotification.Line>();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "select * from links.v_inbound_notification_line inml where inml.inbound_id = :inbound_id";
                command.Parameters.Add(new OracleParameter("inbound_id", inboundNotification.InboundId));
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
                command.CommandText = "select * from links.v_inbound_notification inm  where inm.inbound_id = :inbound_id";
                command.Parameters.Add(new OracleParameter("inbound_id", inboundId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        inboundNotification = CreateInboundNotification(reader);
                        var lines = GetInboundNotificationLines(inboundNotification);
                        inboundNotification.AddLines(lines);
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
                command.CommandText = "select * from links.v_inbound_notification inm " + "\n"
                    + "where inm.partner_inbound_id = :partner_inbound_id and inm.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_inbound_id", partnerInboundId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        inboundNotification = CreateInboundNotification(reader);
                        var lines = GetInboundNotificationLines(inboundNotification);
                        inboundNotification.AddLines(lines);
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
                var sql = new StringBuilder("select * from links.v_inbound_notification inm");
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
                        inboundNotification.AddLines(GetInboundNotificationLines(inboundNotification));
                        inboundNotifications.Add(inboundNotification);
                    }
                    reader.Close();
                }
            }
            return inboundNotifications.ToArray();
        }
        #endregion

        #region Outbound instruction and notification
        public void PostOutboundInstruction(OutboundInstruction outboundInstruction)
        {
            if (outboundInstruction == null)
                throw new ArgumentNullException("outboundInstruction");
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "";
                command.ExecuteNonQuery();
            }
        }
        private OutboundNotification.Line CreateOutboundNotificationLine(DbDataReader reader)
        {
            var product = GetProduct(Convert.ToString(reader["product_id"]));
            var planQuantity = Convert.ToDouble(reader["plan_quantity"]);
            var factQuantity = Convert.ToDouble(reader["fact_quantity"]);
            var line = new OutboundNotification.Line(-1, product, planQuantity);
            line.FactQuantity = factQuantity;
            return line;
        }
        private OutboundNotification CreateOutboundNotification(DbDataReader reader)
        {
            var message = new OutboundNotification(-1, 1);
            message.OutboundId = Convert.ToString(reader["outbound_id"]);
            message.OutboundNumber = Convert.ToString(reader["outbound_number"]);
            message.PartnerOutboundNumber = Convert.ToString(reader["partner_outbound_number"]);
            message.State = Convert.ToString(reader["state_code"]);

            var partner = GetPartner(Convert.ToString(reader["partner_id"]));
            message.Sender = Tslog;
            message.Recipient = partner;
            message.ServiceSeller = Tslog;
            message.ServiceBuyer = partner;
            message.Shipper = Tslog;
            message.Receiver = partner;

            return message;
        }
        private OutboundNotification.Line[] GetOutboundNotificationLines(OutboundNotification outboundNotification)
        {
            if (outboundNotification == null)
                throw new ArgumentNullException(nameof(outboundNotification));
            var lines = new List<OutboundNotification.Line>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "select * from v_outbound_notification_line onml where onml.outbond_id = :outbond_id";
                command.Parameters.Add(new OracleParameter("outbond_id", outboundNotification.OutboundId));
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
                command.CommandText = "select * from liks.v_outbound_notification onm where onm.outbound_id = :outbound_id";
                command.Parameters.Add(new OracleParameter("outbound_id", outboundId));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        outboundNotification = CreateOutboundNotification(reader);
                        var lines = GetOutboundNotificationLines(outboundNotification);
                        outboundNotification.AddLines(lines);
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
                command.CommandText = "select * from liks.v_outbound_notification onm" + "\n"
                    + "where onm.partner_outbound_id = :partner_outbound_id and onm.partner_id = :partner_id";
                command.Parameters.Add(new OracleParameter("partner_outbound_id", partnerOutboundId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        outboundNotification = CreateOutboundNotification(reader);
                        var lines = GetOutboundNotificationLines(outboundNotification);
                        outboundNotification.AddLines(lines);
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
                var sql = new StringBuilder("select * from links.v_outbound_notification onm");
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
                    outboundNotification.AddLines(lines);
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

        public ProductsList GetProductsList(LegalEntity partner)
        {
            var products = new ProductsList(-1, 1);
            using (var command = base._connection.CreateCommand())
            {
                // TODO: check if there is more efficient and not breakable way
                command.CommandText = "select * ecco.v_product p where p.partner_id = :partner_id"; 
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToString(reader["id"]);
                        var product = new Product(-1, id);

                        product.Name = Convert.ToString(reader["name"]);
                        //product.Group = Convert.ToString(reader["ArtGrpNam"]);
                        product.Gtin = Convert.ToString(reader["gtin"]);

                        product.Netto = Convert.ToInt32(reader["net_weight"]);
                        product.Brutto = Convert.ToInt32(reader["gross_weight"]);
                        product.UnitWeight = Convert.ToString(reader["weight_measure"]);

                        product.Length = Convert.ToInt32(reader["length"]);
                        product.Width = Convert.ToInt32(reader["width"]);
                        product.Heigth = Convert.ToInt32(reader["height"]);
                        product.UnitLinerial = Convert.ToString(reader["length_measure"]);

                        product.Volume = Convert.ToInt32(reader["volume"]);
                        product.UnitVolume = Convert.ToString(reader["volume_measure"]);

                        //tmpProduct.UnitType = Convert.ToString(reader["ArtMgEh"]);
                        //product.MaxAmountPerPalet = Convert.ToInt32(r["ArtLttMax"]); // ArtLtt
                        //product.AmountPerPackage = Convert.ToInt32(r["ArtLttMgPack"]); // ArtLtt
                        //tmpProduct.ExpirationDateInDays = Convert.ToInt32(reader["ARTMHDTAGEWEDEFLT"]);
                        //tmpProduct.Price = reader["ARTWERTVK"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ARTWERTVK"]);
                        //
                        product.Note = Convert.ToString(reader["note1"]);
                        //tmpProduct.Info1 = Convert.ToString(reader["ArtInfo1"]);
                        //tmpProduct.Info2 = Convert.ToString(reader["ArtInfo2"]);
                        //tmpProduct.Info3 = Convert.ToString(reader["ArtInfo3"]);
                        //tmpProduct.Info4 = Convert.ToString(reader["ArtInfo4"]);
                        //tmpProduct.Info5 = Convert.ToString(reader["ArtInfo5"]);
                        //tmpProduct.Info6 = Convert.ToString(reader["ArtInfo6"]);
                        //
                        products.AddProduct(product);

                    }
                    reader.Close();
                }
            }
            return products;
        }
        public ClientsList GetClientsList(bool loadPartners = false) // TODO: to delete (just for debug)
        {
            var clients = new ClientsList(-1, 1);
            using (var command = base._connection.CreateCommand())
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var client = new LegalEntity(-1, "");
                        clients.AddClient(client);
                    }
                    reader.Close();
                }
            }
            return clients;
        }
    }
}
