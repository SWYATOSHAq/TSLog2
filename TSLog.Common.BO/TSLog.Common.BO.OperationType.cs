using System;

namespace TSLog.Common.BO;

/// <summary>
/// Тип операции.
/// </summary>
[Flags]
public enum OperationType
{
    /// <summary>
    /// Неопределено.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Перемещение товаров.
    /// </summary>
    MovingProducts = 128,

    /// <summary>
    /// Реализация .
    /// </summary>
    Realization = 187,

    /// <summary>
    /// Возврат от комиссионера.
    /// </summary>
    ReturnFromCommissionAgent = 25,

    /// <summary>
    /// Возврат товаров от клиента.
    /// </summary>
    ReturningProductsFromCustomer = 32,

    /// <summary>
    /// Закупка по импорту.
    /// </summary>
    PurchaseByImport = 59
}

public static class OperationConverter
{
    public static OperationType Convert(string value)
    {
        if (!Enum.TryParse(value, out OperationType result))
            result = OperationType.Undefined;
        return result;
    }
}
