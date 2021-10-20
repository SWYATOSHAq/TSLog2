using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.BO
{
    public class Inbound
    {
        // шапка
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={1497144A-EAAA-437E-84A0-D1AF487D853E}
        // позиция
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7221368A-BD9A-4759-89BF-304C7B61931C}
        public class Line
        {
            public int Number { get; set; }
            public Product Product { get; set; }
            public double Quantity { get; set; } // Количество в единицах учета 
            public double Price { get; set; } // Цена в рублях для документов MX-1.
            public string OKEI { get; set; } // Код ОКЕИ
            public DateTime ArrivalPlanDate { get; set; } // Плановая дата приемки в формате dd.mm.yyyy hh24:mi:ss 
            public string Info { get; set; } // Дополнительная информация  Возможно, по согласованию, включать информацию в виде {BOX=002564}{TYPE=RZN}{COMMENT=комментарий} 
            public string BatchNumber { get; set; } // Партия 
            public string BatchNumber2 { get; set; } // Партия2 (?)


        }
        public string GUID { get; set; }
        public OperationType Operation { get; set; }
        public string ClientDocId { get; set; }

        public Partner Client { get; set; }
        public Partner Vendor { get; set; }

        //
        public DateTime Date { get; set; }
        public Line Body { get; set; }

    }

}
