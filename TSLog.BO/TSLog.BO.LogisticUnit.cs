namespace TSLog.BO
{
    public class LogisticUnit
    {
        public string Type { get; private set; }
        public string Sscc { get; private set; }
        public LogisticUnit(string type, string code)
        {
            Type = type;
            Sscc = code;
        }
    }
}
