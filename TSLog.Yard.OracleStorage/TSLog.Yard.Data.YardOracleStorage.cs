using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Serilog.Core;
using System.Data.Common;
using System.Dynamic;
using System.Text;
using TSLog.BossControl.BO;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Common.Data;
using TSLog.ExceptionHandler;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Products.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Data
{
    /// <summary>
    /// Класс для работ с даннми в БД Oracle
    /// </summary>
    public class YardOracleStorage : OracleStorage, IYardWmsStorage
    {
        // Средее время (ПО УМОЛЧАНИЮ) погрузки паллеты/коробки/штуки
        private const int PalletLoadTime = 93;
        private const int BoxLoadTime = 20;
        private const int PieceLoadTime = 25;

        private const int DefaultMaxRowsCount = 16;
        private const string DefaultStaffSchema = "staff";
        private const string DefaultLegalEntitiesSchema = "legal_entities";
        private const string DefaultPartnersSchema = "partners";
        private const string DefaultProductsSchema = "products";
        //private readonly ILogger<YardOracleStorage> _logger;
        /// <summary>
        /// Максимальное количество строк для вывода
        /// </summary>
        public int MaxRowsCount { protected get; set; } = DefaultMaxRowsCount;
        /// <summary>
        /// Схема для объектов учёта персонала
        /// </summary>
        public string StaffSchema { protected get; set; } = DefaultStaffSchema;
        /// <summary>
        /// Схема для организаций (юр. лиц)
        /// </summary>
        public string LegalEntitiesSchema { protected get; set; } = DefaultLegalEntitiesSchema;
        /// <summary>
        /// Схема для партнёров (покупателей наших складских и логистических услуг)
        /// </summary>
        public string PartnersSchema { protected get; set; } = DefaultPartnersSchema;
        //public YardOracleStorage(string dataSource, ILogger<YardOracleStorage> logger) : base(dataSource)
        //{
        //    _logger=logger;
        //}
        public YardOracleStorage(string dataSource) : base(dataSource)
        {}

        #region Products
        public ProductCatalog<Product> Products { private get; set; } = new ProductCatalog<Product>("PRODUCTS_CATALOG", "Справочник товаров");
        private static Product CreateProduct(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var product = new Product(Utils.DbValueToString(reader["id"]),
                Utils.DbValueToString(reader["code"]),
                Utils.DbValueToString(reader["gtin"]),
                Utils.DbValueToString(reader["name"]),
                Utils.DbValueToString(reader["note1"]));

            product.Length = Utils.DbValueToDouble(reader["length"]);
            product.Width = Utils.DbValueToDouble(reader["width"]);
            product.Height = Utils.DbValueToDouble(reader["height"]);
            product.UnitLinerial = Utils.DbValueToString(reader["length_measure"]);
            product.NetWeight = Utils.DbValueToDouble(reader["net_weight"]);
            product.GrossWeight = Utils.DbValueToDouble(reader["gross_weight"]);
            product.UnitWeight = Utils.DbValueToString(reader["weight_measure"]);
            product.Volume = Utils.DbValueToDouble(reader["volume"]);
            product.UnitVolume = Utils.DbValueToString(reader["volume_measure"]);
            product.Price = Utils.DbValueToDouble(reader["price"]);

            return product;
        }
        /// <summary>
        /// Возвращает товар по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Товар</returns>
        /// <exception cref="ProductNotFoundException"></exception>
        public Product GetProduct(string id)
        {
            id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
            var product = Products.GetByInternalCode(id);
            if (product != null && product != Product.Null)
                return product;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    $"select p.id, p.code, p.gtin,\n" +
                    $"       p.name,\n" +
                    $"       p.note1," +
                    $"       p.length, p.width, p.height, p.length_measure,\n" +
                    $"       p.net_weight, p.gross_weight, p.weight_measure,\n" +
                    $"       p.volume, p.volume_measure,\n" +
                    $"       p.price\n" +
                    $"  from {CommonSchema}.prod_product#v p where p.id = :id";
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    product = CreateProduct(reader);
                    Products.Add(product);
                }
                reader.Close();
            }
            if (product != null && product != Product.Null && product != Product.Empty)
                return product;
            throw new ProductNotFoundException(id);
        }
        /// <summary>
        /// Возвращает товар по GTIN (EAN) номеру
        /// </summary>
        /// <param name="gtin">GTIN (EAN) номер</param>
        /// <returns>Товар</returns>
        /// <exception cref="ProductNotFoundException"></exception>
        public Product GetProductByGtin(string gtin)
        {
            gtin = !string.IsNullOrWhiteSpace(gtin) ? gtin.Trim() : string.Empty;
            var product = Products[gtin];
            if (product != null && product != Product.Null && product != Product.Empty)
                return product;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    $"select p.id, p.code, p.gtin,\n" +
                    $"       p.name,\n" +
                    $"       p.note1," +
                    $"       p.length, p.width, p.height, p.length_measure,\n" +
                    $"       p.net_weight, p.gross_weight, p.weight_measure,\n" +
                    $"       p.volume, p.volume_measure,\n" +
                    $"       p.price\n" +
                    $"  from {CommonSchema}.prod_product#v p where p.gtin = :gtin";
                command.Parameters.Add(new OracleParameter("gtin", gtin));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    product = CreateProduct(reader);
                    Products.Add(product);
                }
                reader.Close();
            }
            if (product != null && product != Product.Null && product != Product.Empty)
                Products.Add(product);
            throw new ProductNotFoundException(gtin);
        }
        /// <summary>
        /// Возвращает товар по идентификатору в системе партнёра (клиента)
        /// </summary>
        /// <param name="partner">Партнёр (клиент)</param>
        /// <param name="partnerProductId">Идентификатор в системе партнёра</param>
        /// <returns>Товар</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Product GetProduct(Partner partner, string partnerProductId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            if (string.IsNullOrWhiteSpace(partnerProductId))
                throw new ArgumentException("partner product id is empty");
            return Product.Null;
        }
        /// <summary>
        /// Возвращает ВСЕ товраы, которые относятся к указанным мандантам (клиентам), если не указан ни один клиент, то возвращает вообще ВСЕ товары
        /// </summary>
        /// <param name="partners">Клиенты (манданты)</param>
        /// <returns>Массиы товаров</returns>
        public Product[] GetProducts(Partner[]? partners = null)
        {
            var products = new List<Product>();
            using (var command = _connection.CreateCommand())
            {
                var sql = new StringBuilder(
                    $"select p.id, p.code, p.gtin,\n" +
                    $"       p.name,\n" +
                    $"       p.note1," +
                    $"       p.length, p.width, p.height, p.length_measure,\n" +
                    $"       p.net_weight, p.gross_weight, p.weight_measure,\n" +
                    $"       p.volume, p.volume_measure,\n" +
                    $"       p.price\n" +
                    $"from {CommonSchema}.prod_product#v p");
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
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var product = CreateProduct(reader);
                    Products.Add(product);
                }
                reader.Close();
            }
            return products.ToArray();
        }
        #endregion

        #region Warehouse actions
        private LineBase CreateWarehouseActionLine(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var productId = Utils.DbValueToString(reader["product_id"]);
            var product = GetProduct(productId);
            var plaQuantity = Utils.DbValueToDouble(reader["plan_quantity"]);
            var factQuantity = Utils.DbValueToDouble(reader["fact_quantity"]);
            var quantityMeasureUnitCode = Utils.DbValueToString(reader["quantity_measure_unit_code"]);
            var quantityMeasureunit = GetMeasureUnit(quantityMeasureUnitCode);
            var line = new LineBase(-1, product, plaQuantity, factQuantity, quantityMeasureunit);

            //line.PalletStandard = Utils.DbValueToDouble(reader["pallet_standard"]);
            //line.BoxStandard = Utils.DbValueToDouble(reader["box_standard"]);

            line.PlanNetWeight = Utils.DbValueToDouble(reader["plan_net_weight"]);
            line.PlanGrossWeight = Utils.DbValueToDouble(reader["plan_gross_weight"]);
            line.FactNetWeight = Utils.DbValueToDouble(reader["fact_net_weight"]);
            line.FactGrossWeight = Utils.DbValueToDouble(reader["fact_gross_weight"]);
            var weightMeasureUnitCode = Utils.DbValueToString(reader["weight_measure_unit_code"]);
            var weightMeasureUnit = GetMeasureUnit(weightMeasureUnitCode);
            line.WeightMeasureUnit = weightMeasureUnit;

            line.PlanVolume = Utils.DbValueToDouble(reader["plan_volume"]);
            line.FactVolume = Utils.DbValueToDouble(reader["fact_volume"]);
            var volumeMeasureUnitCode = Utils.DbValueToString(reader["volume_measure_unit_code"]);
            var volumeMeasureUnit = GetMeasureUnit(volumeMeasureUnitCode);
            line.VolumeMeasureUnit = volumeMeasureUnit;

            line.PlanPalletsCount = Utils.DbValueToDouble(reader["plan_pallets_count"]);
            line.PlanBoxesCount = Utils.DbValueToDouble(reader["plan_boxes_count"]);
            line.FactPalletsCount = Utils.DbValueToDouble(reader["fact_pallets_count"]);
            line.FactBoxesCount = Utils.DbValueToDouble(reader["fact_boxes_count"]);

            return line;
        }
        private WarehouseAction CreateWarehouseAction(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var kind = Convert.ToString(reader["kind"]) == "O" ? ActionKind.Outgo : ActionKind.Income;
            var id = Utils.DbValueToString(reader["id"]);
            var number = string.Empty;
            var partner = GetPartner(Utils.DbValueToString(reader["partner_id"]));
            var partnerActionId = Utils.DbValueToString(reader["partner_number"]);
            var isActive = true;
            var note = string.Empty;
            var planActionDate = Utils.DbValueToDateTime(reader["plan_action_date"]);
            var insertDate = Utils.DbValueToDateTime(reader["insert_date"]);
            var action = new WarehouseAction(kind,
                id,
                number,
                partner,
                partnerActionId,
                planActionDate,
                insertDate,
                isActive,
                note);
            action.TusCount = Utils.DbValueToInt(reader["tu_count"]);
            return action;
        }
        /// <summary>
        /// Возвращает товарную позицию складского действа по виду складского действа, идентификатору складского действа и номеру позиции
        /// </summary>
        /// <param name="actionKind">Вид складского действа (приход, расход)</param>
        /// <param name="actionId">Идентификатор складского действа</param>
        /// <param name="number">Номер товарной позиции</param>
        /// <returns>Товарная позиция</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public LineBase GetWarehouseActionLine(ActionKind actionKind, string actionId, int number)
        {
            if (string.IsNullOrWhiteSpace(actionId))
                throw new ArgumentException("action id is empty");
            actionId = actionId.Trim();
            number = number > 0 ? number : -1;
            LineBase? line = null;
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"select *\n" +
                    $"  from {CommonSchema}.act_warehouse_action_line#v al\n" +
                    $" where al.action_id = :action_id\n" +
                    $"   and al.line_number = :line_number\n" +
                    $"   and al.action_kind = :action_kind";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                command.Parameters.Add(new OracleParameter("action_id", actionId));
                command.Parameters.Add(new OracleParameter("line_number", number));

                using var reader = command.ExecuteReader();
                if (reader.Read())
                    line = CreateWarehouseActionLine(reader);
                reader.Close();
            }
            if (line == null)
                throw new Exception();
            return line;
        }
        /// <summary>
        /// Возвращает товарные позиции складского действа (прихода, расхода, ...)
        /// </summary>
        /// <param name="actionKind">Вид складского действа (прихода, расхода, ...)</param>
        /// <param name="actionId">Идентификатор складского действа (прихода, расхода, ...)</param>
        /// <returns>Массив товарных позиций</returns>
        /// <exception cref="ArgumentException"></exception>
        public LineBase[] GetWarehouseActionLines(ActionKind actionKind, string actionId)
        {
            var lines = new List<LineBase>();
            if (string.IsNullOrWhiteSpace(actionId))
                throw new ArgumentException("action id is empty");
            actionId = actionId.Trim();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"select al.action_kind, al.action_id,\n" +
                    $"       al.line_number,\n" +
                    $"       al.product_id,\n" +
                    $"       al.plan_quantity, al.fact_quantity, al.quantity_measure_unit_code,\n" +
                    $"       al.plan_net_weight, al.plan_gross_weight, al.fact_net_weight, al.fact_gross_weight, al.weight_measure_unit_code,\n" +
                    $"       al.plan_volume, al.fact_volume, al.volume_measure_unit_code,\n" +
                    $"       al.plan_pallets_count, al.plan_boxes_count,\n" +
                    $"       al.fact_pallets_count, al.fact_boxes_count\n" +
                    $"  from {CommonSchema}.act_wa_line_ext#v al\n" +
                    $" where action_id = :action_id and action_kind = :action_kind";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("action_kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                command.Parameters.Add(new OracleParameter("action_id", actionId));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var line = CreateWarehouseActionLine(reader);
                    lines.Add(line);
                }
                reader.Close();
            }
            return lines.ToArray();
        }
        /// <summary>
        /// Возвращает товарные позиции складского действа (прихода, расхода, ...)
        /// </summary>
        /// <param name="action">Складское действо</param>
        /// <returns>Массив товарных позиций</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LineBase[] GetWarehouseActionLines(WarehouseAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return GetWarehouseActionLines(action.Kind, action.Id);
        }
        /// <summary>
        /// Возвращает складское дейтсво по виду действа (приход, расход) и идентификатору
        /// </summary>
        /// <param name="actionKind">Вид складского действа (приход, расход)</param>
        /// <param name="id">Идентификатор</param>
        /// <returns>Складское действо</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WarehouseActionNotFoundException"></exception>
        public WarehouseAction GetWarehouseAction(ActionKind actionKind, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("action id is empty");
            id = id.Trim();
            var action = WarehouseAction.Null;
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var selectWarehouseActionByIdSql =
                    $"select a.kind, a.id, a.partner_id, a.partner_number, a.plan_action_date, a.insert_date, a.tu_count\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where a.id = :id\n" +
                    $"   and a.kind = :kind";
                command.CommandText = selectWarehouseActionByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                command.Parameters.Add(new OracleParameter("kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    action = CreateWarehouseAction(reader);
                reader.Close();

                // Если не нашли складское действо, то пробуем найти его в архиве или в удалённых.
                if (action == null || action == WarehouseAction.Null || action == WarehouseAction.Empty)
                {
                    var selectArchivedWarehouseDeletedActionByIdSql =
                        $"select a.kind, a.id, a.partner_id, a.partner_number, a.plan_action_date, a.insert_date, a.tu_count\n" +
                        $"  from {CommonSchema}.act_warehouse_action_ad#v a\n" +
                        $" where a.id = :id\n" +
                        $"   and a.kind = :kind";
                    command.CommandText = selectArchivedWarehouseDeletedActionByIdSql;
                    command.Parameters.Add(new OracleParameter("id", id));
                    command.Parameters.Add(new OracleParameter("kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                    using var reader2 = command.ExecuteReader();
                    if (reader2.Read())
                        action = CreateWarehouseAction(reader2);
                    reader2.Close();
                }
            }
            if (action != null && action != WarehouseAction.Null && action != WarehouseAction.Empty)
                return action;

            throw new WarehouseActionNotFoundException(id);
        }
        /// <summary>
        /// Возвращает складское действо по виду складского действа (приход, расход), клиенту (манданту) и идентификатору в системе клиента
        /// </summary>
        /// <param name="actionKind">Вид (приход, расход)</param>
        /// <param name="partner">Клиент (мандант)</param>
        /// <param name="partnerActionId">Идентификатор в системе клиента</param>
        /// <returns>Складское действо</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="WarehouseActionNotFoundException"></exception>
        public WarehouseAction GetWarehouseAction(ActionKind actionKind, Partner partner, string partnerActionId)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            partnerActionId = partnerActionId.Trim();
            var action = WarehouseAction.Null;
            using (var command = _connection.CreateCommand())
            {
                var selectWarehouseActionByPartnerActionIdSql =
                    $"select a.kind, a.id, a.partner_id, a.partner_number, a.plan_action_date, a.insert_date, a.tu_count\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where a.partner_number = :partner_number\n" +
                    $"   and a.partner_id = :partner_id\n" +
                    $"   and a.kind = :kind";
                command.CommandText = selectWarehouseActionByPartnerActionIdSql;
                command.Parameters.Add(new OracleParameter("partner_number", partnerActionId));
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                command.Parameters.Add(new OracleParameter("kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    action = CreateWarehouseAction(reader);
                    //var lines = GetWarehouseActionLines(action);
                    //action.AddLines(lines);
                }
                reader.Close();

                // Если не нашли складское действо, то пробуем найти его в архиве или в удалённых.
                if (action == null || action == WarehouseAction.Null || action == WarehouseAction.Empty)
                {
                    var selectArchivedDeletedWarehouseActionByPartnerActionIdSql =
                        $"select a.kind, a.id, a.partner_id, a.partner_number, a.plan_action_date, a.insert_date, a.tu_count\n" +
                        $"  from {CommonSchema}.act_warehouse_action_ad#v a\n" +
                        $" where a.partner_number = :partner_number\n" +
                        $"   and a.partner_id = :partner_id\n" +
                        $"   and a.kind = :kind";
                    command.CommandText = selectArchivedDeletedWarehouseActionByPartnerActionIdSql;
                    command.Parameters.Add(new OracleParameter("partner_number", partnerActionId));
                    command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                    command.Parameters.Add(new OracleParameter("kind", actionKind == ActionKind.Outgo ? "O" : "I"));
                    using var reader2 = command.ExecuteReader();
                    if (reader2.Read())
                        action = CreateWarehouseAction(reader2);
                    reader2.Close();
                }
            }
            if (action != null && action != WarehouseAction.Null && action != WarehouseAction.Empty)
                return action;
            throw new WarehouseActionNotFoundException(partner.Id, partnerActionId);
        }
        /// <summary>
        /// Ищет складское действа по виду складского действа (приход, расход), партнёру и "шаблону" идентификатора/номера/клиентского идентификатора
        /// </summary>
        /// <param name="kind">Вид (приход, расход)</param>
        /// <param name="partner">Клиент (мандант)</param>
        /// <param name="pattern">Шаблон идентификатора/номера/клиентского идентификатора</param>
        /// <returns>Массив складских действ</returns>
        public WarehouseAction[] FindWarehouseActions(ActionKind kind, Partner? partner, string pattern, bool isPartnerUser = false)
        {
            var actions = new List<WarehouseAction>();
            pattern = !string.IsNullOrWhiteSpace(pattern) ? $"%{pattern.Trim().ToUpper()}%" : "%";

            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectWarehouseActionByPartnerActionIdSql = String.Empty;
                if (isPartnerUser)
                {
                    SelectWarehouseActionByPartnerActionIdSql =
                    $"select a.kind,\n" +
                    $"       a.id, a.action_number,\n" +
                    $"       a.partner_id,\n" +
                    $"       a.partner_number,\n" +
                    $"       a.state_code,\n" +
                    $"       a.plan_action_date,\n" +
                    $"       a.insert_date,\n" +
                    $"       a.tu_count\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where (upper(trim(a.id)) like :pattern\n" +
                    $"     or upper(trim(a.action_number)) like :pattern\n" +
                    $"     or upper(trim(a.partner_number)) like :pattern)\n" +
                    $"   and a.kind = :kind\n" +
                    $"   and a.state_code <= 100\n" +
                    $"   and a.state_code > 0\n" +
                    $"   and (:partner_id is null or a.partner_id = :partner_id)\n" +
                    $"   and rownum < {MaxRowsCount}";
                }
                else
                {
                    SelectWarehouseActionByPartnerActionIdSql =
                    $"select a.kind,\n" +
                    $"       a.id, a.action_number,\n" +
                    $"       a.partner_id,\n" +
                    $"       a.partner_number,\n" +
                    $"       a.state_code,\n" +
                    $"       a.plan_action_date,\n" +
                    $"       a.insert_date,\n" +
                    $"       a.tu_count\n" +
                    $"  from {CommonSchema}.act_warehouse_action#v a\n" +
                    $" where (upper(trim(a.id)) like :pattern\n" +
                    $"     or upper(trim(a.action_number)) like :pattern\n" +
                    $"     or upper(trim(a.partner_number)) like :pattern)\n" +
                    $"   and a.kind = :kind\n" +
                    $"   and a.state_code <= 100\n" +
                    $"   and (:partner_id is null or a.partner_id = :partner_id)\n" +
                    $"   and rownum < {MaxRowsCount}";
                }

                command.CommandText = SelectWarehouseActionByPartnerActionIdSql;
                command.Parameters.Add(new OracleParameter("kind", kind == ActionKind.Outgo ? "O" : "I"));
                command.Parameters.Add(new OracleParameter("pattern", pattern));
                command.Parameters.Add(new OracleParameter("partner_id",
                    partner != null && partner != Partner.Null && partner != Partner.Empty ? partner.Id : DBNull.Value));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var action = CreateWarehouseAction(reader);
                    //var lines = GetWarehouseActionLines(action);
                    //action.AddLines(lines);
                    actions.Add(action);
                }
                reader.Close();
            }
            return actions.ToArray();
        }
        /// <summary>
        /// Возвращает информацию по собранному расходу (заказу) - количество собранных паллет и штук
        /// </summary>
        /// <param name="id">Идкнтификатор расхода (заказа) = auf_t.aufid</param>
        /// <param name="palletsCount">Количество собранных паллет</param>
        /// <param name="eachesCount">Количество собранных штук</param>
        /// <exception cref="ArgumentException"></exception>
        public void GetAssembliedOutgo(string id, out double palletsCount, out double eachesCount)
        {
            palletsCount = 0.0;
            eachesCount = 0.0;
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("outgo id is empty!");

            // Пытаемся получить заказ, что бы проверить существование такого заказа
            GetWarehouseAction(ActionKind.Outgo, id);

            id = id.Trim();
            using var command = _connection.CreateCommand();
            command.BindByName = true;
            var sql =
                $"select o.order_id id,\n" +
                $"       o.pallets_count,\n" +
                $"       o.eaches_count\n" +
                $"  from {CommonSchema}.o_assambled_outgo#v o\n" +
                $" where o.order_id = :id";
            command.CommandText = sql;
            command.Parameters.Add(new OracleParameter("id", id));
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                palletsCount = Utils.DbValueToDouble(reader["pallets_count"]);
                eachesCount = Utils.DbValueToDouble(reader["eaches_count"]);
            }
            reader.Close();
        }
        /// <summary>
        /// Возвращает "ПЛАНОВОЕ ФАКТИЧЕСКОЕ" время отгрузки заказа (в зависимости от собранных позиций)
        /// </summary>
        /// <param name="outgoId">Идентификатор заказа (на отгрузку)</param>
        /// <param name="isPallet">Признак паллетной отгрузки</param>
        /// <param name="partner">Партнёр</param>
        /// <returns>Время отгрузки заказа в секундах</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public long GetOutgoPlanTime(string outgoId, bool isPallet, Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            long time = 0;
            List<string> tu_counter = new List<string>();
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"select tul.tul_outgo_id outgo_id, tul.quantity, tul.measure_unit_id, tul.tu_type_id, tul.tu_id, tul.place_id_prefix \n" +
                    $"  from {CommonSchema}.o_outgo_tu_line#v tul\n" +
                    $" where tul.tul_outgo_id = :outgo_id";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("outgo_id", outgoId));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var tuId = Utils.DbValueToString(reader["tu_id"]);
                    var placeIdPrefix = Utils.DbValueToString(reader["place_id_prefix"]);
                    var measureUnitId = Utils.DbValueToString(reader["measure_unit_id"]);
                    var tuTypeId = Utils.DbValueToString(reader["tu_type_id"]);
                    var quantity = Utils.DbValueToLong(reader["quantity"]);
                    if (placeIdPrefix == "AUS" || placeIdPrefix == "VRS" || placeIdPrefix == "TOR")
                    {
                        if (isPallet == true)
                        {
                            if (partner.Id == "0011" || partner.Id == "0017" || partner.Id == "0018")
                            {
                                if (partner.Id == "0011")
                                {
                                    if (measureUnitId == "ST" && (tuTypeId == "EURO_TSL_V" || tuTypeId == "EURO" || tuTypeId == "EURO_TSL_1"))
                                    {
                                        time += BoxLoadTime * quantity;
                                    }
                                    else
                                    {
                                        tu_counter.Add(tuId);
                                    }
                                }
                                else
                                {
                                    if (measureUnitId == "ST" && tuTypeId == "EURO_TSL_V")
                                    {
                                        time += BoxLoadTime * quantity;
                                    }
                                    else
                                    {
                                        tu_counter.Add(tuId);
                                    }
                                }
                            }
                            else
                            {
                                tu_counter.Add(tuId);
                            }
                        }
                        else
                        {
                            // #3153 Для коректного подсчета плановой расч. производительности товара клинета "Белнефтехим". 1 короб в среднем 25-30 кг. Вес в базе данных указан в граммах. 
                            if (partner.Id == "0007")
                            {
                                double boxQuantity = quantity/(30*1000);
                                time += BoxLoadTime * (long)Math.Round(boxQuantity);
                            }
                            else 
                            {
                                time += BoxLoadTime * quantity;
                            }
                            
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            if (tu_counter.Count != 0)
            {
                var uniqueTu = new HashSet<string>(tu_counter);
                time = uniqueTu.Count * PalletLoadTime;
            }
            return time;
        }
        /// <summary>
        /// Возвращает "ПЛАНОВОЕ ФАКТИЧЕСКОЕ" время приемки заказа 
        /// </summary>
        /// <param name="outgoId">Идентификатор заказа (на приемку)</param>
        /// <param name="isPallet">Признак паллетной приемки</param>
        /// <returns>Время приемки заказа в секундах</returns>
        public int GetIngoPlanTime(string outgoId, bool isPallet, Partner partner)
        {
            var time = 0;
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"select sum(PLAN_QUANTITY) AS quantity \n" +
                    $"from {CommonSchema}.V_INCOME_LINE vil\n" +
                    $"where vil.INCOME_ID = :outgoId";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("outgo_id", outgoId));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var quantity = Utils.DbValueToInt(reader["quantity"]);
                    if (isPallet == true)
                    {
                        time = PalletLoadTime * quantity;
                    }
                    else
                    {
                        time = BoxLoadTime * quantity;
                    }
                }
            }
            return time;
        }
        #endregion

        #region Legal entities
        private readonly Catalog<LegalEntity> _legalEntities = new("LEGAL_ENTITIES", "Организации");
        private static LegalEntity CreateLegalEntity(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToString(reader["id"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var diplayName = Utils.DbValueToString(reader["display_name"]);
            var info1 = string.Empty;
            var info2 = string.Empty;
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToLong(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var legalEntity = new LegalEntity(id,
                name, shortName, altName, diplayName, info1, info2,
                isActive,
                displayOrder,
                note);
            return legalEntity;
        }
        /// <summary>
        /// Возвращает юридическое лицо (gp_t) по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Юридическое лицо (gp_t)</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="LegalEntityNotFoundException"></exception>
        public LegalEntity GetLegalEntity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("id is empty");
            id = id.Trim().ToUpper();
            var legalEntity = _legalEntities[id];
            if (legalEntity != null && legalEntity != Partner.Null && legalEntity != LegalEntity.Empty)
                return legalEntity;
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"select le.id,\n" +
                    $"       le.name, le.short_name, le.alt_name, le.display_name,\n" +
                    $"       le.is_active, le.display_order,\n" +
                    $"       le.note\n" +
                    $"  from {LegalEntitiesSchema}.le_legal_entity#v le\n" +
                    $" where upper(trim(le.id)) = :id";
                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    legalEntity = CreateLegalEntity(reader);
                    _legalEntities.Add(legalEntity);
                }
                reader.Close();
            }
            if (legalEntity != null && legalEntity != LegalEntity.Null && legalEntity != LegalEntity.Empty)
                return legalEntity;
            throw new LegalEntityNotFoundException(id);
        }
        #endregion

        #region Sites
        private readonly IDictionary<string, string> _siteCodes = new Dictionary<string, string>
        {
            { "EIN01", "GATE_1" },
            { "EIN02", "GATE_2" },
            { "EIN03", "GATE_3" },
            { "EIN04", "GATE_4" },
            { "EIN05", "GATE_5" },
            { "EIN06", "GATE_6" },
            { "EIN07", "HOOKAH" },
            { "EIN08", "GATE_8" },
            { "EIN09", "GATE_9" },
            { "EIN10", "GATE_10" },
            { "EIN11", "GATE_11" },
            { "EIN12", "GATE_12" },
            { "EIN13", "GATE_13" },
            { "EIN14", "GATE_14" },
            { "EIN15", "GATE_15" },
            { "EIN16", "GATE_16" },
            { "EIN17", "GATE_17" }
        };
        /// <summary>
        /// Возвращает код участка работ по коду ворот
        /// </summary>
        /// <param name="gateCode">Код ворот</param>
        /// <returns>Код участка</returns>
        public string GetSiteCodeByGateCode(string gateCode)
        {
            return !string.IsNullOrWhiteSpace(gateCode) && _siteCodes.ContainsKey(gateCode) ? _siteCodes[gateCode] : string.Empty;
        }
        private readonly Catalog<Site> _sites = new("SITES", "Участки");
        private static Site CreateSite(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToShort(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var kind = Utils.DbValueToString(reader["kind"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var site = new Site(id,
                code,
                kind,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                note);
            return site;
        }
        /// <summary>
        /// Возвращает участок работ по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Учаток работ</returns>
        /// <exception cref="SiteNotFoundException"></exception>
        public Site GetSite(short id)
        {
            var site = _sites[id];
            if (site != null && site != Site.Null && site != Site.Empty)
                return site;
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"select s.id," +
                    $"       s.code,\n" +
                    $"       s.kind,\n" +
                    $"       s.name, s.short_name, s.alt_name, s.display_name,\n" +
                    $"       s.is_active,\n" +
                    $"       s.display_order,\n" +
                    $"       s.parent_id,\n" +
                    $"       s.note\n" +
                    $"  from {StaffSchema}.stf_site#v s\n" +
                    $" where s.id = :id";
                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    site = CreateSite(reader);
                    _sites.Add(site);
                }
                reader.Close();
            }
            if (site != null && site != Site.Null && site != Site.Empty)
                return site;
            throw new SiteNotFoundException(id);
        }
        /// <summary>
        /// Возвращает ВСЕ участки работ
        /// </summary>
        /// <returns>Массив участков работ</returns>
        public Site[] GetSites()
        {
            var sites = new List<Site>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"select s.id," +
                    $"       s.code,\n" +
                    $"       s.kind,\n" +
                    $"       s.name, s.short_name, s.alt_name, s.display_name,\n" +
                    $"       s.is_active,\n" +
                    $"       s.display_order,\n" +
                    $"       s.parent_id,\n" +
                    $"       s.note\n" +
                    $"  from {StaffSchema}.stf_site#v s";
                command.CommandText = SelectPartnerByIdSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var site = CreateSite(reader);
                    sites.Add(site);
                }
                reader.Close();
            }
            return sites.ToArray();
        }
        #endregion

        #region Partners
        private readonly Catalog<Partner> _partners = new("PARTNERS", "Партнёры");
        private static Partner CreatePartner(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToString(reader["id"]);
            var name = Utils.DbValueToString(reader["name"]);
            var isActive = Utils.DbValueToLong(reader["display_order"]) < 9999;
            var displayOrder = Utils.DbValueToLong(reader["display_order"]);
            var partner = new Partner(id,
                name,
                name, // note
                string.Empty,
                string.Empty,
                isActive,
                displayOrder);
            return partner;
        }
        /// <summary>
        /// Возвращает клиента (мандант) по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Клиент (мандант)</returns>
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
        /// <summary>
        /// Возвращает менеджеров указанного клиента (манданта)
        /// </summary>
        /// <param name="partner">Клиент (мандант)</param>
        /// <returns>Массив менеджеров (сотрудников)</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Employee[] GetPartnerManagers(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));

            var managers = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerManagersSql =
                    $"select pm.manager_id\n" +
                    $"  from {StaffSchema}.stf_partner_manager#v pm\n" +
                    $" where pm.partner_id = :partner_id";
                command.CommandText = SelectPartnerManagersSql;
                command.Parameters.Add(new OracleParameter("partner_id", partner.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var managerId = Utils.DbValueToInt(reader["manager_id"]);
                    var manager = GetEmployee(managerId);
                    managers.Add(manager);
                }
                reader.Close();
            }
            return managers.ToArray();
        }
        /// <summary>
        /// Возвращает всех или только активых клиентов (мандантов)
        /// </summary>
        /// <param name="getOnlyActive">Признак - "только активных"</param>
        /// <returns>Массив клиентов (мандантов)</returns>
        public Partner[] GetPartners(bool getOnlyActive = true)
        {
            var partners = new List<Partner>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnersSql =
                    $"select p.id, p.name, p.display_order\n" +
                    $"  from {PartnersSchema}.par_partner#v p";
                command.CommandText = SelectPartnersSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var partner = CreatePartner(reader);
                    if (!getOnlyActive || partner.IsActive)
                        partners.Add(partner);
                }
                reader.Close();
            }
            partners.Sort();
            return partners.ToArray();
        }
        #endregion

        #region Shifts
        private const string DefaultShift = "5/2-40";
        private static Shift CreateShift(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            return new Shift(-1, DateTime.Today, DateTime.Today, "I");
        }
        /// <summary>
        /// Возвращает тип (номер) смены, которая попадает на указанный момент времени
        /// </summary>
        /// <param name="date">Дата (+ время)</param>
        /// <returns>Номер смены</returns>
        private string? GetShiftKind(DateTime? date = null)
        {
            var shift = DefaultShift;
            using (var command = _connection.CreateCommand())
            {
                var SelectShiftSql =
                    $"select {StaffSchema}.api_staff.get_shift(:p_date) shift from dual";
                command.CommandText = SelectShiftSql;
                command.Parameters.Add(new OracleParameter("p_date", date != null && date != DateTime.MinValue
                    ? date : DBNull.Value));
                var result = command.ExecuteScalar();
                shift = result != null ? Convert.ToString(result) : DefaultShift;
            }
            return shift;
        }
        /// <summary>
        /// Возвращает конкретную рабочую смену по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Рабочая смена</returns>
        public Shift GetShift(int id)
        {
            return new Shift(id, DateTime.Today, DateTime.Today, "I"); ;
        }
        /// <summary>
        /// Возвращает конкретную рабочую смену по попаданию в эту смену указанного момента времени
        /// </summary>
        /// <param name="date">Дата (+ время)</param>
        /// <returns>Конкретная рабочая смена</returns>
        public Shift GetShift(DateTime date)
        {
            return new Shift(-1, date, date, "I");
        }
        #endregion

        #region Employees
        private readonly Catalog<Employee> _employees = new("EMPLOYEES", "Сотрудники");
        private Employee CreateEmployee(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt64(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var surname = Utils.DbValueToString(reader["surname"]);
            var firstName = Utils.DbValueToString(reader["first_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var birthDate = Utils.DbValueToDateTime(reader["birth_date"]);
            var companyId = Utils.DbValueToString(reader["company_id"]);
            var company = GetLegalEntity(companyId);
            var note = Utils.DbValueToString(reader["note"]);

            var employee = new Employee(id, code,
                surname, firstName,
                birthDate,
                company,
                isActive,
                note);
            employee.Patronymic = Utils.DbValueToString(reader["patronymic"]);
            employee.WmsLogin = Utils.DbValueToString(reader["wms_login"]);
            employee.AdLogin = Utils.DbValueToString(reader["ad_login"]);
            employee.Sex = Utils.DbValueToString(reader["sex"]);
            employee.Nationality = Utils.DbValueToString(reader["nationality"]);
            employee.PhoneNumber = Utils.DbValueToString(reader["phone_number"]);
            employee.Email = Utils.DbValueToString(reader["email"]);

            employee.DocumentName = Utils.DbValueToString(reader["document_name"]);
            employee.DocumentSeries = Utils.DbValueToString(reader["document_series"]);
            employee.DocumentNumber = Utils.DbValueToString(reader["document_number"]);
            employee.DocumentIssueDate = Utils.DbValueToDateTime(reader["document_issue_date"]);
            employee.DocumentIssueOrganization = Utils.DbValueToString(reader["document_issue_organization"]);

            employee.EmploymentDate = Utils.DbValueToDateTime(reader["employment_date"]);
            employee.DismissalDate = Utils.DbValueToDateTime(reader["dismissal_date"]);

            employee.WorkMode = Utils.DbValueToString(reader["work_mode"]);

            //var departmentId = Utils.DbValueToInt(reader["department_id"]);
            //employee.Department = GetDepartment(departmentId);

            var positionId = Utils.DbValueToShort(reader["position_id"]);
            employee.MajorPosition = positionId > 0 ? GetPosition(positionId) : Position.Null;

            employee.Cost = Utils.DbValueToDouble(reader["cost"]);
            var costCurrencyCode = Utils.DbValueToString(reader["cost_currency_code"]);
            employee.CostMeasureUnit = GetMeasureUnit(costCurrencyCode);

            return employee;
        }
        /// <summary>
        /// Возвращает сотрудника по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Сотрудник</returns>
        /// <exception cref="EmployeeNotFoundException"></exception>
        public Employee GetEmployee(int id)
        {
            id = id > 0 ? id : 0;
            var employee = _employees[id];
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.work_mode,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where e.id = :id";
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    employee = CreateEmployee(reader);
                    _employees.Add(employee);
                }
                reader.Close();
            }
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            throw new EmployeeNotFoundException(id);
        }
        /// <summary>
        /// Возвращает сотрудника по имени пользователя active directory
        /// </summary>
        /// <param name="login">Имя пользователя active directory</param>
        /// <returns>Сотрудник</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EmployeeNotFoundException"></exception>
        public Employee GetEmployeeByAdLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login is empty!");
            login = login.Trim().ToLower();
            var employees = _employees.Entities.ToList();
            //_logger.LogWarning("Отработал метод GetEmployeeByAdLogin с логином {Login} и массивом {employees} и переменной {_employees}", login, employees, _employees);
            var employee = employees.Find(e => e.AdLogin.Trim().ToLower() == login);
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var SelectEmployeeById =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.work_mode,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where e.ad_login = :ad_login";
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("ad_login", login));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    employee = CreateEmployee(reader);
                    _employees.Add(employee);
                }
                reader.Close();
            }
            if (employee != null && employee != Employee.Null && employee != Employee.Empty)
                return employee;
            throw new EmployeeNotFoundException("AD login", login);
        }
        /// <summary>
        /// Ищет сотрудников по "фаблону" Ф. И. О. (полному или краткому)
        /// </summary>
        /// <param name="pattern">Шаблон поиска</param>
        /// <returns>Массов сотрудников</returns>        
        public Employee[] FindEmployees(string pattern)
        {
            pattern = string.IsNullOrWhiteSpace(pattern) ? "%" : $"%{pattern.ToUpper()}%";
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var FindEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, e.document_issue_date, e.document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.work_mode,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee_ext#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where upper(trim(e.surname)) like :pattern\n" +
                    $"       or upper(trim(e.first_name)) like :pattern\n" +
                    $"       or upper(trim(e.patronymic)) like :pattern\n" +
                    $"       or upper(trim(e.short_name)) like :pattern\n" +
                    $"       or upper(trim(e.short_name2)) like :pattern\n" +
                    $"       or upper(trim(e.short_name3)) like :pattern\n" +
                    $"       or upper(trim(e.short_name4)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name2)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name3)) like :pattern\n" +
                    $"       or upper(trim(e.alt_name4)) like :pattern\n" +
                    $"       or upper(trim(e.full_name)) like :pattern\n" +
                    $"       or upper(trim(e.full_name2)) like :pattern";
                command.CommandText = FindEmployeesSql;
                command.Parameters.Add(new OracleParameter("pattern", pattern));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        /// <summary>
        /// Ищет сотрудников по шаблону Ф. И. О., на определённой должности, по определёному режиму работы (номеру смены)
        /// </summary>
        /// <param name="position">Должность</param>
        /// <param name="pattern">Шаблон Ф. И. О.</param>
        /// <param name="workMode">Режим работы (номер смены)</param>
        /// <returns>Массив сотрудников</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Employee[] FindEmployees(Position position, string pattern, string workMode = "")
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            pattern = string.IsNullOrWhiteSpace(pattern) ? "%" : $"%{pattern.ToUpper()}%";
            var employees = new HashSet<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeeById =
                    $"select ep.employee_id\n" +
                    $"  from {StaffSchema}.stf_employee_position_ext#v ep\n" +
                    $" where ep.position_id = :position_id\n" +
                    $"   and ep.dismissal_date is null\n" +
                    $"   and (:work_mode is null or upper(trim(ep.work_mode)) = :work_mode)\n" +
                    $"   and (upper(trim(ep.employee_surname)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_first_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_patronymic)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name2)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name3)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_short_name4)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name2)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name3)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_alt_name4)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_full_name)) like :pattern\n" +
                    $"     or upper(trim(ep.employee_full_name2)) like :pattern)";
                command.BindByName = true;
                command.CommandText = SelectEmployeeById;
                command.Parameters.Add(new OracleParameter("position_id", position.Id));

                var workModeParameter = new OracleParameter
                {
                    ParameterName = "work_mode",
                    OracleDbType = OracleDbType.NVarchar2,
                    Value = !string.IsNullOrWhiteSpace(workMode) ? workMode.Trim().ToUpper() : DBNull.Value
                };

                command.Parameters.Add(new OracleParameter("pattern", pattern));
                command.Parameters.Add(workModeParameter);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employeeId = Utils.DbValueToInt(reader["employee_id"]);
                    var employee = GetEmployee(employeeId);
                    if (!employees.Contains(employee))
                        employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        /// <summary>
        /// Возвращает "линейных" ("линейнх" должностей) сотрудников
        /// </summary>
        /// <returns>Массис вотрудников</returns>
        public Employee[] GetLinearEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.work_mode,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id" +
                    //                    $"    where emp.is_linear = 1"; было изначально
                    $"    where POSITION_CODE IN ('WH_AREA_MANAGER', 'WH_SHIFT_HEAD', 'WH_OUTGO_SECTION_HEAD', 'WH_FOREMAN') OR emp.is_linear = 1 AND dismissal_date IS null";
                command.CommandText = SelectEmployeesSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        /// <summary>
        /// Возвращает ВСЕХ сотрудников
        /// </summary>
        /// <returns>Массив сотрудников</returns>
        public Employee[] GetEmployees()
        {
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.work_mode,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id";
                command.CommandText = SelectEmployeesSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        /// <summary>
        /// Возвращает сотрудников, которые должны работать по плану по указанному моменту времени
        /// </summary>
        /// <param name="date">Дата (+ время)</param>
        /// <returns>Массив сотрудников</returns>
        public Employee[] GetPlanShiftEmployees(DateTime? date = null)
        {
            var employees = new List<Employee>();
            var shift = GetShiftKind(date);
            using (var command = _connection.CreateCommand())
            {
                var SelectEmployeesSql =
                    $"   select e.id, e.code, e.wms_login, e.ad_login,\n" +
                    $"          e.surname, e.first_name, e.patronymic,\n" +
                    $"          e.sex, e.birth_date,\n" +
                    $"          e.nationality,\n" +
                    $"          e.phone_number, e.email,\n" +
                    $"          e.document_name, e.document_series, e.document_number, document_issue_date, document_issue_organization,\n" +
                    $"          e.employment_date, e.dismissal_date,\n" +
                    $"          e.work_mode,\n" +
                    $"          e.company_id,\n" +
                    $"          e.department_id,\n" +
                    $"          e.cost,\n" +
                    $"          e.cost_currency_code,\n" +
                    $"          e.is_active,\n" +
                    $"          e.display_order,\n" +
                    $"          emp.position_id,\n" +
                    $"          e.note\n" +
                    $"     from {StaffSchema}.stf_employee#v e\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on e.id = emp.employee_id\n" +
                    $"    where upper(trim(e.work_mode)) = :shift";
                command.CommandText = SelectEmployeesSql;
                command.Parameters.Add(new OracleParameter("shift", shift));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employee = CreateEmployee(reader);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        /// <summary>
        /// Возвращает сотрудников, работающих в указанную смену
        /// </summary>
        /// <param name="shift">Смена</param>
        /// <returns>Массив сотрудников</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Employee[] GetShiftEmployees(Shift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));
            var employees = new List<Employee>();
            using (var command = _connection.CreateCommand())
            {
                var SelectShiftEmployeesSql =
                    $"select se.id employee_id\n" +
                    $"  from {StaffSchema}.stf_shift_employee#v se\n" +
                    $" where se.shift_id = :shift_id";
                command.CommandText = SelectShiftEmployeesSql;
                command.Parameters.Add(new OracleParameter("shift_id", shift.Id));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var employeeId = Utils.DbValueToInt(reader["employee_id"]);
                    var employee = GetEmployee(employeeId);
                    employees.Add(employee);
                }
                reader.Close();
            }
            return employees.ToArray();
        }
        /// <summary>
        /// Возвращает массив сотрудников, работающих в смену по указанному моменту времени
        /// </summary>
        /// <param name="date">Дата (+ время)</param>
        /// <returns>Массив сотрудников</returns>
        public Employee[] GetShiftEmployees(DateTime date)
        {
            var shift = GetShift(date);
            return GetShiftEmployees(shift);
        }
        /// <summary>
        /// Возвращает работы из БоссКонтророля в промежутке дат (со временем) на указанном участке
        /// </summary>
        /// <param name="startDate">Начальная дата (+ время)</param>
        /// <param name="finishDate">Конечная дата (+ время)</param>
        /// <param name="siteCode">Код участка</param>
        /// <returns>Массив работ</returns>
        public dynamic[] GetBossControlWorks(DateTime startDate, DateTime finishDate, string siteCode)
        {
            var works = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"with dates as (\n" +
                    $"   select :start_date fact_work_start_date,\n" +
                    $"          :finish_date fact_work_finish_date\n" +
                    $"     from dual\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"    select emp.position_id, emp.position_name,\n" +
                    $"           d.worker_id, e.full_name worker_full_name,\n" +
                    $"           --greatest(d.start_date, dt.fact_work_start_date) start_date,\n" +
                    $"           d.start_date,\n" +
                    $"           greatest(d.start_date, dt.fact_work_start_date) clipped_start_date,\n" +
                    $"           --least(d.finish_date, dt.fact_work_finish_date) finish_date,\n" +
                    $"           d.finish_date,\n" +
                    $"           least(d.finish_date, dt.fact_work_finish_date) clipped_finish_date,\n" +
                    $"           --least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date) duration, -- Продолжительность\n" +
                    $"           d.finish_date - d.start_date duration, -- Продолжительность\n" +
                    $"           least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date) clipped_duration, -- Продолжительность\n" +
                    $"           d.site_id, s.name site_name\n" +
                    $"      from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works_wi(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $" left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $" left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $" left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"     where d.finish_date - d.start_date > 0\n" +
                    $"       and s.kind <> 'AUXILIARY'\n" +
                    $"       and (:site_code is null or s.code = :site_code)\n" +
                    $"       and s.kind = 'GATE' -- Только ворота\n" +
                    $"       and e.id is not null\n" +
                    $" )\n" +
                    $"    select d.position_id, d.position_name,\n" +
                    $"           d.worker_id, d.worker_full_name, \n" +
                    $"           d.site_id, d.site_name,\n" +
                    $"           d.start_date, d.finish_date,\n" +
                    $"           d.clipped_start_date, d.clipped_finish_date,\n" +
                    $"           round(d.duration * 24 * 60 * 60) duration,\n" +
                    $"           round(d.duration * 24 * 60 * 60) clipped_duration\n" +
                    $"      from data2 d\n" +
                    $"  order by d.position_id, d.worker_id, d.worker_full_name";

                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", startDate));
                command.Parameters.Add(new OracleParameter("finish_date", finishDate));
                command.Parameters.Add(new OracleParameter("site_code", !string.IsNullOrWhiteSpace(siteCode) ? siteCode : DBNull.Value));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dynamic work = new ExpandoObject();
                    work.workerId = Utils.DbValueToInt(reader["worker_id"]);
                    work.workerFullName = Utils.DbValueToString(reader["worker_full_name"]);
                    work.positionName = Utils.DbValueToString(reader["position_name"]);
                    work.startDate = Utils.DbValueToDateTime(reader["start_date"]);
                    work.clippedStartDate = Utils.DbValueToDateTime(reader["clipped_start_date"]);
                    work.finishDate = Utils.DbValueToDateTime(reader["finish_date"]);
                    work.clippedFinishDate = Utils.DbValueToDateTime(reader["clipped_finish_date"]);
                    work.duration = Utils.DbValueToDouble(reader["duration"]);
                    work.clippedDuration = Utils.DbValueToDouble(reader["clipped_duration"]);
                    works.Add(work);
                }
                reader.Close();
            }
            return works.ToArray();
        }
        /// <summary>
        /// Возвращает работы из БоссКонтророля в промежутке дат (со временем) на указанном участке (в виде объектов SiteWorker)
        /// </summary>
        /// <param name="startDate">Начальная дата (+ время)</param>
        /// <param name="finishDate">Конечная дата (+ время)</param>
        /// <param name="siteCode">Код участка</param>
        /// <returns>Массив обхектов SiteWorker</returns>
        public SiteWorker[] GetSitesWorkers(DateTime startDate, DateTime finishDate, string siteCode)
        {
            var works = new List<SiteWorker>();
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"with dates as (\n" +
                    $"   select :start_date fact_work_start_date,\n" +
                    $"          :finish_date fact_work_finish_date\n" +
                    $"     from dual\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"    select d.worker_id,\n" +
                    $"           d.start_date, d.finish_date,\n" +
                    $"           d.site_id,\n" +
                    $"           d.partner_id\n" +
                    $"      from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works_wi(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $" left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $" left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"     where d.finish_date - d.start_date > 0\n" +
                    $"       and s.kind <> 'AUXILIARY'\n" +
                    $"       and (:site_code is null or s.code = :site_code)\n" +
                    $"       and s.kind = 'GATE' -- Только ворота\n" +
                    $"       and e.id is not null\n" +
                    $" )\n" +
                    $"    select d.worker_id,\n" +
                    $"           d.site_id,\n" +
                    $"           d.partner_id,\n" +
                    $"           d.start_date, d.finish_date\n" +
                    $"      from data2 d";

                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", startDate));
                command.Parameters.Add(new OracleParameter("finish_date", finishDate));
                command.Parameters.Add(new OracleParameter("site_code", !string.IsNullOrWhiteSpace(siteCode) ? siteCode : DBNull.Value));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var siteId = Utils.DbValueToShort(reader["site_id"]);
                    var site = GetSite(siteId);
                    var workerId = Utils.DbValueToInt(reader["worker_id"]);
                    var worker = GetEmployee(workerId);
                    var partnerId = Utils.DbValueToString(reader["partner_id"]);
                    var partner = GetPartner(partnerId);
                    var workStartDate = Utils.DbValueToDateTime(reader["start_date"]);
                    var workFinishDate = Utils.DbValueToDateTime(reader["finish_date"]);
                    var work = new SiteWorker(site, partner, worker, workStartDate, workFinishDate);
                    works.Add(work);
                }
                reader.Close();
            }
            return works.ToArray();
        }
        #endregion

        #region Departments
        private readonly Catalog<Department> _departments = new("DEPARTMENTS", "Подразделения");
        private static Department CreateDepartment(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt16(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var note = Utils.DbValueToString(reader["note"]);
            var position = new Department(id, code,
                name, shortName, altName, displayName,
                isActive, displayOrder,
                LegalEntity.Null, Employee.Null,
                note);
            return position;
        }
        /// <summary>
        /// Возвращает подразделение по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Подразделение</returns>
        /// <exception cref="DepartmentNotFoundException"></exception>
        public Department GetDepartment(short id)
        {
            id = id > 0 ? id : (short)-1;
            var department = _departments[id];
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;

            using (var command = _connection.CreateCommand())
            {
                var SelectDepartmentByIdSql =
                    $"select d.id, d.code,\n" +
                    $"       d.name, d.short_name, d.alt_name, d.display_name,\n" +
                    $"       d.is_active, d.display_order,\n" +
                    $"       d.parent_id, d.company_id, d.gead_id,\n" +
                    $"       d.note\n" +
                    $"  from {StaffSchema}.stf_department#v d\n" +
                    $" where d.id = :id";
                command.CommandText = SelectDepartmentByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    department = CreateDepartment(reader);
                    _departments.Add(department);
                }
                reader.Close();
            }
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;
            throw new DepartmentNotFoundException(id);
        }
        /// <summary>
        /// Возвращает подразделение по коду
        /// </summary>
        /// <param name="code">Код</param>
        /// <returns>Подразделение</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DepartmentNotFoundException"></exception>
        public Department GetDepartment(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var department = _departments[code];
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;

            using (var command = _connection.CreateCommand())
            {
                var SelectDepartmentByCodeSql =
                    $"select d.id, d.code,\n" +
                    $"       d.name, d.short_name, d.alt_name, d.display_name,\n" +
                    $"       d.is_active, d.display_order,\n" +
                    $"       d.parent_id, d.company_id, d.gead_id,\n" +
                    $"       d.note\n" +
                    $"  from {StaffSchema}.stf_department#v d\n" +
                    $" where d.code = :code";
                command.CommandText = SelectDepartmentByCodeSql;
                command.Parameters.Add(new OracleParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    department = CreateDepartment(reader);
                    _departments.Add(department);
                }
                reader.Close();
            }
            if (department != null && department != Department.Null && department != Department.Empty)
                return department;
            throw new DepartmentNotFoundException(code);
        }
        /// <summary>
        /// Возвращает подразделения
        /// </summary>
        /// <returns>Массив подразделений</returns>
        public Department[] GetDepartments()
        {
            var departments = new List<Department>();
            using (var command = _connection.CreateCommand())
            {
                var SelectDepartmentsSql =
                    $"select d.id, d.code,\n" +
                    $"       d.name, d.short_name, d.alt_name, d.display_name,\n" +
                    $"       d.is_active, d.display_order,\n" +
                    $"       d.parent_id, d.company_id, d.gead_id,\n" +
                    $"       d.note\n" +
                    $"  from {StaffSchema}.stf_department#v d";
                command.CommandText = SelectDepartmentsSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var department = CreateDepartment(reader);
                    departments.Add(department);
                }
                reader.Close();
            }
            departments.Sort();
            return departments.ToArray();
        }
        #endregion

        #region Positions
        private readonly Catalog<Position> _positions = new("POSITIONS", "Должности");
        private Position CreatePosition(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Convert.ToInt16(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
            var isLinear = Utils.DbValueToShort(reader["is_linear"]) == 1;
            var note = Utils.DbValueToString(reader["note"]);
            var position = new Position(id,
                code,
                name,
                shortName,
                altName,
                displayName,
                isActive,
                displayOrder,
                isLinear,
                note)
            {
                LaborHourCost = Utils.DbValueToDouble(reader["cost"])
            };
            var costCurrencyCode = Utils.DbValueToString(reader["cost_currency_code"]);
            position.LaborHourCostCurrency = GetMeasureUnit(costCurrencyCode);
            return position;
        }
        /// <summary>
        /// Возвращает должность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Должность</returns>
        /// <exception cref="PositionNotFoundException"></exception>
        public Position GetPosition(short id)
        {
            id = id > 0 ? id : (short)-1;
            var position = _positions[id];
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;

            using (var command = _connection.CreateCommand())
            {
                var SelectPositionByIdSql =
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_position#v p\n" +
                    $" where p.id = :id";
                command.CommandText = SelectPositionByIdSql;
                command.Parameters.Add(new OracleParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    position = CreatePosition(reader);
                    _positions.Add(position);
                }
                reader.Close();
            }
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;
            throw new PositionNotFoundException(id);
        }
        /// <summary>
        /// Возвращает должность по коду
        /// </summary>
        /// <param name="code">Код</param>
        /// <returns>Должность</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="PositionNotFoundException"></exception>
        public Position GetPosition(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            code = code.Trim().ToUpper();
            var position = _positions[code];
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;

            using (var command = _connection.CreateCommand())
            {
                var SelectPositionByCodeSql =
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_position#v p\n" +
                    $" where p.code = :code";
                command.CommandText = SelectPositionByCodeSql;
                command.Parameters.Add(new OracleParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    position = CreatePosition(reader);
                    _positions.Add(position);
                }
                reader.Close();
            }
            if (position != null && position != Position.Null && position != Position.Empty)
                return position;
            throw new PositionNotFoundException(code);
        }
        /// <summary>
        /// Возвращает ВСЕ должности
        /// </summary>
        /// <returns>Массив должностей</returns>
        public Position[] GetPositions()
        {
            var positions = new List<Position>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPositionsSql =
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_position#v p";
                command.CommandText = SelectPositionsSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var position = CreatePosition(reader);
                    positions.Add(position);
                }
                reader.Close();
            }
            positions.Sort();
            return positions.ToArray();
        }
        /// <summary>
        /// Возвращает "линейные" должности (линейного персонала)
        /// </summary>
        /// <returns>Массив "линейных" должностей</returns>
        public Position[] GetLinePositions()
        {
            var positions = new List<Position>();
            using (var command = _connection.CreateCommand())
            {
                var sql =
                    $"select p.id, p.code,\n" +
                    $"       p.name, p.short_name, p.alt_name, p.display_name,\n" +
                    $"       p.is_active, p.display_order,\n" +
                    $"       p.is_linear,\n" +
                    $"       p.cost, p.cost_currency_code,\n" +
                    $"       p.note\n" +
                    $"  from {StaffSchema}.stf_line_staff#v p";
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var position = CreatePosition(reader);
                    positions.Add(position);
                }
                reader.Close();
            }
            return positions.ToArray();
        }
        #endregion

        #region Reporting
        private const int DefaultShiftLength = 12 * 60 * 60;
        /// <summary>
        /// Возвщает трудозатраты по БоссКотролю за период
        /// </summary>
        /// <param name="fromDate">Дата начала периода (+ время)</param>
        /// <param name="toDate">Дата окончания периода (+ время)</param>
        /// <returns>Массив объектов (мандант, должность, количество работников, трудозатраты)</returns>
        public dynamic[] GetBcLabocCosts(DateTime fromDate, DateTime toDate)
        {
            var lines = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                var SelectPartnerByIdSql =
                    $"   select--d.worker_id employee_id,\n" +
                    $"          --emp.employee_surname, emp.employee_first_name, emp.employee_patronymic,\n" +
                    $"          d.partner_id, p.name partner_name,\n" +
                    $"          emp.position_id, emp.position_name,\n" +
                    $"          --Количество работников(различных)\n" +
                    $"          count(distinct d.worker_id) workers_count,\n" +
                    $"          --d.start_date, d.finish_date,\n" +
                    $"          --Трудозатраты в человекочасах\n" +
                    $"          round(sum((d.finish_date - d.start_date) * 24), 2) labor_cost,\n" +
                    $"          --Нормализованные трудозатраты(суммарные / количество работников) в человекочасах\n" +
                    $"          round(sum((d.finish_date - d.start_date) * 24) / count(distinct d.worker_id), 2) norm_labor_cost\n" +
                    $"          --d.site_id\n" +
                    $"     from table({StaffSchema}.stf_get_all_works(:from_date, :to_date)) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                    $"    where d.site_id in (select id from {StaffSchema}.stf_site#v connect by prior id = parent_id start with code = 'GROUND_FLOOR')\n" +
                    $"      and emp.employee_id is not null\n" +
                    $" group by emp.position_id, emp.position_name,\n" +
                    $"          d.partner_id, p.name\n" +
                    $" order by d.partner_id, emp.position_id";

                command.CommandText = SelectPartnerByIdSql;
                command.Parameters.Add(new OracleParameter("from_date", fromDate));
                command.Parameters.Add(new OracleParameter("to_date", toDate));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dynamic line = new ExpandoObject();
                    line.PartnerId = Utils.DbValueToString(reader["partner_id"]);
                    line.PositionId = Utils.DbValueToShort(reader["position_id"]);
                    line.WorkersCount = Utils.DbValueToInt(reader["workers_count"]);
                    line.LaborCost = Utils.DbValueToDouble(reader["labor_cost"]);
                    lines.Add(line);
                }
                reader.Close();
            }
            return lines.ToArray();
        }
        /// <summary>
        /// Возвращает трудозатраты за смену
        /// </summary>
        /// <param name="date">Дата, по которой определяется смена (попадание даты в смену)</param>
        /// <returns>Массив объектов (должность, нормативное время работы, фактическое время работы)</returns>
        public dynamic[] GetBcShiftLabocCosts(DateTime date)
        {
            var lines = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"with data2 as (\n" +
                    $"   select {StaffSchema}.get_shift_start(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')) shift_start,\n" +
                    $"          {StaffSchema}.get_shift_finish(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')) shift_finish,\n" +
                    $"          d.worker_id, e.full_name worker_full_name,\n" +
                    $"          emp.position_id worker_position_id, emp.position_name worker_position_name,\n" +
                    $"          e.work_mode worker_work_mode,\n" +
                    $"          case \n" +
                    $"              when e.work_mode = '5/2-40' then 8.0\n" +
                    $"              when e.work_mode in ('I', 'II', 'III', 'IV') then 10.5\n" +
                    $"              else 8.0\n" +
                    $"          end worker_standard,\n" +
                    $"          sum((d.finish_date - d.start_date) * 24) worker_labor_cost\n" +
                    $"     from table({StaffSchema}.stf_get_all_works({StaffSchema}.get_shift_start(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')),\n" +
                    $"                                       {StaffSchema}.get_shift_finish(0, to_date(:shift_date, 'DD.MM.YYYY HH24:MI:SS')))) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $"left join {StaffSchema}.par_partner#v p on d.partner_id = p.id\n" +
                    $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"    where d.finish_date - d.start_date > 0\n" +
                    $"          and e.id is not null\n" +
                    $"      and d.site_id in (select id from {StaffSchema}.stf_site#v connect by prior id = parent_id start with code = 'GROUND_FLOOR')\n" +
                    $" group by d.worker_id, e.full_name, e.work_mode, emp.position_id, emp.position_name)\n" +
                    $"   select d.worker_position_id, d.worker_position_name,\n" +
                    $"          round(sum(d.worker_labor_cost / d.worker_standard), 4) labor_cost, --Человекосмены\n" +
                    $"          round(sum(d.worker_labor_cost) / sum(d.worker_standard) * 100, 2) efficiency,\n" +
                    $"          count(distinct d.worker_id) standard_labor_cost-- Человекосмены\n" +
                    $"    from data2 d\n" +
                    $"left join {StaffSchema}.stf_position#v p on d.worker_position_id = p.id\n" +
                    $"    where p.is_linear = 1\n" +
                    $" group by d.worker_position_id, d.worker_position_name";

                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("shift_date", date.ToString()));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dynamic line = new ExpandoObject();
                    line.PositionId = Utils.DbValueToShort(reader["worker_position_id"]);
                    line.StandardLaborCost = Utils.DbValueToDouble(reader["standard_labor_cost"]);
                    line.LaborCost = Utils.DbValueToDouble(reader["labor_cost"]);
                    lines.Add(line);
                }
                reader.Close();
            }
            return lines.ToArray();
        }
        /// <summary>
        /// Возвращает промежутки работ по БоссКонтролю
        /// </summary>
        /// <param name="fromDate">Дата начала периода (+ время)</param>
        /// <param name="toDate">Дата окочания периода (+ время)</param>
        /// <param name="siteCode">Код участка</param>
        /// <returns>Массив объектов (работник, должность, дата и время начала работы, дата и время окончания работы)</returns>
        public dynamic[] GetBcWorks(DateTime fromDate, DateTime toDate, string siteCode)
        {
            var works = new List<dynamic>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"with dates as (\n" +
                    $"  select :start_date fact_work_start_date,\n" +
                    $"         :finish_date fact_work_finish_date\n" +
                    $"    from dual\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"    select emp.position_id, emp.position_name,\n" +
                    $"           d.worker_id, e.full_name worker_full_name,\n" +
                    $"           --greatest(d.start_date, dt.fact_work_start_date) start_date,\n" +
                    $"           d.start_date,\n" +
                    $"           --least(d.finish_date, dt.fact_work_finish_date) finish_date,\n" +
                    $"           d.finish_date,\n" +
                    $"           --round((least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date)) * 24 * 60 * 60) duration, -- Продолжительность\n" +
                    $"           round((d.finish_date - d.start_date) * 24 * 60 * 60) duration, -- Продолжительность\n" +
                    $"           d.site_id, s.name site_name\n" +
                    $"      from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works_wi(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $" left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $" left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $" left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"     where d.finish_date - d.start_date > 0\n" +
                    $"       and s.kind <> 'AUXILIARY'\n" +
                    $"       and s.code = :site_code\n" +
                    $"       and s.kind = 'GATE' -- Только ворота\n" +
                    $"       and e.id is not null\n" +
                    $" )\n" +
                    $"   select *\n" +
                    $"     from data2 d\n" +
                    $" order by d.position_id, d.worker_id, d.worker_full_name, d.start_date, d.finish_date";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", fromDate));
                command.Parameters.Add(new OracleParameter("finish_date", toDate));
                command.Parameters.Add(new OracleParameter("site_code", siteCode));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var duration = Utils.DbValueToInt(reader["duration"]);
                    if (duration <= 0)
                        continue;

                    dynamic work = new ExpandoObject();
                    work.workerId = Utils.DbValueToInt(reader["worker_id"]);
                    work.workerFullName = Utils.DbValueToString(reader["worker_full_name"]);
                    work.positionId = Utils.DbValueToShort(reader["position_id"]);
                    work.positionName = Utils.DbValueToString(reader["position_name"]);
                    work.startDate = Utils.DbValueToDateTime(reader["start_date"]);
                    work.finishDate = Utils.DbValueToDateTime(reader["finish_date"]);
                    work.duration = duration;
                    works.Add(work);
                }
                reader.Close();
            }
            return works.ToArray();
        }
        /// <summary>
        /// Возвращает количество РАЗЛИЧНЫХ работников, которые по системе БоссКонтроль 
        /// работали в указанный интервал времени на указанном участке
        /// </summary>
        /// <param name="fromDate">Дата и время начала интервала</param>
        /// <param name="toDate">Дата и время окончания интервала</param>
        /// <param name="siteCode">Код участка работ</param>
        /// <returns>Количество различных работников</returns>
        public int GetBcWorkersCount(DateTime fromDate, DateTime toDate, string siteCode)
        {
            var ids = new HashSet<int>();
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"with dates as (\n" +
                    $"  select :start_date fact_work_start_date,\n" +
                    $"         :finish_date fact_work_finish_date\n" +
                    $"    from dual\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"    select emp.position_id, emp.position_name,\n" +
                    $"           d.worker_id, e.full_name worker_full_name\n" +
                    $"      from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works_wi(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $" left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $" left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $" left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"     where d.finish_date - d.start_date > 0\n" +
                    $"       and s.kind <> 'AUXILIARY'\n" +
                    $"       and s.code = :site_code\n" +
                    $"       and s.kind = 'GATE' -- Только ворота\n" +
                    $"       and e.id is not null\n" +
                    $" )\n" +
                    $"   select distinct d.worker_id\n" +
                    $"     from data2 d\n" +
                    $" order by d.worker_id";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", fromDate));
                command.Parameters.Add(new OracleParameter("finish_date", toDate));
                command.Parameters.Add(new OracleParameter("site_code", siteCode));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = Utils.DbValueToInt(reader["worker_id"]);
                    if (!ids.Contains(id))
                        ids.Add(id);
                }
                reader.Close();
            }
            return ids.Count;
        }
        /// <summary>
        /// Функция определения планового времени работы и простоя персонала за период (во времени и в деньгах).
        /// Период разбивается на смены по 12 часов.
        /// Для каждой смены определяются работники, которые в течение смены отработали хотя бы 1% времени от смены.
        /// Для всех таких работников определяется их общее время работы на смене и нормативное время смены.
        /// Из нормативного времени смены вычитаем время работы сотрудника на смене - это и будет простой.
        /// Все такие простои суммируем по всем работникам по всем сменам.
        /// </summary>
        /// <param name="fromDate">Дата начала периода</param>
        /// <param name="toDate">Дата окончания периода</param>        
        /// <param name="planTime">Плановое время</param>
        /// <param name="planCost">Плановые затраты в деньгах</param>
        /// <param name="downTime">Простои</param>
        /// <param name="downCost">Потери из-за простоев в деньгах</param>
        public void GetBcPlan(DateTime fromDate, DateTime toDate,
            out double planTime, out double planCost, out double downTime, out double downCost)
        {
            downTime = 0.0;
            downCost = 0.0;
            planTime = 0.0;
            planCost = 0.0;
            var shifts = Shift.GetShifts(fromDate, toDate);
            foreach (var shift in shifts)
            {
                var k = (double)shift.Length / DefaultShiftLength;
                using var command = _connection.CreateCommand();
                command.BindByName = true;
                var sql =
                    $"   select d.worker_id,\n" +
                    $"          d.site_id,\n" +
                    $"          d.start_date, d.finish_date\n" +
                    $"     from table({StaffSchema}.stf_get_all_works(:from_date, :to_date)) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"    where s.kind <> 'AUXILIARY' and emp.employee_id is not null";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("from_date", shift.From));
                command.Parameters.Add(new OracleParameter("to_date", shift.To));
                using var reader = command.ExecuteReader();
                var planWorks = new Dictionary<int, double>();
                var factWorks = new Dictionary<int, double>();
                int site_id =0;
                while (reader.Read())
                {
                    var workerId = Utils.DbValueToInt(reader["worker_id"]);
                    site_id = Utils.DbValueToInt(reader["site_id"]);
                    var startTime = Utils.DbValueToDateTime(reader["start_date"]);
                    var finishTime = Utils.DbValueToDateTime(reader["finish_date"]);
                    var employee = GetEmployee(workerId);
                    int planShiftLengthSeconds = Shift.GetShortShiftLength(startTime, finishTime, site_id, employee.ShiftLength, shift.Kind); //проверка на случай сокращенной смены
                    if (!planWorks.ContainsKey(workerId))
                    {
                        planWorks.Add(workerId, planShiftLengthSeconds * k);
                    }
                    else
                    {
                        if (site_id == 1053)
                            planWorks[workerId] = planShiftLengthSeconds * k;
                    }
                    
                    if (!factWorks.ContainsKey(workerId))
                        factWorks.Add(workerId, 0.0);
                    var span = finishTime - startTime;
                    factWorks[workerId] += (int)Math.Round(span.TotalSeconds);
                }
                reader.Close();
                foreach (var workerId in planWorks.Keys)
                {
                    // в случае случайной переработки необходимо отметиться на участке 1053 и таким образом игнорировать
                    // это время при подсчете планового времени смены и простоев. Сейчас это не работает из-за условия ниже.
                    // Проверку factWorks[workerId] / planWorks[workerId] < 0.1 - Вернул по заявке 4760 по распоряжению Охотникова А.А.
                    if (factWorks[workerId] / planWorks[workerId] < 0.1)
                        continue;
                    var workerDownTime = planWorks[workerId] - factWorks[workerId];
                    if (workerDownTime <= 0.0)
                        continue;

                    var employee = GetEmployee(workerId);
                    var cost = Math.Max(employee.MajorPosition.LaborHourCost, employee.Cost);
                    planTime += planWorks[workerId];
                    planCost += planWorks[workerId] / 60 / 60 * cost;
                    downTime += workerDownTime;
                    downCost += workerDownTime / 60 / 60 * cost;
                }
            }
        }
        /// <summary>
        /// Функция формирования отчёта о затратах/выручке за период
        /// </summary>
        /// <param name="fromDate">Дата (+ время) начала периода</param>
        /// <param name="toDate">Дата (+ время) окончания периода</param>
        /// <param name="partnersIds">Идентификаторы клиентов (мандантов)</param>
        /// <returns>Данные отчёта</returns>
        public dynamic[] GetEcData(DateTime fromDate, DateTime toDate, string[] partnersIds)
        {
            var partners = GetPartners();
            var ids = new StringBuilder();
            if (partnersIds.Length > 0 && partnersIds.Length < partners.Length)
            {
                foreach (var id in partnersIds)
                {
                    var partner = Array.Find(partners, p => p.Id == id);
                    if (partner == null)
                        continue;
                    if (ids.Length > 0)
                        ids.Append(", ");
                    ids.Append(id);
                }
            }

            var works = new List<dynamic>();
            dynamic element;
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"with dates as (\n" +
                    $"  select :start_date fact_work_start_date,\n" +
                    $"         :finish_date fact_work_finish_date\n" +
                    $"    from dual\n" +
                    $"),\n" +
                    $"billind_data as (\n" +
                    $"    -- 1.2 складывается результат умножения всех начисленных услуг на их стоимость по данному клиенту за указанный период -\n" +
                    $"    -- это есть вся выручка по клиенту за период.\n" +
                    $"  select bsl.bl_mndnr partner_id,\n" +
                    $"         sum(case when bst.bst_doc_type = 'SYSTEM' and lower(bsd.bsd_name) not like '%редост%поддон%' and bsd.BSD_BST_CODE not like '%K%' then bsl.bl_qty * bsl.bl_price end) store_income,\n" +
                    $"         sum(case when lower(bsd.bsd_name) like '%редост%поддон%' then bsl.bl_qty * bsl.bl_price end) consumables_income,\n" +
                    $"         sum(case when bsd.BSD_BST_CODE NOT LIKE '%STG%' and lower(bsd.bsd_name) not like '%редост%поддон%' then bsl.bl_qty * bsl.bl_price end) handling_income,\n" +
                    $"         sum(bsl.bl_qty * bsl.bl_price) total_income\n" +
                    $"    from {CommonSchema}.bl_service_log bsl\n" +
                    $"    --left join {CommonSchema}.bl_price bp on bp.bp_bsd_id = bsl.bl_bsd_id and bp.bp_mndnr = bsl.bl_mndnr\n" +
                    $"    left join {CommonSchema}.bl_services_def bsd on bsd.bsd_id = bsl.bl_bsd_id\n" +
                    $"    left join {CommonSchema}.bl_service_type bst on bst.bst_code = bsd.bsd_bst_code\n" +
                    $"   where bsl.BL_DATE between :start_date and :finish_date\n" +
                    (ids.Length <= 0 ? "" : $"         and bsl.bl_mndnr in ({ids.ToString()})\n") +
                    $"group by bsl.bl_mndnr\n" +
                    $"order by partner_id\n" +
                    $"),\n" +
                    $"data2 as (\n" +
                    $"   select case\n" +
                    $"              when emp.is_linear = 1 then emp.cost\n" +
                    $"              else 0\n" +
                    $"          end position_cost,\n" +
                    $"          d.worker_id,\n" +
                    $"          greatest(d.start_date, dt.fact_work_start_date) start_date,\n" +
                    $"          least(d.finish_date, dt.fact_work_finish_date) finish_date,\n" +
                    $"          case\n" +
                    $"              when emp.is_linear = 1 then least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date)\n" +
                    $"              else 0\n" +
                    $"          end duration, -- Продолжительность\n" +
                    $"          d.partner_id, par.name partner_name\n" +
                    $"     from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"left join {PartnersSchema}.par_partner#v par on d.partner_id = par.id\n" +
                    $"    where d.finish_date - d.start_date > 0\n" +
                    $"      and s.kind <> 'AUXILIARY'\n" +
                    $"      and e.id is not null\n" +
                    (ids.Length <= 0 ? "" : $"      and d.partner_id in ({ids.ToString()})\n") +
                    $" ),\n" +
                    $"data3 as (\n" +
                    $"   select d.partner_id, d.partner_name,\n" +
                    $"          sum(duration * 24) total_duration,\n" +
                    $"          sum(d.position_cost * d.duration * 24) * 1.305 total_cost, 'RUR' total_cost_currency\n" +
                    $"     from data2 d\n" +
                    $" group by\n" +
                    $"          d.partner_id, d.partner_name\n" +
                    $")\n" +
                    $"   select nvl(d.partner_id, bd.partner_id) partner_id, par.name partner_name,\n" +
                    $"          round(nvl(d.total_duration, 0.0), 8) bc_duration, -- В часах\n" +
                    $"          round(nvl(d.total_cost, 0.0), 8) bc_cost,\n" +
                    $"          round(nvl(bd.store_income, 0.0), 8) billing_store_income,\n" +
                    $"          round(nvl(bd.consumables_income, 0.0), 8) billing_consumables_income,\n" +
                    $"          round(nvl(bd.handling_income, 0.0), 8) billing_handling_income,\n" +
                    $"          round(nvl(bd.total_income, 0.0), 8) billing_total_income,\n" +
                    $"          round(nvl(bd.handling_income, 0.0) - nvl(d.total_cost, 0.0), 8) handling_profit,\n" +
                    $"          round(nvl(bd.total_income, 0.0) - nvl(d.total_cost, 0.0), 8) total_profit,\n" +
                    $"          case\n" +
                    $"              when nvl(bd.handling_income, 0.0) <> 0\n" +
                    $"                  then round((nvl(bd.handling_income, 0.0) - nvl(d.total_cost, 0.0)) / nvl(bd.handling_income, 0.0) * 100.0, 8)\n" +
                    $"              else 0.0\n" +
                    $"          end handling_margin,\n" +
                    $"          case\n" +
                    $"              when nvl(bd.total_income, 0.0) <> 0\n" +
                    $"                  then round((nvl(bd.total_income, 0.0) - nvl(d.total_cost, 0.0)) / nvl(bd.total_income, 0.0) * 100.0, 8)\n" +
                    $"              else 0.0\n" +
                    $"          end total_margin,\n" +
                    $"          'RUR' currency_code\n" +
                    $"     from data3 d\n" +
                    $"full join billind_data bd on d.partner_id = bd.partner_id\n" +
                    $"left join {PartnersSchema}.par_partner#v par on nvl(d.partner_id, bd.partner_id) = par.id\n" +
                    $"order by partner_id";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", fromDate));
                command.Parameters.Add(new OracleParameter("finish_date", toDate));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    element = new ExpandoObject();
                    element.partnerId = Utils.DbValueToString(reader["partner_id"]);
                    element.partnerName = Utils.DbValueToString(reader["partner_name"]);
                    element.bcDuration = Utils.DbValueToDouble(reader["bc_duration"]);
                    element.bcCost = Utils.DbValueToDouble(reader["bc_cost"]);
                    element.billingStoreIncome = Utils.DbValueToDouble(reader["billing_store_income"]);
                    element.billingHandlingIncome = Utils.DbValueToDouble(reader["billing_handling_income"]);
                    element.billingConsumablesIncome = Utils.DbValueToDouble(reader["billing_consumables_income"]);
                    element.billingTotalIncome = Utils.DbValueToDouble(reader["billing_total_income"]);
                    element.handlingProfit = Utils.DbValueToDouble(reader["handling_profit"]);
                    element.totalProfit = Utils.DbValueToDouble(reader["total_profit"]);
                    element.handlingMargin = Utils.DbValueToDouble(reader["handling_margin"]);
                    element.totalMargin = Utils.DbValueToDouble(reader["total_margin"]);
                    element.currencyCode = Utils.DbValueToString(reader["currency_code"]);
                    works.Add(element);
                }
                reader.Close();
            }

            // Определяем простои
            GetBcPlan(fromDate, toDate, out double planTime, out double planCost, out double downTime, out double downCost);
            element = new ExpandoObject();
            element.partnerId = "DOWN";
            element.partnerName = "Простои";
            element.bcDuration = downTime / 60 / 60;
            element.bcCost = downCost;
            works.Add(element);

            return works.ToArray();
        }
        /// <summary>
        /// Функция формирования детализированного отчёта по трудозатратам персонала на конктретном клиенте (манданте) за выбранный период
        /// </summary>
        /// <param name="fromDate">Дата (+ время) начала периода</param>
        /// <param name="toDate">Дата (+ время) окончания периода</param>
        /// <param name="partnersId">Идентификатор клиента (манданта)</param>
        /// <returns></returns>
        public dynamic GetEcDetailedData(DateTime fromDate, DateTime toDate, string partnersId)
        {
            //var partners = GetPartners();
            //var id = Array.Find(partners, p => p.Id == partnersId);
            var works = new List<dynamic>();
            dynamic element;
            using (var command = _connection.CreateCommand())
            {
                command.BindByName = true;
                var sql =
                    $"--Отчёт должен показывать за период, какой сотрудник, сколько отработал на клиенте, по всем участкам. Участки тоже можно вывести.\r\n" +
                    $"with dates as (\n" +
                    $"  select :start_date fact_work_start_date,\n" +
                    $"         :finish_date fact_work_finish_date\n" +
                    $"  from dual),\n" +
                    $"data2 as (\n" +
                    $"  select emp.position_id, emp.position_name,\n" +
                    $"         d.worker_id, e.full_name worker_full_name,\n" +
                    $"         emp.cost position_cost,\n" +
                    $"         d.start_date,\n" +
                    $"         greatest(d.start_date, dt.fact_work_start_date) clipped_start_date,\n" +
                    $"         d.finish_date,\n" +
                    $"         least(d.finish_date, dt.fact_work_finish_date) clipped_finish_date,\n" +
                    $"         d.finish_date - d.start_date duration, -- Продолжительность\n" +
                    $"         least(d.finish_date, dt.fact_work_finish_date) - greatest(d.start_date, dt.fact_work_start_date) clipped_duration, -- Продолжительность\n" +
                    $"         d.site_id, s.name site_name, d.PARTNER_ID, par.name partner_name\n" +
                    $"  from dates dt\n" +
                    $"cross join table({StaffSchema}.stf_get_all_works_wi(dt.fact_work_start_date, dt.fact_work_finish_date)) d\n" +
                    $"left join {StaffSchema}.stf_employee_major_position#v emp on d.worker_id = emp.employee_id\n" +
                    $"left join {StaffSchema}.stf_employee#v e on d.worker_id = e.id\n" +
                    $"left join {PartnersSchema}.par_partner#v par on d.partner_id = par.id\n" +
                    $"left join {StaffSchema}.stf_site#v s on d.site_id = s.id\n" +
                    $"where d.finish_date - d.start_date > 0\n" +
                    $"      and s.kind <> 'AUXILIARY'\n" +
                    $"  --     and (:site_code is null or s.code = :site_code)\n" +
                    $"  --     and s.kind = 'GATE' --Только ворота\n" +
                    $"      and e.id is not null\n " +
                    $"      and par.id = :partnersId)\n" +
                    $"select --d.position_id, \n" +
                    $"  d.position_name, --d.worker_id,\n" +
                    $"  d.worker_full_name, --d.site_id,\n" +
                    $"  d.site_name,\n" +
                    $"  D.PARTNER_ID,\n" +
                    $"  d.partner_name,\n" +
                    $"  d.start_date, \n" +
                    $"  d.finish_date,\n" +
                    $"  d.clipped_start_date,\n" +
                    $"  d.clipped_finish_date,\n" +
                    $"  round(nvl(d.position_cost * d.clipped_duration * 24 * 1.305, 0.0), 8) bc_cost,\n" +
                    $"  round(d.duration * 24 * 60 * 60) duration,\n" +
                    $"  round(d.clipped_duration * 24 * 60 * 60) clipped_duration\n" +
                    $"from data2 d\n" +
                    $"order by --d.worker_full_name,\n" +
                    $"d.START_DATE";
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("start_date", fromDate));
                command.Parameters.Add(new OracleParameter("finish_date", toDate));
                command.Parameters.Add(new OracleParameter("partnersId", partnersId));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    element = new ExpandoObject();
                    element.partnerId = Utils.DbValueToString(reader["partner_id"]);
                    element.partnerName = Utils.DbValueToString(reader["partner_name"]);
                    element.startWork = Utils.DbValueToDateTime(reader["clipped_start_date"]);
                    element.finishWork = Utils.DbValueToDateTime(reader["clipped_finish_date"]);
                    element.duration = Utils.DbValueToDouble(reader["clipped_duration"]);
                    element.workerFullName =  Utils.DbValueToString(reader["worker_full_name"]);
                    element.siteName =  Utils.DbValueToString(reader["site_name"]);
                    element.positionName = Utils.DbValueToString(reader["position_name"]);
                    element.bcCost = Utils.DbValueToDouble(reader["bc_cost"]);
                    works.Add(element);
                }
                reader.Close();
            }
            return works.ToArray();
        }

        #endregion
    }
}