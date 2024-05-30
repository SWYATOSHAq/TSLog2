namespace TSLog.Links.IntegrationModels;

/// <summary>
/// Модель телеграммы справочника артикулов.
/// </summary>
public class SkuCatalogModel
{
    /// <summary>
    /// Название сообщения находящегося в строке.
    /// </summary>
    public string MsgNam { get; set; }

    /// <summary>
    /// Цифровой идентификатор клиента.
    /// </summary>
    public string MndNr { get; set; }

    /// <summary>
    /// Код артикула, как он заведен у поклажедателя
    /// </summary>
    public string ItemId { get; set; }

    /// <summary>
    /// Наименование артикула у поклажедателя.
    /// </summary>
    public string ArtBez { get; set; } = string.Empty;
    /// <summary>
    /// Группа артикула. Для разделения учетных групп артикулов, например, по методу резервирования (FIFO, LIFO и т.п.). Необходимо согласовать с технологами Оператора.
    /// </summary>
    /// 

    public string ArtGrpNam { get; set; }
    /// <summary>
    /// Значение штрихкода.
    /// </summary>    
    public string ArtEan { get; set; }

    /// <summary>
    /// Вес единицы артикула нетто (кг до г).
    /// </summary>
    public string ArtNetWeight { get; set; } = string.Empty;

    /// <summary>
    /// Вес единицы артикула брутто (кг до г).
    /// </summary>
    public string ArtGrossWeight { get; set; } = string.Empty;

    /// <summary>
    /// Длина (км до мм).
    /// </summary>
    public string ArtLng { get; set; } = string.Empty;

    /// <summary>
    /// Ширина (км до мм).
    /// </summary>
    public string ArtBre { get; set; } = string.Empty;

    /// <summary>
    /// Высота (км до мм).
    /// </summary>
    public string ArtHoe { get; set; } = string.Empty;

    /// <summary>
    /// Объем  (м3 до мм3).
    /// </summary>
    public string ArtVol { get; set; } = string.Empty;

    /// <summary>
    /// Единица измерения по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет.
    /// </summary>
    public string ArtMgEh { get; set; } = string.Empty;

    /// <summary>
    /// Единица измерения линейных размеров по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет.
    /// </summary>
    public string ArtAmbEh { get; set; } = string.Empty;

    /// <summary>
    /// Единица измерения объема по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет.
    /// </summary>
    public string ArtVolEh { get; set; } = string.Empty;

    /// <summary>
    /// Единица измерения веса по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет.
    /// </summary>
    public string ArtWeiEh { get; set; } = string.Empty;

    /// <summary>
    /// Максимальное количество единиц артикула на одном паллете.
    /// </summary>
    public string ArtLttMax { get; set; } = string.Empty;

    /// <summary>
    /// Кол-во в упаковке / квант товара.
    /// </summary>
    public string ArtLttMgPack { get; set; } = string.Empty;

    /// <summary>
    /// Срок годности в днях.
    /// </summary>
    public string DayOfExpiration { get; set; } = string.Empty;

    /// <summary>
    /// Базовая цена товара. Разделитель точка «.».
    /// </summary>
    public string ArtPrice { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительная информация. Возможно, по согласованию, включать подтэги в виде {SIZE=52}{COLOR=RED}{COMMENT=комментарий}.
    /// </summary>
    public string ArtInfo1 { get; set; } = string.Empty;

    public SkuCatalogModel(string msgNam, string mndNr, string itemId, string artGrpNam, string artEan)
    {
        MsgNam = msgNam;
        MndNr = mndNr;
        ItemId = itemId;
        ArtGrpNam = artGrpNam;
        ArtEan = artEan;
    }
}
