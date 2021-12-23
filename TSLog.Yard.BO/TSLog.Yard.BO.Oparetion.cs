using TSLog.Common.BO;
using TSLog.ExceptionHandler;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Операция двора (разгрузка или погрузка автомашины)
    /// </summary>
    public class Operation : Entity
    {
        private const short DefaultPriority = 1;
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new long Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        /// <summary>
        /// Тип
        /// </summary>
        public OperationType Type { get; set; }
        /// <summary>
        /// Фактическая дата и время начала
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Плановая дата и время начала непосредственно работ
        /// </summary>
        public DateTime PlanWorkStartDate { get; private set; }
        public DateTime FactWorkStartDate { get; set; }
        /// <summary>
        /// Плановая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime PlanWorkFinishDate { get; private set; }
        public DateTime FactWorkFinishDate { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания
        /// </summary>
        public DateTime FinishDate { get; set; }
        private IDictionary<Position, short> _workers = new Dictionary<Position, short>();
        public Position[] Positions
        {
            get
            {
                return _workers.Keys.ToArray();
            }
        }
        public short GetWorkers(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            return _workers.ContainsKey(position) ? _workers[position] : (short)0;
        }
        public void AddWorkers(Position position, short count)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            if (!_workers.ContainsKey(position))
                _workers.Add(position, count);
            else
                _workers[position] += count;
        }
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
        public Logist Logist { get; set; } = Logist.Null;
        /// <summary>
        /// Ворота
        /// </summary>
        public Gate Gate { get; set; } = Gate.Null;
        /// <summary>
        /// Состояние (статус)
        /// </summary>
        public Entity State { get; set; }
        /// <summary>
        /// Ответственный менеджер
        /// </summary>
        public Employee Manager { get; set; }
        /// <summary>
        /// Ответственный сотрудник склада
        /// </summary>
        public Employee ResponsibleOfficer { get; set; } = Employee.Null;
        /// <summary>
        /// Приоритет
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// Список партнеров
        /// </summary>
        public Partner[] Partners
        {
            get { return GetPartners(); }
        }

        private readonly IDictionary<string, WarehouseAction> _actions = new Dictionary<string, WarehouseAction>();
        public Operation(long id, string? code, OperationType type,
            DateTime planWorkStartDate, DateTime planWorkFinishDate,
            Entity vehicleKind, string vehicleBrand, string vehicleModel, string vehicleNumber, string driverName,
            Entity state,
            short priority,
            Employee manager)
            : base(id, code)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            PlanWorkStartDate = planWorkStartDate;
            PlanWorkFinishDate = planWorkFinishDate;
            VehicleKind = vehicleKind ?? throw new ArgumentNullException(nameof(vehicleKind));
            if (string.IsNullOrWhiteSpace(vehicleBrand))
                throw new ArgumentException("vehicle brand is empty");
            VehicleBrand = vehicleBrand.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(vehicleModel))
                throw new ArgumentException("vehicle model is empty");
            VehicleModel = vehicleModel.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(vehicleNumber))
                throw new ArgumentException("vehicle number is empty");
            VehicleNumber = vehicleNumber.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(driverName))
                throw new ArgumentException("driver name is empty");
            DriverName = driverName.Trim().ToUpper();
            State = state ?? throw new ArgumentNullException(nameof(state));
            Priority = priority > 0 ? priority : DefaultPriority;
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        private Operation() : base()
        {
            Type = OperationType.Empty;
            PlanWorkStartDate = DateTime.MinValue;
            PlanWorkFinishDate = DateTime.MinValue;
            VehicleKind = Entity.Empty;
            VehicleBrand = string.Empty;
            VehicleModel = string.Empty;
            VehicleNumber = string.Empty;
            DriverName = string.Empty;
            State = Entity.Empty;
            Priority = DefaultPriority;
            Manager = Employee.Empty;
        }
        public WarehouseAction GetAction(ActionKind kind, string id)
        {
            var k = kind == ActionKind.Outgo ? "O" : "I";
            var key = $"{k}{id}";
            if (_actions.ContainsKey(key))
                return _actions[key];
            throw new WarehouseActionNotFoundException(id);
        }
        public WarehouseAction GetAction(ActionKind kind, LegalEntity partner, string partnerActionNumber)
        {
            var actions = _actions.Values.ToList();
            actions.Select(a => a.Kind == kind 
                && a.Partner.Id == partner.Id 
                && a.PartnerActionId == partnerActionNumber).ToArray();
            if (actions.Count > 0)
                return actions[0];
            throw new WarehouseActionNotFoundException(partner.Id, partnerActionNumber);
        }
        public WarehouseAction[] GetActions()
        {
            return _actions.Values.ToArray();
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
        public void AddActions(WarehouseAction[] actions)
        {
            foreach (var action in actions)
                AddAction(action);
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

        public string GetStartTime()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"план. {PlanWorkStartDate:dd.MM.yyyy HH:mm:ss}") +
                (FactWorkStartDate == DateTime.MinValue ? "" : $" (факт.: {FactWorkStartDate:dd.MM.yyyy HH:mm:ss})");
        }
        public string GetFinishTime()
        {
            return (PlanWorkFinishDate == DateTime.MinValue ? "" : $"план. {PlanWorkFinishDate:dd.MM.yyyy HH:mm:ss}") +
                (FactWorkFinishDate == DateTime.MinValue ? "" : $" (факт.: {FactWorkFinishDate:dd.MM.yyyy HH:mm:ss})");
        }
        public string GetGateTime()
        {
            throw new NotImplementedException();
        }
        public string GetWorkTime()
        {
            var from = FactWorkStartDate == DateTime.MinValue ? "//" : $"{FactWorkStartDate:dd.MM.yyyy HH:mm:ss}";
            var to = (FactWorkFinishDate == DateTime.MinValue) ? "//" : $"{FactWorkFinishDate:dd.MM.yyyy HH:mm:ss}";
            return $"{from} - {to}";
        }

        public string GetPlanTime()
        {
            var from = PlanWorkStartDate == DateTime.MinValue ? "//" : $"{PlanWorkStartDate:HH:mm:ss}";
            var to = (PlanWorkFinishDate == DateTime.MinValue) ? "//" : $"{PlanWorkFinishDate:HH:mm:ss}";
            return $"{from} - {to}";
        }
        public Partner[] GetPartners()
        {
            var partners = new HashSet<Partner>();
            foreach (var action in _actions.Values)
            {
                if (!partners.Contains(action.Partner))
                    partners.Add(action.Partner);
            }
            return partners.ToArray();
        }
        private static Operation? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Operation Null
        {
            get
            {
                if (_null == null)
                    _null = new Operation(-1, string.Empty, OperationType.Null,
                        DateTime.MinValue, DateTime.MinValue,
                        Entity.Null, "NULL", "NULL", "NULL", "NULL",
                        Entity.Null,
                        -1,
                        Employee.Null);
                return _null;
            }
        }
        private static Operation? _empty = null;
        /// <summary>
        /// Empty object
        /// </summary>
        public static new Operation Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Operation();
                return _empty;
            }
        }
    }
}