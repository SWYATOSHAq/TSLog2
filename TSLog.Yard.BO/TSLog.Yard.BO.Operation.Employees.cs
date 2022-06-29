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
        /// Работник, назначенный на операцию двора на определённую должность (связь работника и исполняемых обязаностей)
        /// </summary>
        public class OperationWorker
        {
            /// <summary>
            /// Работник
            /// </summary>
            public Employee Worker { get; private set; }
            /// <summary>
            /// Должность
            /// </summary>
            public Position Position { get; private set; }
            public OperationWorker(Employee employee, Position position)
            {
                Worker = employee ?? throw new ArgumentNullException(nameof(employee));
                Position = position ?? throw new ArgumentNullException(nameof(position));
            }
        }
        private readonly IList<OperationWorker> _operationWorkers = new List<OperationWorker>();
        /// <summary>
        /// Назначить работника на операцию двора на указанную должность
        /// </summary>
        /// <param name="worker">Работник</param>
        /// <param name="position">Должность</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddWorker(Employee worker, Position position)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            var operationWorker = new OperationWorker(worker, position);
            AddWorker(operationWorker);
        }
        /// <summary>
        /// Назначить работника на операцию двора на указанную должность
        /// </summary>
        /// <param name="worker">Связь работника и должности</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddWorker(OperationWorker worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            // TODO: check existance
            _operationWorkers.Add(worker);
        }
        /// <summary>
        /// Назначение нескольких работников на операцию двора (ПРР)
        /// </summary>
        /// <param name="workers">Массив работников (+ должности)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddWorkers(OperationWorker[] workers)
        {
            if (workers == null)
                throw new ArgumentNullException(nameof(workers));
            foreach (var operationWorker in workers)
                AddWorker(operationWorker);
        }
        /// <summary>
        /// Снятие работника с операции двора (ПРР)
        /// </summary>
        /// <param name="worker">Работник</param>
        /// <param name="position">Должность</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RemoveWorker(Employee worker, Position position)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            foreach (var operationWorker in _operationWorkers)
                if (operationWorker.Worker == worker && operationWorker.Position == position)
                    _operationWorkers.Remove(operationWorker);
        }
        /// <summary>
        /// Работники, назначенные на данную операцию двора (с указанными должностями)
        /// </summary>
        public OperationWorker[] OperationWorkers
        {
            get
            {
                return _operationWorkers.ToArray();
            }
        }
        /// <summary>
        /// Количество назначенных работников на операцию двора (ПРР)
        /// </summary>
        public short WorkersCount
        {
            get 
            {
                return (short)_operationWorkers.Count;
            }
        }
        /// <summary>
        /// Возвращает работников, назначенных на данную операцию двора (ПРР) "в разрезе" должностей
        /// </summary>
        /// <returns>Хэш-таблица должность - массив назначенных работников</returns>
        public IDictionary<Position, IList<Employee>> GetWorkersByPositions()
        {
            var result = new Dictionary<Position, IList<Employee>>();
            foreach (var operationWorker in _operationWorkers)
            {
                if (!result.ContainsKey(operationWorker.Position))
                    result.Add(operationWorker.Position, new List<Employee>());
                result[operationWorker.Position].Add(operationWorker.Worker);
            }
            return result;
        }
        /// <summary>
        /// Стоимость работ по должностям (в зависимости от назначеых сотрудников)
        /// </summary>
        public IDictionary<Position, double> PositionsCosts()
        {
            var result = new Dictionary<Position, double>();
            var positions = Positions;
            var positionsLaborCosts = PositionsLaborCosts();
            var positionsWorkers = GetWorkersByPositions();
            foreach (var position in positions)
            {
                var positionLaborCost = positionsLaborCosts[position];
                var positionAvgCost = 0.0;
                if (!positionsWorkers.ContainsKey(position))
                    positionAvgCost = position.LaborHourCost;
                else
                {
                    var positionEmployees = positionsWorkers[position];
                    foreach (var employee in positionEmployees)
                        positionAvgCost += employee.Cost;
                    positionAvgCost = positionAvgCost / positionEmployees.Count;
                }
                if (!result.ContainsKey(position))
                    result.Add(position, 0.0);
                result[position] += positionAvgCost * positionLaborCost / 3600;
            }
            return result;
        }
        /// <summary>
        /// Общая стоимость работ (в зависимости от назначеных сотрудников)
        /// </summary>
        public double TotalCost
        {
            get
            {
                var result = 0.0;
                var positions = Positions;
                var positionsLaborCosts = PositionsLaborCosts();
                var positionsWorkers = GetWorkersByPositions();
                foreach (var position in positions)
                {
                    var positionLaborCost = positionsLaborCosts[position];
                    var positionAvgCost = 0.0;
                    if (!positionsWorkers.ContainsKey(position))
                        positionAvgCost = position.LaborHourCost;
                    else
                    {
                        var positionEmployees = positionsWorkers[position];
                        foreach (var employee in positionEmployees)
                            positionAvgCost += employee.Cost;
                        positionAvgCost = positionAvgCost / positionEmployees.Count;
                    }
                    result += positionAvgCost * positionLaborCost / 3600;
                }
                return result;
            }
        }
    }
}
