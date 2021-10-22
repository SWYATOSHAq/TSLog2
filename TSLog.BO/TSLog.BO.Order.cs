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
        public OperationType Operation { get; set; } // Тип документа 
        //
        public DateTime ShipDate { get; set; } // Плановая дата отгрузки в формате dd.mm.yyyy hh24:mi:ss 
        //
        public string DeliveryName { get; set; } // Наименование точки доставки 
        public string DeliveryZip { get; set; } // Индекс адреса доставки 
        public string DeliveryCity { get; set; } // Город доставки 
        public string DeliveryAddress1 { get; set; } // Улица и дом доставки (строка 1) 
        public string DeliveryAddress2 { get; set; } // Улица и дом доставки (строка 2)
        public string DeliveryComment { get; set; } // Комментарий к доставке. Возможно, по согласованию, включать текст в виде {BOX=002564}{TYPE=RZN}{COMMENT = комментарий}
        //
        public DateTime Date { get; set; }
        //
        public Line[] Body { get; set; }
    }
}
