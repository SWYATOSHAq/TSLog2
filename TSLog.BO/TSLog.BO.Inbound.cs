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
            public double Quantity { get; set; }
        }
        public string Id { get; set; }
        public DateTime Date { get; set; }
    }
}
