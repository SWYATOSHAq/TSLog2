using TSLog.Common.BO;
using TSLog.Partners.BO;

namespace TSLog.WarehouseActions.BO
{
    /// <summary>
    /// Складское действо (приход, расход, ...)
    /// </summary>
    public class WarehouseAction : Entity
    {
        /// <summary>
        /// Идентификаитор
        /// </summary>
        public new string Id
        {
            get 
            { 
                return Code;  
            }
            protected set 
            { 
                Code = value; 
            }
        }
        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; private set; }
        /// <summary>
        /// Вид (приход, расход, ...)
        /// </summary>
        public ActionKind Kind { get; private set; }
        public string Key
        {
            get
            {
                var kind = Kind == ActionKind.Outgo ? "O" : "I";
                return $"{kind}{Id}";
            }
        }
        /// <summary>
        /// Партнёр - покупатель наших складских и логистических услуг
        /// </summary>
        public Partner Partner { get; set; }
        /// <summary>
        /// Идентификатор складского действа в системе партнёра
        /// </summary>
        public string PartnerActionId { get; set; }
        public WarehouseAction(ActionKind kind, string id, string number, Partner partner, string partnerActionId, 
            bool isActive, string note) 
            : base(-1, id, string.Empty, string.Empty, string.Empty, string.Empty, isActive, -1, note)
        {
            //if (string.IsNullOrWhiteSpace(number))
            //    throw new ArgumentException("number is empty");
            Number = number.Trim();
            Kind = kind;
            Partner = partner;
            PartnerActionId = partnerActionId;
        }
        private WarehouseAction() : base()
        {
            Number = string.Empty;
            Partner = Partner.Empty;
            PartnerActionId = string.Empty;
        }
        private static WarehouseAction? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new WarehouseAction Null
        {
            get
            {
                if (_null == null)
                    _null = new WarehouseAction(ActionKind.Income, "NULL", "NULL", Partner.Null, "NULL", true, "NULl");
                return _null;
            }
        }
        private static WarehouseAction? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new WarehouseAction Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new WarehouseAction();
                return _empty;
            }
        }
    }
}