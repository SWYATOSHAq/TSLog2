using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Тип операции двора
    /// </summary>
    public class OperationType : Entity
    {
        public const short DefaultTime = 60;
        public const int DefaultLaborCost = 60;
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
        /// <summary>
        /// Приход или расход (текстом)
        /// </summary>
        public string DirectionName
        {
            get
            {
                return Direction == TypeDirection.Out ? "Погрузка" : "Разгрузка";
            }
        }
        /// <summary>
        /// Признак паллетной обработки
        /// </summary>
        public bool IsPallet { get; private set; }
        public OperationType(long id, TypeDirection direction, bool isPallet,
            string code, string name, string shortName, string altName, string displayName, int time,
            bool isActive = true, short displayOrder = -1,
            string note = "")
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            Direction = direction;
            IsPallet = isPallet;
            Time = time > 0 ? time : DefaultTime;
        }
        private OperationType() : base()
        {
            Direction = TypeDirection.Out;
            Time = 0;
        }
        /// <summary>
        /// Продолжительность операции двора данного типа по умолчанию (в секундах)
        /// </summary>
        public int Time { get; private set; }
        public string GetFormattedTime()
        {
            return new TimeSpan(0, Time, 0).ToString(@"hh\:mm\:ss");
        }

        public override string ToString()
        {
            return $"{DisplayName}";
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
                    _null = new OperationType(-1, TypeDirection.Out, true, "NULL", "NULL", "NULL", "NULL", "NULL", DefaultTime, true, -1, "NULL");
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
