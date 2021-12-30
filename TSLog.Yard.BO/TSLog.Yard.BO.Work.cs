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
        public Work(short id, string code, string name, string shortName, string altName, string displayName, short time,
            bool isActive = true, short displayOrder = -1,
            string note = "") 
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            Time = time > 0 ? Time : DefaultTime;
        }
    }
}
