using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Тип операции двора
    /// </summary>
    public class OperationType : Entity
    {
        private const int DefaultTime = 60;
        private const short DefaultWorkersCount = 2;
        public enum TypeDirection { In, Out };
        public TypeDirection Direction { get; private set; }
        public OperationType(long id, TypeDirection direction, 
            string code, string name, string shortName, string altName, string displayName, int time, short workersCount, string note, 
            bool isActive = true) 
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        {
            Direction = direction;
            Time = time > 0 ? time : DefaultTime;
            WorkersCount = workersCount > 0 ? workersCount : DefaultWorkersCount;
        }

        public int Time { get; set; }
        public short WorkersCount { get; set; }
        public string GetFormattedTime()
        {
            return new TimeSpan(0, Time, 0).ToString(@"hh\:mm\:ss");
        }

        public override string ToString()
        {
            return $"{this.DisplayName} ({GetFormattedTime()})";
        }

        private static OperationType _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new OperationType Null
        {
            get
            {
                if (_null == null)
                    _null = new OperationType(-1, TypeDirection.Out, "NULL", "NULL", "NULL", "NULL", "NULL", 60, 2, "NULL");
                return _null;
            }
        }
    }
}
