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
        
        public string Id { get; set; } // Код артикула, как он заведен у поклажедателя
        public string Name { get; set; } // Наименование артикула у поклажедателя 
        public string Group { get; set; } // Группа артикула. Для разделения учетных групп артикулов, например, по методу резервирования (FIFO, LIFO и т.п.). Необходимо согласовать с технологами Оператора.
        public string Gtin { get; set; } // Значение штрихкода 
        //
        public int Netto { get; set; } // Вес единицы артикула нетто (кг до г) 
        public int Brutto { get; set; } // Вес единицы артикула брутто (кг до г) 
        //
        public int Length { get; set; } // Длина (км до мм) 
        public int Width { get; set; } // Ширина (км до мм) 
        public int Heigth { get; set; } // Высота (км до мм) 
        public int Volume { get; set; } // Объем  (м3 до мм3) 
        //
        public string UnitLinerial { get; set; } // Единица измерения линейных размеров по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет 
        public string UnitVolume { get; set; } // Единица измерения объема по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет 
        public string UnitWeight { get; set; } // Единица измерения веса по умолчанию, т.е. в те, в которых указываются присылаемые данные и производится учет 
        //
        public int MaxAmountPerPalet { get; set; } // Максимальное количество единиц артикула на одном паллете 
        public int AmountPerPackage { get; set; } // Кол-во в упаковке / квант товара 
        public int ExpirationDateInDays { get; set; } // Срок годности в днях 
        public double Price { get; set; } // Базовая цена товара.
        //
        public string Info1 { get; set; } // Дополнительная информация. Возможно, по согласованию, включать информацию в виде {SIZE=52}{COLOR=RED}{ COMMENT = комментарий}  

    }
}
