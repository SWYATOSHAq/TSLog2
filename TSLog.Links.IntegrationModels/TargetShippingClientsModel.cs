using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.IntegrationModels;

/// <summary>
/// Модель телеграммы с перечнем контрагентов-грузополучателей.
/// </summary>
public class TargetShippingClientsModel
{
    /// <summary>
    /// Название сообщения находящегося в строке.
    /// </summary>
    public string MsgNam { get; set; }

    /// <summary>
    /// Цифровой идентификатор клиента.
    /// </summary>
    public string MndNr { get; set; }

    /// <summary>
    /// Код поставщика/грузополучателя.
    /// </summary>
    public string CustVendId { get; set; }

    /// <summary>
    /// Наименование поставщика/грузополучателя.
    /// </summary>
    public string CustVendName { get; set; }

    /// <summary>
    /// Дополнительная информация (адрес).
    /// </summary>
    public string Info1 { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительная информация.
    /// </summary>
    public string Info2 { get; set; } = string.Empty;

    public TargetShippingClientsModel(string msgNam, string mndNr, string сustVendId, string custVendName)
    {
        MsgNam = msgNam;
        MndNr = mndNr;
        CustVendId = сustVendId;
        CustVendName = custVendName;
    }
}
