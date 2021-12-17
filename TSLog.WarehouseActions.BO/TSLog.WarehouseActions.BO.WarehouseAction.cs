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
    }
}