using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.BO
{
    public class MeasureUnit
    {
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={6512FD8B-4D58-49B0-9A36-AF01F2EED2FF}
        string Name { get; set; } // Название единицы измерения
        string Category { get; set; } // Размерность единицы измерения (объем, вес, длина и т. Д.)
        string Code { get; set; } // Название единицы измерения (например, кг, м, дюйм, литр)
        double Coefficient { get; set; } // Коэффициент преобразования: определяет номер этой единицы измерения, который соответствует базовой единице измерения.
        //
        MeasureUnit Baseunit { get; set; } // базовая единица измерения, коэффициент преобразования которой равен 1
    }
}
