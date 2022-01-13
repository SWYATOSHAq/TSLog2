using TSLog.ExceptionHandler;
using TSLog.Staff.BO;
using static TSLog.Yard.BO.OperationType;

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


        #region Operations
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
        public short MaxParallelOperations(TypeDirection type)
        {

            var temp = new Dictionary<DateTime, short>();
            foreach (var operation in _operations.Values)
            {
                if (operation.Type.Direction == type)
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
            }
            if (!temp.ContainsKey(From))
                temp.Add(From, 0);
            if (!temp.ContainsKey(To))
                temp.Add(To, 0);
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
        #endregion

        #region Workers counts
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
                if (!moments.Contains(From))
                    moments.Add(From);
                if (!moments.Contains(To))
                    moments.Add(To);
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
                            state -= operation.TotalWorkersCount;
                    if (begins.ContainsKey(moment))
                        foreach (var operation in begins[moment])
                            state += operation.TotalWorkersCount;
                    if (moment >= From && result < state)
                        state = result;
                }
                return result;
            }
        }
        /// <summary>
        /// Максимальное количество параллельно работающих сотрудников по должностям
        /// </summary>
        /// <param name="type">Направление</param>
        /// <returns>Количества сотрудников по должностям</returns>
        public IDictionary<Position, short> MaxParallelPositionWorkers(TypeDirection? type = null)
        {
            var begins = new Dictionary<DateTime, List<Operation>>();
            var ends = new Dictionary<DateTime, List<Operation>>();
            var moments = new HashSet<DateTime>();
            foreach (var operation in _operations.Values)
            {
                if (type == null || operation.Type.Direction == type)
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
            }

            if (!moments.Contains(From))
                moments.Add(From);
            if (!moments.Contains(To))
                moments.Add(To);
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
                        var positions = operation.Positions;
                        foreach (var position in positions)
                            state[position] -= operation.GetWorkersCount(position);
                    }
                }
                if (begins.ContainsKey(moment))
                {
                    var operatons = begins[moment];
                    foreach (var operation in operatons)
                    {
                        var positions = operation.Positions;
                        foreach (var position in positions)
                        {
                            var count = operation.GetWorkersCount(position);
                            if (!state.ContainsKey(position))
                                state.Add(position, count);
                            else
                                state[position] += count;
                        }
                    }
                }
                if (moment >= From)
                {
                    var positions = state.Keys.ToArray();
                    foreach (var position in positions)
                    {
                        var count = state[position];
                        if (!result.ContainsKey(position))
                            result.Add(position, count);
                        else if (state[position] > result[position])
                            result[position] = state[position];
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Максимальное количество параллельно работающих сотрудников по должностям
        /// </summary>
        /// <param name="type">Направление</param>
        /// <returns>Количества сотрудников по элементарным работам</returns>
        public IDictionary<Work, short> MaxParallelWorkWorkers(TypeDirection? type = null)
        {
            var begins = new Dictionary<DateTime, List<Operation>>();
            var ends = new Dictionary<DateTime, List<Operation>>();
            var moments = new HashSet<DateTime>();
            foreach (var operation in _operations.Values)
            {
                if (type == null || operation.Type.Direction == type)
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
            }

            if (!moments.Contains(From))
                moments.Add(From);
            if (!moments.Contains(To))
                moments.Add(To);
            var temp = moments.ToArray();
            Array.Sort(temp);
            var result = new Dictionary<Work, short>();
            var state = new Dictionary<Work, short>();
            foreach (var moment in temp)
            {
                if (moment > To)
                    break;
                if (ends.ContainsKey(moment))
                {
                    var operatons = ends[moment];
                    foreach (var operation in operatons)
                    {
                        var works = operation.Works;
                        foreach (var work in works)
                            state[work] -= operation.GetWorkersCount(work);
                    }
                }
                if (begins.ContainsKey(moment))
                {
                    var operatons = begins[moment];
                    foreach (var operation in operatons)
                    {
                        var works = operation.Works;
                        foreach (var work in works)
                        {
                            var count = operation.GetWorkersCount(work);
                            if (!state.ContainsKey(work))
                                state.Add(work, count);
                            else
                                state[work] += count;
                        }
                    }
                }
                if (moment >= From)
                {
                    var works = state.Keys.ToArray();
                    foreach (var work in works)
                    {
                        var count = state[work];
                        if (!result.ContainsKey(work))
                            result.Add(work, count);
                        else if (state[work] > result[work])
                            result[work] = state[work];
                    }
                }
            }
            return result;
        }
        #endregion

        #region Labor costs
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
            foreach (var operation in _operations.Values)
                result += Convert.ToInt32(operation.GetLaborCost(position) * OperationShare(operation));
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
            foreach (var operation in _operations.Values)
                result += Convert.ToInt32(operation.GetLaborCost(work) * OperationShare(operation));
            return result;
        }
        /// <summary>
        /// Трудозатраты по должностям (с человекосекундах)
        /// </summary>
        public IDictionary<Position, int> PositionsLaborCosts
        {
            get
            {
                var result = new Dictionary<Position, int>();
                foreach (var operation in _operations.Values)
                {
                    var costs = operation.PositionsLaborCosts();
                    var share = OperationShare(operation);
                    foreach (var pair in costs)
                    {
                        var cost = Convert.ToInt32(pair.Value * share);
                        if (result.ContainsKey(pair.Key))
                            result[pair.Key] += cost;
                        else
                            result.Add(pair.Key, cost);
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// Трудозатраты по элементарным работам (с человекосекундах)
        /// </summary>
        public IDictionary<Work, int> WorksLaborCosts
        {
            get
            {
                var result = new Dictionary<Work, int>();
                foreach (var operation in _operations.Values)
                {
                    var costs = operation.WorksLaborCosts();
                    var share = OperationShare(operation);
                    foreach (var pair in costs)
                    {
                        var cost = Convert.ToInt32(pair.Value * share);
                        if (result.ContainsKey(pair.Key))
                            result[pair.Key] += cost;
                        else
                            result.Add(pair.Key, cost);
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// Общие трудозатраты в человекосекундах
        /// </summary>
        public int TotalLaborCost
        {
            get
            {
                var result = 0;
                foreach (var operation in _operations.Values)
                    result += Convert.ToInt32(operation.TotalLaborCost * OperationShare(operation));
                return result;
            }
        }
        #endregion


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

        /// <summary>
        /// Возвращает долю операции, которая приходтся на данный интервал
        /// </summary>
        /// <param name="operation">Операция двора (ПРР)</param>
        /// <returns>Доля</returns>
        public double OperationShare(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            var from = From >= operation.PlanWorkStartDate ? From : operation.PlanWorkStartDate;
            var to = To <= operation.PlanWorkFinishDate ? To : operation.PlanWorkFinishDate;
            var duration = from < to ? to - from : TimeSpan.Zero;
            var operationDuration = operation.PlanWorkFinishDate - operation.PlanWorkStartDate;
            return duration / operationDuration;
        }
        public string GetFormattedTimeString(bool simplified = true)
        {
            string ret = simplified ? $"{From:HH} - {To:HH}" : $"{From:HH\\:mm\\ss} - {To:HH\\:mm\\ss}";
            return ret;
        }
    }
}
