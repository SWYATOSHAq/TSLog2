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
            public int Number { get; set; }
            public Product Product { get; set; }
        }

        public string GUID { get; set; }
        public string ClientDocId { get; set; }
        public LegalEntity Client { get; set; }
        public LegalEntity Vendor { get; set; }
        // Тип документа 
        public OperationType Operation { get; set; }
        /// <summary>
        /// Плановая дата отгрузки в формате dd.mm.yyyy hh24:mi:ss 
        /// </summary>
        public DateTime ShipDate { get; set; }
        //
        /// <summary>
        /// Наименование точки доставки 
        /// </summary>
        public string DeliveryName { get; set; }
        /// <summary>
        /// Индекс адреса доставки 
        /// </summary>
        public string DeliveryZip { get; set; }
        /// <summary>
        /// Город доставки 
        /// </summary>
        public string DeliveryCity { get; set; }
        /// <summary>
        /// Улица и дом доставки (строка 2)
        /// </summary>
        public string DeliveryAddress1 { get; set; }
        /// <summary>
        /// Улица и дом доставки (строка 1) 
        /// </summary>
        public string DeliveryAddress2 { get; set; }
        /// <summary>
        /// Комментарий к доставке. Возможно, по согласованию, включать текст в виде {BOX=002564}{TYPE=RZN}{COMMENT = комментарий}
        /// </summary>
        public string DeliveryComment { get; set; }
        //
        public DateTime Date { get; set; }
        //
        public Line[] Body { get; set; }
    }
}
