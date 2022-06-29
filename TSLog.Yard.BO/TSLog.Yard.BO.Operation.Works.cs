using TSLog.Common.BO;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Операция двора (разгрузка или погрузка автомашины)
    /// </summary>
    public partial class Operation : Entity
    {
        /// <summary>
        /// Элементарная работа операции двора (ПРР)
        /// </summary>
        public class OperationWork
        {
            private const short DefaultTime = 600;
            private const short DefaultWorkersCount = 1;
            public Work Work { get; set; }
            public Position Position { get; set; }
            public short Time { get; set; }
            public short WorkersCount { get; set; }
            public OperationWork(Work work, Position position, short time, short workersCount)
            {
                Work = work;
                Position = position;
                Time = time > 0 ? time : DefaultTime;
                WorkersCount = workersCount > 0 ? workersCount : DefaultWorkersCount;
            }
        }
        private readonly IList<OperationWork> _operationWorks = new List<OperationWork>();
        /// <summary>
        /// Добавление работы в операцию двора (ПРР)
        /// </summary>
        /// <param name="work">Работа</param>        
        /// <exception cref="ArgumentNullException"></exception>
        public void AddOperationWork(OperationWork operationWork)
        {
            if (operationWork == null)
                throw new ArgumentNullException(nameof(operationWork));
            _operationWorks.Add(operationWork);
        }
        /// <summary>
        /// Добавление нескольких работ в операцию двора ПРР
        /// </summary>
        /// <param name="operationWorks"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddOperationWorks(OperationWork[] operationWorks)
        {
            if (operationWorks == null)
                throw new ArgumentNullException(nameof(operationWorks));
            foreach (OperationWork operationWork in operationWorks)
                AddOperationWork(operationWork);
        }
        /// <summary>
        /// Должности, задействованные на данной операции двора
        /// </summary>
        public Position[] Positions
        {
            get
            {
                var positions = new HashSet<Position>();
                foreach (var operationWork in _operationWorks)
                    if (!positions.Contains(operationWork.Position))
                        positions.Add(operationWork.Position);
                return positions.ToArray();
            }
        }
        /// <summary>
        /// Элементарные работы по данной операции двора (ПРР)
        /// </summary>
        public Work[] GetWorks()
        {
            var works = new HashSet<Work>();
            foreach (var operationWork in _operationWorks)
                if (!works.Contains(operationWork.Work))
                    works.Add(operationWork.Work);
            return works.ToArray();
        }
        /// <summary>
        /// Работы операции двора (ПРР)
        /// </summary>
        public OperationWork[] OperationWorks
        {
            get
            {
                return _operationWorks.ToArray();
            }
        }
        private const short MinWorkersCount = 1;
        private const short MaxWorkersCount = 4;
        /// <summary>
        /// Пересчёт планового времени операции двора (ПРР) в зависимости от количества выделенных сотрудников
        /// </summary>
        /// <param name="staff">ВЫделенные сотрудники по должностям</param>
        /// <returns></returns>        
        public short CalculatePlanTime(IDictionary<Position, short>? staff = null)
        {
            var result = 0.0;
            foreach (var operationWork in _operationWorks)
            {
                var workersCount = (staff == null || !staff.ContainsKey(operationWork.Position))
                    ? operationWork.WorkersCount
                    : staff[operationWork.Position];
                workersCount = workersCount <= 0 ? MinWorkersCount : workersCount;
                workersCount = workersCount <= 4 ? workersCount : MaxWorkersCount;

                var coefficient = operationWork.WorkersCount / workersCount;
                result += operationWork.Time * coefficient;
            }
            return (short)result;
        }
    }
}