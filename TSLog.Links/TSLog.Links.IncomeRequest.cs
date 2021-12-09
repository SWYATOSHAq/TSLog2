using System;
using TSLog.Incomes.BO;
using TSLog.LegalEntities.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Сообщение запрос на приход товара на склад
    /// </summary>
    public class IncomeRequest : Message
    {
        /// <summary>
        /// Приход товара на склад
        /// </summary>
        public Income Income { get; private set; }

        /// <summary>
        /// Продавец складских и логистических услуг
        /// </summary>
        public LegalEntity ServiceSeller { get; private set; }

        /// <summary>
        /// Покупатель складских и логистических услуг
        /// </summary>
        public LegalEntity ServiceBuyer { get; private set; }

        /// <summary>
        /// Отправитель груза
        /// </summary>
        public LegalEntity Shipper { get; private set; }

        /// <summary>
        /// Получатель груза
        /// </summary>
        public LegalEntity Receiver { get; private set; }

        public IncomeRequest(long id, string number, byte version, DateTime date, Income income,
            LegalEntity sender, LegalEntity recipient,
            LegalEntity serviceSeller, LegalEntity serviceBuyer, LegalEntity shipper, LegalEntity receiver,
            string note = "")
            : base(id, number, 'I', "INCOME_REQUEST", version, date, sender, recipient, note)
        {
            Income = income ?? throw new ArgumentNullException(nameof(income));
            ServiceSeller = serviceSeller ?? throw new ArgumentNullException(nameof(serviceSeller));
            ServiceBuyer = serviceBuyer ?? throw new ArgumentNullException(nameof(serviceBuyer));
            Shipper = shipper ?? throw new ArgumentNullException(nameof(shipper));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }
    }
}