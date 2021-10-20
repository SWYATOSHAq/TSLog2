using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.BO
{
    public class Product
    {
        // если это позиция прихода
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7221368A-BD9A-4759-89BF-304C7B61931C}
        // если это позиция заказа
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={2874B98C-FCA4-4398-964A-E8ACB90EA6FA}

        // сама таблица товара
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={B2A883FB-F8F9-4C05-9552-4A09398C8EF0}
        public string Id { get; set; }
        public string Name { get; set; }
        public string Gtin { get; set; }
    }
}
