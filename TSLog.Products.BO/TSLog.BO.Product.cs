using TSLog.Common.BO;

namespace TSLog.Products.BO
{
    /// <summary>
    /// Товар (карточка товара)
    /// </summary>
    public class Product : Entity
    {
        /// Если это позиция прихода
        /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7221368A-BD9A-4759-89BF-304C7B61931C}
        /// Если это позиция заказа
        /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={2874B98C-FCA4-4398-964A-E8ACB90EA6FA}
        /// Сама таблица товара
        /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={B2A883FB-F8F9-4C05-9552-4A09398C8EF0}

        /// <summary>
        /// (ArtGrpNam) Группа артикула. Для разделения учетных групп артикулов, например, по методу резервирования (FIFO, LIFO и т.п.). Необходимо согласовать с технологами Оператора.
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор
        /// </summary>
        public new string Id { get; set; }

        /// <summary>
        /// Global trade item number
        /// </summary>
        public string Gtin { get; private set; }

        /// <summary>
        /// Название
        /// </summary>
        public new string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        /// <summary>
        /// (ArtNetWeight) Вес единицы артикула нетто (кг до г)
        /// </summary>
        public double Netto { get; set; } = 0.0;

        /// <summary>
        /// (ArtGrossWeight) Вес единицы артикула брутто (кг до г)
        /// </summary>
        public double Brutto { get; set; } = 0.0;

        //
        /// <summary>
        /// (ArtLng) Длина (км до мм)
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// (ArtBre) Ширина (км до мм)
        /// </summary>
        public int Width { get; set; } = 0;

        /// <summary>
        /// (ArtHoe) Высота (км до мм)
        /// </summary>
        public int Height { get; set; } = 0;

        /// <summary>
        /// (ArtVol) Объем  (м3 до мм3)
        /// </summary>
        public int Volume { get; set; } = 0;

        /// <summary>
        /// (ArtMgEh) Единица измерения по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет
        /// </summary>
        public string UnitType { get; set; } = string.Empty;

        /// <summary>
        /// (ArtAmbEh) Единица измерения линейных размеров по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет
        /// </summary>
        public string UnitLinerial { get; set; } = string.Empty;

        /// <summary>
        /// (ArtVolEh) Единица измерения объема по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет
        /// </summary>
        public string UnitVolume { get; set; } = string.Empty;

        /// <summary>
        /// (ArtWeiEh) Единица измерения веса по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет
        /// </summary>
        public string UnitWeight { get; set; } = string.Empty;

        /// <summary>
        /// (ArtLttMax) Максимальное количество единиц артикула на одном паллете
        /// </summary>
        public int MaxAmountPerPalet { get; set; }

        /// <summary>
        /// (ArtLttMgPack) Кол-во в упаковке / квант товара
        /// </summary>
        public int AmountPerPackage { get; set; }

        /// <summary>
        /// Срок годности в днях
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// (ArtPrice) Базовая цена товара.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public new string Note
        {
            get
            {
                return base.Note;
            }
            set
            {
                base.Note = value;
            }
        }

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}
        /// </summary>
        public string Info1 { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}
        /// </summary>
        public string Info2 { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}
        /// </summary>
        public string Info3 { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}
        /// </summary>
        public string Info4 { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}
        /// </summary>
        public string Info5 { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}
        /// </summary>
        public string Info6 { get; set; } = string.Empty;

        public Product(string id, string code, string gtin, string name,
            string shortName = "", string altName = "", string displayName = "",
            string note = "")
            : base(-1, code, name, shortName, altName, displayName, note)
        {
            Id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : string.Empty;
            Gtin = !string.IsNullOrWhiteSpace(gtin) ? gtin.Trim() : string.Empty;
        }
        private Product() : base()
        {
            Id = string.Empty;
            Gtin = string.Empty;
        }

        private static Product? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public new static Product Null
        {
            get
            {
                if (_null == null)
                    _null = new Product("NULL", "NULL", "NULL", "NULL", "NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }
        private static Product? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Product Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Product();
                return _empty;
            }
        }
    }
}