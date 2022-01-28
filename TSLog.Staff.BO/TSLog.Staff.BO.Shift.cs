namespace TSLog.Staff.BO
{
    public class Shift
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string WorkMode { get; set; } = string.Empty;
        private IDictionary<int, Employee> _employees = new Dictionary<int, Employee>();
    }
}
