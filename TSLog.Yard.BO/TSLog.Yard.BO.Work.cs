using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Элементарная работа по операции двора
    /// </summary>
    public class Work : Entity
    {
        private const short DefaultTime = 3600;
        /// <summary>
        /// Время проведения элементарной работы
        /// </summary>
        public int Time { get; private set; }
        /// <summary>
        /// Признак "переменной" длительности элементарной работы
        /// </summary>
        public bool IsVariable { get; private set; }
        public Work(short id, string code, bool isVariable, string name, string shortName, string altName, string displayName, short time,
            bool isActive = true, short displayOrder = -1,
            string note = "") 
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            Time = time > 0 ? Time : DefaultTime;
            IsVariable = isVariable;
        }
        private Work() : base()
        {
            Time = DefaultTime;
        }

        private static Work? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Work Null
        {
            get
            {
                if (_null == null)
                    _null = new Work(-1, "NULL", false, "NULL", "NULL", "NULL", "NULL", DefaultTime);
                return _null;
            }
        }
        private static Work? _empty = null;
        /// <summary>
        /// Empty object
        /// </summary>
        public static new Work Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Work();
                return _empty;
            }
        }
    }
}