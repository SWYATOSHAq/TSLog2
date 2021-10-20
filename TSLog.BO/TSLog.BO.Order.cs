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
        public string Id { get; set; }
        public string Number { get; set; }
        public double Quantity { get; set; }
    }
}
