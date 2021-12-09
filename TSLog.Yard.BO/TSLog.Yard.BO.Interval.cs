namespace TSLog.Yard.BO
{
    /// <summary>
    /// Временное окно для ПРР
    /// </summary>
    public class Interval
    {
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        public Gate Gate { get; private set; }

        public Interval(DateTime from, DateTime to, Gate gate) 
        {
            From = from;
            To = to;
            Gate = gate;
        }
    }
}
