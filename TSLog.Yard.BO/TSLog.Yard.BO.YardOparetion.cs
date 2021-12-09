using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Операция двора (разгрузка или погрузка автомашины)
    /// </summary>
    public class YardOperation : Entity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new long Id 
        {
            get { return base.Id; }
            set { base.Id = value; } 
        }
        public Entity Type { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime FactStartDate { get; set; }
        public DateTime GateArrivalDate { get; set; }
        public DateTime WorkStartDate { get; set; }
        public DateTime WorkEndDate { get; set; }
        public DateTime GateDepartureDate { get; set; }
        public DateTime PlanFinshDate { get; set; }
        public DateTime FactFinishdate { get; set; }
        public Entity VehicleKind { get; set; }
        public string VehicleBrand { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverName { get; set; }
        public Entity Logist { get; set; }
        public Gate Gate { get; set; }
        public Entity State { get; set; }
        public Employee Manager { get; set; }
        public Employee ResponsibleOfficer { get; set; }
        public int Priority { get; set; }


        private IDictionary<string, WarehouseAction> _actions = null;
        public YardOperation(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", bool isActive = true) 
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        {
            _actions = new Dictionary<string, WarehouseAction>();
        }
        public WarehouseAction GetAction(ActionKind kind, string id)
        {
            return null;
        }
        public WarehouseAction GetAction(ActionKind kind, LegalEntity partner, string partnerActionNumber)
        {
            return null;
        }
        public WarehouseAction[] GetActions()
        {
            var actions = new List<WarehouseAction>();
            return actions.ToArray();
        }
        public void AddAction(WarehouseAction action)
        { }
        public void RemoveAction(WarehouseAction action)
        { }
    }
}