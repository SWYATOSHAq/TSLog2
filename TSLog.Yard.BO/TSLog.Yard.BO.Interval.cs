namespace TSLog.Yard.BO
{
    /// <summary>
    /// Временное окно для ПРР
    /// </summary>
    public class Interval
    {
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
