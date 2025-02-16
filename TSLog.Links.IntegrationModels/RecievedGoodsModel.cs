namespace TSLog.Links.IntegrationModels;

/// <summary>
/// Модель для телграммы заявки на приёмку. Включает в себя Заголовок, Строку или Строки и Окончание.
/// </summary>
public class ReceivedGoodsModel
{
    public Header HeaderInfo { get; set; }
    public List<Body> BodyInfo { get; set; }
    public Tail TailInfo { get; set; }

    public ReceivedGoodsModel(string msgNam, string mndNr, string operationType, string custDocId, string custVendId)
    {
        HeaderInfo = new Header(msgNam, mndNr, operationType, custDocId, custVendId);
        BodyInfo = new List<Body>();
        TailInfo = new Tail(msgNam, mndNr, custDocId);
    }

    /// <summary>
    /// Заголовок телеграммы заявки на приёмку.
    /// </summary>
    public class Header
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
        /// Тип документа.
        /// </summary>
        public string EcOperation { get; set; } = string.Empty;

        /// <summary>
        /// Вид операции – приемка.
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Номер документа клиента.
        /// </summary>
        public string CustDocId { get; set; }

        /// <summary>
        /// Код контрагента (поставщика/грузополучателя).
        /// </summary>
        public string CustVendId { get; set; }

        /// <summary>
        /// Код GUID номера документа.
        /// </summary>
        public string CustDocGUID { get; set; } = string.Empty;

        public Header(string msgNam, string mndNr, string operationType, string custDocId, string custVendId)
        {
            MsgNam = msgNam;
            MndNr = mndNr;
            OperationType = operationType;
            CustDocId = custDocId;
            CustVendId = custVendId;
        }
    }

    /// <summary>
    /// Строка телеграммы заявки на приёмку.
    /// </summary>
    public class Body
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
        /// Номер документа клиента.
        /// </summary>
        public string CustDocId { get; set; }

        /// <summary>
        /// Код артикула.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Количество в единицах учета.
        /// </summary>
        public string Qty { get; set; }

        /// <summary>
        /// Цена в рублях для документов MX-1.
        /// </summary>
        public string Price { get; set; } = string.Empty;

        /// <summary>
        /// Код ОКЕИ.
        /// </summary>
        public string OKEI { get; set; } = string.Empty;

        /// <summary>
        /// Единица измерения учета.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Код GUID номера документа.
        /// </summary>
        public string CustDocGUID { get; set; } = string.Empty;

        /// <summary>
        /// Плановая дата приемки в формате dd.mm.yyyy hh24:mi:ss.
        /// </summary>
        public string ArrivalPlanDate { get; set; } = string.Empty;

        /// <summary>
        /// Дополнительная информация.
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// Партия.
        /// </summary>
        public string BstChgName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BstChgName2 { get; set; } = string.Empty;

        /// <summary>
        /// SSCC код контейнера (мастер коробка) 18 символов.
        /// </summary>
        public string TeNam { get; set; }

        public Body(string msgNam, string mndNr, string custDocId,
            string itemId, string qty, string price, string unit, string bstChgNam, string teNam)
        {
            MsgNam = msgNam;
            MndNr = mndNr;
            CustDocId = custDocId;
            ItemId = itemId;
            Qty = qty;
            Price = price;
            Unit = unit;
            BstChgName = bstChgNam;
            TeNam = teNam;
        }
    }

    /// <summary>
    /// Окончание телеграммы заявки на приёмку.
    /// </summary>
    public class Tail
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
        /// Номер документа клиента.
        /// </summary>
        public string CustDocId { get; set; }

        /// <summary>
        /// Код GUID номера документа.
        /// </summary>
        public string CustDocGUID { get; set; } = string.Empty;

        public Tail(string msgNam, string mndNr, string custDocId)
        {
            MsgNam = msgNam;
            MndNr = mndNr;
            CustDocId = custDocId;
        }
    }
}
