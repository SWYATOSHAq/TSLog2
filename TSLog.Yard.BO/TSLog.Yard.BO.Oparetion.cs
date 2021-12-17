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
        private const short DefauktWorkersCount = 2;
        private const short DefaultPriority = 1;
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
        public DateTime WorkFinishDate { get; set; }
        /// <summary>
        /// Дата и время отбытия автомашины с ворот
        /// </summary>
        public DateTime GateDepartureDate { get; set; }
        /// <summary>
        /// Плановая дата и время окончания
        /// </summary>
        public DateTime PlanFinishDate { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания
        /// </summary>
        public DateTime FactFinishDate { get; set; }
        /// <summary>
        /// Количество работников для выполнения операции
        /// </summary>
        public short WorkersCount { get; set; }
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
        public Employee ResponsibleOfficer { get; set; } = Employee.Null;
        /// <summary>
        /// Приоритет
        /// </summary>
        public int Priority { get; set; }


        private readonly IDictionary<string, WarehouseAction> _actions = new Dictionary<string, WarehouseAction>();
        public Operation(long id, string? code, OperationType type, 
            DateTime planStartDate, DateTime planFinishDate,
            short workersCount, 
            Entity vehicleKind, string vehicleBrand, string vehicleModel, string vehicleNumber, string driverName,
            Entity state,
            short priority,
            Employee manager) 
            : base(id, code)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            PlanStartDate = planStartDate;
            PlanFinishDate = planFinishDate;
            WorkersCount = workersCount > 0 ? workersCount : DefauktWorkersCount;
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
            throw new NotImplementedException();
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
            return (PlanStartDate == DateTime.MinValue ? "" : $"план. {PlanStartDate:dd.MM.yyyy HH:mm:ss}") + (FactStartDate == DateTime.MinValue ? "" : $" (факт.: {FactStartDate:dd.MM.yyyy HH:mm:ss})");
        }
        public string GetFinishTime()
        {
            return (PlanFinishDate == DateTime.MinValue ? "" : $"план. {PlanFinishDate:dd.MM.yyyy HH:mm:ss}") + (FactFinishDate == DateTime.MinValue ? "" : $" (факт.: {FactFinishDate:dd.MM.yyyy HH:mm:ss})");
        }
        public string GetGateTime()
        {
            var from = GateArrivalDate == DateTime.MinValue ? "//" : $"{GateArrivalDate:dd.MM.yyyy HH:mm:ss}";
            var to = GateDepartureDate == DateTime.MinValue ? "//" : $"{GateDepartureDate:dd.MM.yyyy HH:mm:ss}";
            return $"{from} - {to}";
        }
        public string GetWorkTime()
        {
            var from = WorkStartDate == DateTime.MinValue ? "//" : $"{WorkStartDate:dd.MM.yyyy HH:mm:ss}";
            var to = (WorkFinishDate == DateTime.MinValue) ? "//" : $"{WorkFinishDate:dd.MM.yyyy HH:mm:ss}";
            return $"{from} - {to}";
        }

        public string GetPlanTime()
        {
            var from = PlanStartDate == DateTime.MinValue ? "//" : $"{PlanStartDate:HH:mm:ss}";
            var to = (PlanFinishDate == DateTime.MinValue) ? "//" : $"{PlanFinishDate:HH:mm:ss}";
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
                    _null = new Operation(-1, "NULL", OperationType.Null, 
                        DateTime.MinValue, DateTime.MinValue,
                        -1,
                        Entity.Null, "NULL", "NULL", "NULL", "NULL",
                        Entity.Null,
                        -1,
                        Employee.Null);
                return _null;
            }
        }
    }
}