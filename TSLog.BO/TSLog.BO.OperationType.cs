using System;

namespace TSLog.BO
{
    /// <summary>
    /// Тип операции
    /// </summary>
    [Flags]
    public enum OperationType
    {
        Undefined = 0,

        /// <summary>
        /// Перемещение товаров
        /// </summary>
        MovingProducts = 128,

        /// <summary>
        /// Реализация 
        /// </summary>
        Realization = 187,

        /// <summary>
        /// Возврат от комиссионера
        /// </summary>
        ReturnFromCommissionAgent = 25,

        /// <summary>
        /// Возврат товаров от клиента
        /// </summary>
        ReturningProductsFromCustomer = 32,

        /// <summary>
        /// Закупка по импорту
        /// </summary>
        PurchaseByImport = 59
    }

    public static class OperationConverter
    {
        public static OperationType Convert(string str)
        {
            OperationType ret;
            if (!Enum.TryParse(str, out ret))
                ret = OperationType.Undefined;

            return ret;
        }
    }
}