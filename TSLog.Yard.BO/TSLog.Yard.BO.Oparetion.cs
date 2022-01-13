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
        /// <summary>
        /// Фактическая дата и время начала непосредственно работ
        /// </summary>
        public DateTime FactWorkStartDate { get; set; }
        /// <summary>
        /// Плановая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime PlanWorkFinishDate { get; private set; }
        /// <summary>
        /// Фактическая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime FactWorkFinishDate { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания
        /// </summary>
        public DateTime FinishDate { get; set; }


        #region Operation works
        /// <summary>
        /// Элементарная работа операции двора (ПРР)
        /// </summary>
        public class OperationWork
        {
            private const short DefaultTime = 600;
            private const short DefaultWorkersCount = 1;
            public Work Work { get; set; }
            public Position Position { get; set; }
            public short Time { get; set; }
            public short WorkersCount { get; set; }
            public OperationWork(Work work, Position position, short time, short workersCount)
            {
                Work = work;
                Position = position;
                Time = time > 0 ? time : DefaultTime;
                WorkersCount = workersCount > 0 ? workersCount : DefaultWorkersCount;
            }
        }
        private readonly IList<OperationWork> _operationWorks = new List<OperationWork>();
        /// <summary>
        /// Добавление работы в операцию двора (ПРР)
        /// </summary>
        /// <param name="work">Работа</param>        
        /// <exception cref="ArgumentNullException"></exception>
        public void AddOperationWork(OperationWork operationWork)
        {
            if (operationWork == null)
                throw new ArgumentNullException(nameof(operationWork));
            _operationWorks.Add(operationWork);
        }
        /// <summary>
        /// Добавление нескольких работ в операцию двора ПРР
        /// </summary>
        /// <param name="operationWorks"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddOperationWorks(OperationWork[] operationWorks)
        {
            if (operationWorks == null)
                throw new ArgumentNullException(nameof(operationWorks));
            foreach (OperationWork operationWork in operationWorks)
                AddOperationWork(operationWork);
        }
        /// <summary>
        /// Должности, задействованные на данной операции двора
        /// </summary>
        public Position[] Positions
        {
            get
            {
                var positions = new HashSet<Position>();
                foreach (var operationWork in _operationWorks)
                    if (!positions.Contains(operationWork.Position))
                        positions.Add(operationWork.Position);
                return positions.ToArray();
            }
        }
        /// <summary>
        /// Элементарные работы по данной операции двора (ПРР)
        /// </summary>
        public Work[] Works
        {
            get
            {
                var works = new HashSet<Work>();
                foreach (var operationWork in _operationWorks)
                    if (!works.Contains(operationWork.Work))
                        works.Add(operationWork.Work);
                return works.ToArray();
            }
        }
        /// <summary>
        /// Работы операции двора (ПРР)
        /// </summary>
        public OperationWork[] OperationWorks
        {
            get
            {
                return _operationWorks.ToArray();
            }
        }
        #endregion

        #region Workers count
        /// <summary>
        /// Количество работников по определённой должности
        /// </summary>
        /// <param name="position">Должность</param>
        /// <returns>Уоличество работников</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public short GetWorkersCount(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            var result = (short)0;
            foreach (var operationWork in _operationWorks)
                if (operationWork.Position == position && result < operationWork.WorkersCount)
                    result = operationWork.WorkersCount;
            return result;
        }
        /// <summary>
        /// Количество работников, выполняющих определённой элементарную работу
        /// </summary>
        /// <param name="work">Элементарная работа</param>
        /// <returns>Количество работников</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public short GetWorkersCount(Work work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            var result = (short)0;
            foreach (var operationWork in _operationWorks)
                if (operationWork.Work == work && result < operationWork.WorkersCount)
                    result = operationWork.WorkersCount;
            return result;
        }
        /// <summary>
        /// Количество задействованных работников по должностям
        /// </summary>
        /// <returns></returns>
        public IDictionary<Position, short> PositionsWorkersCount()
        {
            var result = new Dictionary<Position, short>();
            foreach (var operationWork in _operationWorks)
            {
                var position = operationWork.Position;
                var count = operationWork.WorkersCount;
                if (!result.ContainsKey(position))
                    result.Add(position, count);
                else if (result[position] > count)
                    result[position] = count;
            }
            return result;
        }
        /// <summary>
        /// Количество задействованых работников по элементарным радотам
        /// </summary>
        public IDictionary<Work, short> WorksWorkersCount()
        {
            var result = new Dictionary<Work, short>();
            foreach (var operationWork in _operationWorks)
            {
                var work = operationWork.Work;
                var count = operationWork.WorkersCount;
                if (!result.ContainsKey(work))
                    result.Add(work, count);
                else if (result[work] > count)
                    result[work] = count;
            }
            return result;
        }
        /// <summary>
        /// Общее количество сотрудников для операции двора (ПРР) по всем должностям
        /// </summary>
        public short TotalWorkersCount
        {
            get
            {
                var result = (short)0;
                var counts = new Dictionary<Position, short>();
                foreach (var operastionWork in _operationWorks)
                {
                    var position = operastionWork.Position;
                    var count = operastionWork.WorkersCount;
                    if (!counts.ContainsKey(position))
                        counts.Add(position, count);
                    else
                        counts[position] += count;
                }
                foreach (var count in counts.Values)
                    result += count;
                return result;
            }
        }
        #endregion

        #region Labor cost
        /// <summary>
        /// Возвращает трудозатраты (в человекосекундах) для конкретной должности
        /// </summary>
        /// <param name="position">Должности</param>
        /// <returns>Трудозатраты (в человекосекундах)</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int GetLaborCost(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            var result = 0;
            foreach (var operationWork in _operationWorks)
                if (operationWork.Position == position)
                    result += operationWork.WorkersCount * operationWork.Time;
            return result;
        }
        /// <summary>
        /// Возвращает трудозатраты (в человекосекундах) для конертной элементарной работы
        /// </summary>
        /// <param name="work">Трудозатраты</param>
        /// <returns>Трудозатраты (в человекосекундах)</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int GetLaborCost(Work work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            var result = 0;
            foreach (var operationWork in _operationWorks)
                if (operationWork.Work == work)
                    result += operationWork.WorkersCount * operationWork.Time;
            return result;
        }
        /// <summary>
        /// Трудозатраты по должностям (с человекосекундах)
        /// </summary>
        public IDictionary<Position, int> PositionsLaborCosts()
        {
            var result = new Dictionary<Position, int>();
            foreach (var operstionWork in _operationWorks)
            {
                var position = operstionWork.Position;
                var labor = operstionWork.WorkersCount * operstionWork.Time;
                if (!result.ContainsKey(position))
                    result.Add(position, labor);
                else
                    result[position] += labor;
            }
            return result;
        }
        /// <summary>
        /// Трудозатраты по элементарным работам (с человекосекундах)
        /// </summary>
        public IDictionary<Work, int> WorksLaborCosts()
        {
            var result = new Dictionary<Work, int>();
            foreach (var operstionWork in _operationWorks)
            {
                var work = operstionWork.Work;
                var labor = operstionWork.WorkersCount * operstionWork.Time;
                if (!result.ContainsKey(work))
                    result.Add(work, labor);
                else
                    result[work] += labor;
            }
            return result;
        }
        /// <summary>
        /// Общие трудозатраты в человекосекундах
        /// </summary>
        public int TotalLaborCost
        {
            get
            {
                var result = 0;
                foreach (var operationWork in _operationWorks)
                    result += operationWork.Time * operationWork.WorkersCount;
                return result;
            }
        }
        #endregion

        #region Operation workers
        public class OperationWorker
        {
            public Employee Worker { get; private set; }
            public Position Position { get; private set; }
            public OperationWorker(Employee worker, Position position)
            {
                if (worker == null)
                    throw new ArgumentNullException(nameof(worker));
                if (position == null)
                    throw new ArgumentNullException(nameof(position));
                Worker = worker;
                Position = position;
            }
        }
        private readonly IList<OperationWorker> _operationWorkers = new List<OperationWorker>();
        public void AddOperationWorker(Employee worker, Position position)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            var operationWorker = new OperationWorker(worker, position);
            AddOperationWorker(operationWorker);
        }
        public void AddOperationWorker(OperationWorker operationWorker)
        { 
            if (operationWorker == null)
                throw new ArgumentNullException(nameof(operationWorker));

            // TODO: check existance
            _operationWorkers.Add(operationWorker);
        }
        public void AddOperationWorkers(OperationWorker[] operationWorkers)
        {
            if (operationWorkers == null)
                throw new ArgumentNullException(nameof(operationWorkers));
            foreach (var operationWorker in operationWorkers)
                AddOperationWorker(operationWorker);
        }
        public void RemoveOperationWorker(Employee worker, Position position)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            foreach (var operationWorker in _operationWorkers)
                if (operationWorker.Worker == worker && operationWorker.Position == position)
                    _operationWorkers.Remove(operationWorker);
        }
        public OperationWorker[] OperationWorkers
        {
            get
            {
                return _operationWorkers.ToArray();
            }
        }
        #endregion

        /// <summary>
        /// Вид автомашины
        /// </summary>
        public VehicleKind VehicleKind { get; set; }
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
            VehicleKind vehicleKind, string vehicleBrand, string vehicleModel, string vehicleNumber, string driverName,
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
            VehicleKind = VehicleKind.Empty;
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
            var action = actions.Find(a => a.Kind == kind
                && a.Partner.Id == partner.Id
                && a.PartnerActionId == partnerActionNumber);
            if (action != null)
                return action;
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
        //Getdate TODO
        public string GetDate()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"( {PlanWorkStartDate:dd.MM.yyyy})");
        }
        public string GetStartTime()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"план. {PlanWorkStartDate:dd.MM.yyyy HH:mm}") +
                (FactWorkStartDate == DateTime.MinValue ? "" : $" (факт.: {FactWorkStartDate:dd.MM.yyyy HH:mm})");
        }
        public string GetFinishTime()
        {
            return (PlanWorkFinishDate == DateTime.MinValue ? "" : $"план. {PlanWorkFinishDate:dd.MM.yyyy HH:mm}") +
                (FactWorkFinishDate == DateTime.MinValue ? "" : $" (факт.: {FactWorkFinishDate:dd.MM.yyyy HH:mm})");
        }
        public string GetGateTime()
        {
            throw new NotImplementedException();
        }
        public string GetWorkTime()
        {
            var from = FactWorkStartDate == DateTime.MinValue ? "//" : $"{FactWorkStartDate:dd.MM.yyyy HH:mm}";
            var to = (FactWorkFinishDate == DateTime.MinValue) ? "//" : $"{FactWorkFinishDate:dd.MM.yyyy HH:mm}";
            return $"{from} - {to}";
        }

        public string GetPlanTime()
        {
            var from = PlanWorkStartDate == DateTime.MinValue ? "//" : $"{PlanWorkStartDate:HH:mm}";
            var to = (PlanWorkFinishDate == DateTime.MinValue) ? "//" : $"{PlanWorkFinishDate:HH:mm}";
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
                        VehicleKind.Null, "NULL", "NULL", "NULL", "NULL",
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