using TSLog.ExceptionHandler;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Временное окно для ПРР
    /// </summary>
    public class Interval
    {
        private const int DefaultIntervalSize = 60;
        /// <summary>
        /// Дата и время начала
        /// </summary>
        public DateTime From { get; private set; }
        /// <summary>
        /// Дата и время окончания
        /// </summary>
        public DateTime To { get; private set; }
        /// <summary>
        /// Ворота
        /// </summary>
        public Gate Gate { get; private set; } = Gate.Null;
        public Interval(DateTime from, DateTime to, Gate? gate = null)
        {
            if (gate == null)
                gate = Gate.Null;
            if (from >= to)
                throw new ArgumentException("from more than to");
            From = from;
            To = to;
            Gate = gate;
        }
        private readonly IDictionary<long, Operation> _operations = new Dictionary<long, Operation>();
        /// <summary>
        /// Добавдение операции двора в интервал
        /// </summary>
        /// <param name="operation">Операция</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            // TODO: check begin and end time
            if (!_operations.ContainsKey(operation.Id))                
                _operations.Add(operation.Id, operation);
        }
        /// <summary>
        /// Возвращает операцию двора (ПРР) по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Операция двора (ПРР)</returns>
        public Operation GetOperation(long id)
        {
            id = id > 0 ? id : -1;
            if (_operations.ContainsKey(id))
                return _operations[id];
            throw new OperationNotFoundException(id);
        }
        /// <summary>
        /// Удаление операции двора (ПРР) из интервала
        /// </summary>
        /// <param name="operation">Операция двора (прр)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RemoveOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (_operations.ContainsKey(operation.Id))
                _operations.Remove(operation.Id);
        }
        /// <summary>
        /// Все операции затрагиваемые в указанный период
        /// </summary>
        public Operation[] Operations
        {
            get
            {                
                return _operations.Values.ToArray();
            }
        }
        /// <summary>
        /// ВКоличество операций, затрагиваемых в указанный период
        /// </summary>
        public int OperationsCount
        {
            get 
            { 
                return _operations.Values.Count; 
            }
        }
        /// <summary>
        /// Максимальное количество одновременных операций за период
        /// </summary>
        public short MaxParallelOperations
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// Максимальное количество параллельно работающих сотрудников
        /// </summary>
        public short MaxParallelWorkers
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// Максимальное количество параллельно работающих сотрудников по должностям
        /// </summary>
        public IDictionary<Position, short> MaxParallelPositionWorkers
        {
            get
            {
                var positionWorkers = new Dictionary<Position, short>();
                return positionWorkers;
            }
        }
        /// <summary>
        /// Разбивка интервала на более мелкие интервалы с указанной протяжённостью
        /// </summary>
        /// <param name="size">Протяжёнсть интервала в секундах</param>
        /// <returns>Массив более мелких интервалов</returns>
        public Interval[] Split(int size = DefaultIntervalSize)
        {
            var intervals = new List<Interval>();
            return intervals.ToArray();
        }

        /// <summary>
        /// Объединение интервалов
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static Interval[] Union(Interval[] intervals)
        {
            var result = new List<Interval>();
            var moments = new Dictionary<DateTime, short>();
            foreach (var interval in intervals)
            {
                if (!moments.ContainsKey(interval.From))
                    moments.Add(interval.From, 1);
                else
                    moments[interval.From] += 1;
                if (!moments.ContainsKey(interval.To))
                    moments.Add(interval.To, -1);
                else
                    moments[interval.To] -= 1;
            }
            var dates = moments.Keys.ToList();
            dates.Sort();
            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MinValue;
            short state = 0;
            foreach (var date in dates)
            {
                state += moments[date];
                if (state > 0 && from == DateTime.MinValue)
                    from = date;
                else if (state <= 0 && from != DateTime.MinValue)
                {
                    to = date;
                    var interval = new Interval(from, to);
                    result.Add(interval);
                    from = DateTime.MinValue;
                    to = DateTime.MinValue;
                }
            }
            return result.ToArray();
        }
    }
}
