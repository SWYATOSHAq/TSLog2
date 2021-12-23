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
            get { return Code;  }
            protected set { Code = value; }
        }
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
        public WarehouseAction(ActionKind kind, string id, Partner partner, string partnerActionId) 
            : base(-1, id, string.Empty)
        {
            Kind = kind;
            Partner = partner;
            PartnerActionId = partnerActionId;
        }
        private WarehouseAction() : base()
        {
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
                    _null = new WarehouseAction(ActionKind.Income, "NULL", Partner.Null, "NULL");
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