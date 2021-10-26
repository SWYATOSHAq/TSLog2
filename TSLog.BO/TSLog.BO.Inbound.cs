using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.BO
{
    public class Inbound
    {
        public object Id;

        // шапка
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={1497144A-EAAA-437E-84A0-D1AF487D853E}
        // позиция
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7221368A-BD9A-4759-89BF-304C7B61931C}
        public class Line
        {
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
            /// Плановая дата приемки в формате dd.mm.yyyy hh24:mi:ss 
            /// </summary>
            public DateTime ArrivalPlanDate { get; set; }
            /// <summary>
            /// Дополнительная информация  Возможно, по согласованию, включать информацию в виде {BOX=002564}{TYPE=RZN}{COMMENT=комментарий}
            /// </summary>
            public string Info { get; set; }
            /// <summary>
            /// Партия 
            /// </summary>
            public string BatchNumber { get; set; } 
            /// <summary>
            /// Партия2 (?)
            /// </summary>
            public string BatchNumber2 { get; set; } 


        }
        /// <summary>
        /// (CustDocGUID) Код GUID номера документа
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// (EcOperation) Тип документа
        /// </summary>
        public OperationType Operation { get; set; }
        /// <summary>
        /// (CustDocId) Номер документа клиента
        /// </summary>
        public string ClientDocId { get; set; }
        /// <summary>
        /// (MndNr) Цифровой идентификатор клиента
        /// </summary>

        public LegalEntity Client { get; set; }
        /// <summary>
        /// (CustVendId) Код контрагента (поставщика/грузополучателя)
        /// </summary>
        public LegalEntity Vendor { get; set; }

        //
        /// <summary>
        /// отметка о дате и времени
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// тело документа
        /// </summary>
        public Line[] Body { get; set; }

    }

}
