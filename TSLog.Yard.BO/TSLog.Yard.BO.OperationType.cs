using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Тип операции двора
    /// </summary>
    public class OperationType : Entity
    {
        public const short DefaultTime = 60;
        /// <summary>
        /// Приход, расход
        /// </summary>
        public enum TypeDirection 
        { 
            /// <summary>
            /// Приход
            /// </summary>
            In, 
            /// <summary>
            /// Расход
            /// </summary>
            Out 
        };
        /// <summary>
        /// Приход или расход
        /// </summary>
        public TypeDirection Direction { get; private set; } 
        public OperationType(long id, TypeDirection direction, 
            string code, string name, string shortName, string altName, string displayName, int time, string note, 
            bool isActive = true) 
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        {
            Direction = direction;
            Time = time > 0 ? time : DefaultTime;
        }
        private OperationType() : base()
        { 
            Direction = TypeDirection.Out;
            Time = 0;
        }

        public int Time { get; private set; }
        public string GetFormattedTime()
        {
            return new TimeSpan(0, Time, 0).ToString(@"hh\:mm\:ss");
        }

        public override string ToString()
        {
            return $"{this.DisplayName} ({GetFormattedTime()})";
        }

        private static OperationType? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new OperationType Null
        {
            get
            {
                if (_null == null)
                    _null = new OperationType(-1, TypeDirection.Out, "NULL", "NULL", "NULL", "NULL", "NULL", DefaultTime, "NULL");
                return _null;
            }
        }
        private static OperationType? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new OperationType Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new OperationType();
                return _empty;
            }
        }
    }
}
