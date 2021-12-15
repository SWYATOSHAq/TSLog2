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
        public Gate Gate { get; private set; }

        public Interval(DateTime from, DateTime to, Gate gate) 
        {
            From = from;
            To = to;
            Gate = gate;
        }
    }
}
