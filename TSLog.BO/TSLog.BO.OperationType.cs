namespace TSLog.BO
{
    public enum OperationType
    {
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
}
