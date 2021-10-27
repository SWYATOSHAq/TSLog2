using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.BO
{
    public class Order
    {

        // шапка
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7896B268-8251-47A6-95CC-6603BDD743D4}
        // позиция
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={2874B98C-FCA4-4398-964A-E8ACB90EA6FA}

        public class Line 
        {
            /// <summary>
            /// номер строки
            /// </summary>
            public int Number { get; set; }
            public Product Product { get; set; }
            /// <summary>
            /// (Qty) Количество в единицах учета 
            /// </summary>
            public double Quantity { get; set; }
            /// <summary>
            /// (Price) Цена в рублях для документов MX-1.
            /// </summary>
            public double Price { get; set; }
            /// <summary>
            /// (OKAI) Код ОКЕИ
            /// </summary>
            public string OKEI { get; set; }
            /// <summary>
            /// (Unit) Единица измерения учета
            /// </summary>
            public string Unit { get; set; }
            /// <summary>
            /// (Info) Дополнительная информация  Возможно, по согласованию, включать информацию в виде {BOX=002564}{TYPE=RZN}{COMMENT=комментарий}
            /// </summary>
            public string Info { get; set; }
        }
        /// <summary>
        /// (CustDocGUID) Код GUID номера документа
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// (CustDocId) Номер документа клиента
        /// </summary>
        public string ClientDocId { get; set; }
        public LegalEntity Client { get; set; }
        public LegalEntity Vendor { get; set; }
        /// <summary>
        /// (AuaNam) Название типа заказа
        /// </summary>
        public string OrderKind { get; set; }
        /// <summary>
        /// (EcOperation) Тип документа 
        /// </summary>
        public OperationType Operation { get; set; }
        /// <summary>
        /// (ShipDate) Плановая дата отгрузки в формате dd.mm.yyyy hh24:mi:ss 
        /// </summary>
        public DateTime ShipDate { get; set; }
        //
        /// <summary>
        /// (DeliveryName) Наименование точки доставки 
        /// </summary>
        public string DeliveryName { get; set; }
        /// <summary>
        /// (DeliveryZip) Индекс адреса доставки 
        /// </summary>
        public string DeliveryZip { get; set; }
        /// <summary>
        /// (DeliveryCity) Город доставки 
        /// </summary>
        public string DeliveryCity { get; set; }
        /// <summary>
        /// (DeliveryAddress1) Улица и дом доставки (строка 2)
        /// </summary>
        public string DeliveryAddress1 { get; set; }
        /// <summary>
        /// (DeliveryAddress2) Улица и дом доставки (строка 1) 
        /// </summary>
        public string DeliveryAddress2 { get; set; }
        /// <summary>
        /// (DeliveryComment) Комментарий к доставке. Возможно, по согласованию, включать текст в виде {BOX=002564}{TYPE=RZN}{COMMENT = комментарий}
        /// </summary>
        public string DeliveryComment { get; set; }
        //
        /// <summary>
        /// дата создания документа
        /// </summary>
        public DateTime Date { get; set; }
        //
        public Line[] Body { get; set; }
    }
}
