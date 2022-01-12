using TSLog.ExceptionHandler;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Временное окно для ПРР
    /// </summary>
    public class Interval
    {
        private const int DefaultIntervalSize = 3600;
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
        /// Добавление нескольких операций в интервал
        /// </summary>
        /// <param name="operations">Массив операций</param>
        public void AddOperations(Operation[] operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));
            foreach (var operation in operations)
                AddOperation(operation);
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
                var temp = new Dictionary<DateTime, short>();
                foreach (var operation in _operations.Values)
                {
                    if (!temp.ContainsKey(operation.PlanWorkStartDate))
                        temp.Add(operation.PlanWorkStartDate, 1);
                    else
                        temp[operation.PlanWorkStartDate]++;
                    if (!temp.ContainsKey(operation.PlanWorkFinishDate))
                        temp.Add(operation.PlanWorkFinishDate, -1);
                    else
                        temp[operation.PlanWorkFinishDate]--;
                }
                var moments = temp.Keys.ToArray();
                Array.Sort(moments);
                var result = (short)0;
                var state = (short)0;
                foreach (var moment in moments)
                {
                    if (moment > To)
                        break;
                    state += temp[moment];
                    if (state > result && moment >= From)
                        result = state;
                }
                return result;
            }
        }
        /// <summary>
        /// Максимальное количество параллельно работающих сотрудников
        /// </summary>
        public short MaxParallelWorkers
        {
            get
            {
                var begins = new Dictionary<DateTime, List<Operation>>();
                var ends = new Dictionary<DateTime, List<Operation>>();
                var moments = new HashSet<DateTime>();
                foreach (var operation in _operations.Values)
                {
                    if (!begins.ContainsKey(operation.PlanWorkStartDate))
                        begins.Add(operation.PlanWorkStartDate, new List<Operation>());
                    begins[operation.PlanWorkStartDate].Add(operation);

                    if (!ends.ContainsKey(operation.PlanWorkFinishDate))
                        ends.Add(operation.PlanWorkFinishDate, new List<Operation>());
                    ends[operation.PlanWorkFinishDate].Add(operation);

                    if (!moments.Contains(operation.PlanWorkStartDate))
                        moments.Add(operation.PlanWorkStartDate);
                    if (!moments.Contains(operation.PlanWorkFinishDate))
                        moments.Add(operation.PlanWorkFinishDate);
                }
                var temp = moments.ToArray();
                Array.Sort(temp);
                var result = (short)0;
                var state = (short)0;
                foreach (var moment in temp)
                {
                    if (moment > To)
                        break;
                    if (ends.ContainsKey(moment))
                        foreach (var operation in ends[moment])
                            state -= operation.WorkersCount;
                    if (begins.ContainsKey(moment))
                        foreach (var operation in begins[moment])
                            state += operation.WorkersCount;
                    if (moment >= From && result < state)
                        state = result;
                }
                return result;
            }
        }
        /// <summary>
        /// Максимальное количество параллельно работающих сотрудников по должностям
        /// </summary>
        public IDictionary<Position, short> MaxParallelPositionWorkers
        {
            get
            {
                return null;
                /*
                var begins = new Dictionary<DateTime, List<Operation>>();
                var ends = new Dictionary<DateTime, List<Operation>>();
                var moments = new HashSet<DateTime>();
                foreach (var operation in _operations.Values)
                {
                    if (!begins.ContainsKey(operation.PlanWorkStartDate))
                        begins.Add(operation.PlanWorkStartDate, new List<Operation>());
                    begins[operation.PlanWorkStartDate].Add(operation);

                    if (!ends.ContainsKey(operation.PlanWorkFinishDate))
                        ends.Add(operation.PlanWorkFinishDate, new List<Operation>());
                    ends[operation.PlanWorkFinishDate].Add(operation);

                    if (!moments.Contains(operation.PlanWorkStartDate))
                        moments.Add(operation.PlanWorkStartDate);
                    if (!moments.Contains(operation.PlanWorkFinishDate))
                        moments.Add(operation.PlanWorkFinishDate);
                }

                var temp = moments.ToArray();
                Array.Sort(temp);
                var result = new Dictionary<Position, short>();
                var state = new Dictionary<Position, short>();
                foreach (var moment in temp)
                {
                    if (moment > To)
                        break;
                    if (ends.ContainsKey(moment))
                    {
                        var operatons = ends[moment];
                        foreach (var operation in operatons)
                        {
                            var positions = operation.Works;
                            foreach (var position in positions)
                                state[position] -= operation.GetWorkersCount(position);
                        }
                    }
                    if (begins.ContainsKey(moment))
                    {
                        var operatons = begins[moment];
                        foreach (var operation in operatons)
                        {
                            var positions = operation.Works;
                            foreach (var position in positions)
                            {
                                var count = operation.GetWorkersCount(position);
                                if (!state.ContainsKey(position))
                                    state.Add(position, count);
                                else
                                    state[position] += count;
                                if (moment >= From)
                                {
                                    if (!result.ContainsKey(position))
                                        result.Add(position, count);
                                    else if (state[position] > result[position])
                                        result[position] = state[position];
                                }
                            }
                        }
                    }
                }
                return result;
                */
            }
        }
        /// <summary>
        /// Разбивка интервала на более мелкие интервалы с указанной протяжённостью
        /// </summary>
        /// <param name="size">Протяжёнсть интервала в секундах</param>
        /// <returns>Массив более мелких интервалов</returns>
        public Interval[] Split(int size = DefaultIntervalSize)
        {
            if (size <= 0)
                size = DefaultIntervalSize;
            var intervals = new List<Interval>();
            DateTime from;
            var to = From;
            do
            {
                from = to;
                to = to.AddSeconds(size);
                to = to > To ? To : to;
                var interval = new Interval(from, to);
                var operations = Operations;
                foreach (var operation in operations)
                    if (interval.ContainsOperation(operation))
                        interval.AddOperation(operation);
                intervals.Add(interval);
            }
            while (to < To);
            return intervals.ToArray();
        }

        /// <summary>
        /// Объединение интервалов
        /// </summary>
        /// <param name="intervals">Массив интервалов</param>
        /// <returns>Массив объединённых интервалов (без пересечений)</returns>
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
            var from = DateTime.MinValue;
            DateTime to;
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
        /// <summary>
        ///  Проверка пересечения текущего интервала с другим
        /// </summary>
        /// <param name="other">Другой интервал</param>
        /// <returns>Признак пересечения</returns>
        public bool HasIntersection(Interval other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return !(From > other.To || To < other.From);
        }
        /// <summary>
        /// Возвращает признак "попадания" опеарции двора (ПРР) в текущий интервал
        /// </summary> 
        /// <param name="operation">Операция двора (ПРР)</param>
        /// <returns>Признак попадания</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ContainsOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            return !(From > operation.PlanWorkFinishDate || To < operation.PlanWorkStartDate);
        }
        public string GetFormattedTimeString(bool simplified = true)
        {
            var result = simplified ? $"{From:HH} - {To:HH}" : $"{From:HH\\:mm\\ss} - {To:HH\\:mm\\ss}";
            return result;
        }
    }
}
