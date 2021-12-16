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
    }
}
