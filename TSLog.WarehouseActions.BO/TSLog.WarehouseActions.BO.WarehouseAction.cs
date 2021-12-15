using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

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
        public LegalEntity Partner { get; set; }
        public string PartnerId { get; set; }
        public WarehouseAction(string id, ActionKind kind, LegalEntity partner) 
            : base(-1, id, string.Empty)
        {
            Kind = kind;
            Partner = partner;
        } 
    }
}