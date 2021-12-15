using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Операция двора (разгрузка или погрузка автомашины)
    /// </summary>
    public class Operation : Entity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new long Id 
        {
            get { return base.Id; }
            set { base.Id = value; } 
        }
        /// <summary>
        /// Тип
        /// </summary>
        public OperationType Type { get; set; }
        /// <summary>
        /// Планования дата и время начала
        /// </summary>
        public DateTime PlanStartDate { get; set; }
        /// <summary>
        /// Фактическая дата и время начала
        /// </summary>
        public DateTime FactStartDate { get; set; }
        /// <summary>
        /// Дата и время постановки на ворота
        /// </summary>
        public DateTime GateArrivalDate { get; set; }
        /// <summary>
        /// Дата и время начала непосредственно работ
        /// </summary>
        public DateTime WorkStartDate { get; set; }
        /// <summary>
        /// Дата и время окончания непосредственно работ
        /// </summary>
        public DateTime WorkEndDate { get; set; }
        /// <summary>
        /// Дата и время отбытия автомашины с ворот
        /// </summary>
        public DateTime GateDepartureDate { get; set; }
        /// <summary>
        /// Плановая дата и время окончания
        /// </summary>
        public DateTime PlanFinshDate { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания
        /// </summary>
        public DateTime FactFinishdate { get; set; }
        /// <summary>
        /// Вид автомашины
        /// </summary>
        public Entity VehicleKind { get; set; }
        /// <summary>
        /// Марка автомашины
        /// </summary>
        public string VehicleBrand { get; set; }
        /// <summary>
        /// Модель автомашины
        /// </summary>
        public string VehicleModel { get; set; }
        /// <summary>
        /// Номер автомашины
        /// </summary>
        public string VehicleNumber { get; set; }
        /// <summary>
        /// Ф. И. О. шофёра
        /// </summary>
        public string DriverName { get; set; }
        /// <summary>
        /// Логист (транспортно-логистическая компания)
        /// </summary>
        public Entity Logist { get; set; }
        /// <summary>
        /// Ворота
        /// </summary>
        public Gate Gate { get; set; }
        /// <summary>
        /// Состояние (статус)
        /// </summary>
        public Entity State { get; set; }
        /// <summary>
        /// Ответственный менеджер
        /// </summary>
        public Employee Manager { get; set; }
        public Employee ResponsibleOfficer { get; set; }
        /// <summary>
        /// Приоритет
        /// </summary>
        public int Priority { get; set; }


        private IDictionary<string, WarehouseAction> _actions = null;
        public Operation(long id, string code) 
            : base(id, code)
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
        {
            if (_actions == null)
                throw new ArgumentNullException(nameof(action));

            var kind = action.Kind == ActionKind.Outgo ? "O" : "I";
            var key = $"{kind}{action.Id}";
            if (!_actions.ContainsKey(key))
                _actions.Add(key, action);
        }
        public void RemoveAction(WarehouseAction action)
        {
            if (_actions == null)
                throw new ArgumentNullException(nameof(action));

            var kind = action.Kind == ActionKind.Outgo ? "O" : "I";
            var key = $"{kind}{action.Id}";
            if (_actions.ContainsKey(key))
                _actions.Remove(key);
        }
    }
}