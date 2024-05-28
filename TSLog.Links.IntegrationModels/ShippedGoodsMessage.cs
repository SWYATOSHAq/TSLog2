using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSLog.Links.IntegrationModels.ReceivedGoodsMessage;

namespace TSLog.Links.IntegrationModels;

/// <summary>
///  Модель телеграммы заявки на отгрузку. Включает в себя Заголовок, Строку или Строки и Окончание.
/// </summary>
public class ShippedGoodsMessage
{
    public Header HeaderInfo { get; set; }
    public List<Body> BodyInfo { get; set; }
    public Tail TailInfo { get; set; }

    public ShippedGoodsMessage(string msgNam, string mndNr, string operationType, string custDocId, string custVendId, string shipDate, AuaNameValues aualName)
    {
        HeaderInfo = new Header(msgNam, mndNr, operationType, custDocId, custVendId, shipDate, aualName);
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
        /// Вид операции – отгрузка.
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
        /// Плановая дата отгрузки в формате dd.mm.yyyy hh24:mi:ss.
        /// </summary>
        public string ShipDate { get; set; }

        /// <summary>
        /// Код GUID номера документа.
        /// </summary>
        public string CustDocGUID { get; set; } = string.Empty;

        /// <summary>
        /// Может принимать одно из 4 фиксированных значений RETAIL/MONO/MIX/NORMAL. 
        /// Определяет каким способом укладывать:
        /// 1. Несколько наименований на поддон (NORMAL)
        /// 2. 3 наименования на поддон (MIX)
        /// 3. Столбиками сколько войдет (RETAIL)
        /// 4. Одно наименование на поддон (MONO)
        /// </summary>
        public AuaNameValues AualName { get; set; }

        /// <summary>
        /// Наименование точки доставки.
        /// </summary>
        public string DeliveryName { get; set; } = string.Empty;

        /// <summary>
        /// Индекс адреса доставки.
        /// </summary>
        public string DeliveryZip { get; set; } = string.Empty;

        /// <summary>
        /// Город доставки.
        /// </summary>
        public string DeliveryCity { get; set; } = string.Empty;

        /// <summary>
        /// Улица и дом доставки (строка 1).
        /// </summary>
        public string DeliveryAddress1 { get; set; } = string.Empty;

        /// <summary>
        /// Улица и дом доставки (строка 2).
        /// </summary>
        public string DeliveryAddress2 { get; set; } = string.Empty;

        /// <summary>
        /// Комментарий к доставке. Содержит текстовое дублирование способа укладки на поддоны:
        /// 1. Несколько наименований на поддон (NORMAL)
        /// 2. 3 наименования на поддон (MIX)
        /// 3. Столбиками сколько войдет (RETAIL)
        /// 4. Одно наименование на поддон (MONO)
        /// Возможно, по согласованию, включать подтэги в виде BOX=002564}{TYPE=RZN}{COMMENT=комментарий}
        /// </summary>
        public string DeliveryComment { get; set; } = string.Empty;

        public Header(string msgNam, string mndNr, string operationType, string custDocId, string custVentId, string shipDate, AuaNameValues aualName)
        {
            MsgNam = msgNam;
            MndNr = mndNr;
            OperationType = operationType;
            CustDocId = custDocId;
            CustVendId = custVentId;
            ShipDate = shipDate;
            AualName = aualName;
        }
    }

    /// <summary>
    /// Строка телеграммы заявки на отгрузку.
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
        /// Цена в рублях для документов MX-3.
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
        /// Партия.
        /// </summary>
        public string BstChgNam { get; set; }

        /// <summary>
        /// Дополнительная информация. Возможно, по согласованию, включать подтэги в виде {BOX=002564}{TYPE=RZN}{COMMENT=комментарий}
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// Дата производства в формате dd.mm.yyyy hh24:mi:ss
        /// </summary>
        public string BstDzChg { get; set; }

        public Body(string msgNam, string mndNr, string custDocId, string itemId, string qty, string unit, string bstChgNam, string bstDzChg)
        {
            MsgNam = msgNam;
            MndNr = mndNr;
            CustDocId = custDocId;
            ItemId = itemId;
            Qty = qty;
            Unit = unit;
            BstChgNam = bstChgNam;
            BstDzChg = bstDzChg;
        }
    }

    /// <summary>
    /// Окончание телеграммы заявки на отгрузку.
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

        public Tail(string msgNam, string mndNr, string custDocId)
        {
            MsgNam = msgNam;
            MndNr = mndNr;
            CustDocId = custDocId;
        }
    }
}

public enum AuaNameValues
{
    Retail,
    Mono,
    Mix,
    Normal
}
