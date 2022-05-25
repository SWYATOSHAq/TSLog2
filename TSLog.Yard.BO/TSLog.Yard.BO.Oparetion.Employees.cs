using TSLog.Common.BO;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Операция двора (разгрузка или погрузка автомашины)
    /// </summary>
    public partial class Operation : Entity
    {
        public class OperationEmployee
        {
            public Employee Employee { get; private set; }
            public Position Position { get; private set; }
            public OperationEmployee(Employee employee, Position position)
            {
                Employee = employee ?? throw new ArgumentNullException(nameof(employee));
                Position = position ?? throw new ArgumentNullException(nameof(position));
            }
        }
        private readonly IList<OperationEmployee> _operationWorkers = new List<OperationEmployee>();
        public void AddOperationEmployee(Employee employee, Position position)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            var operationWorker = new OperationEmployee(employee, position);
            AddOperationEmployee(operationWorker);
        }
        public void AddOperationEmployee(OperationEmployee operationEmployee)
        {
            if (operationEmployee == null)
                throw new ArgumentNullException(nameof(operationEmployee));

            // TODO: check existance
            _operationWorkers.Add(operationEmployee);
        }
        public void AddOperationEmployees(OperationEmployee[] operationEmployees)
        {
            if (operationEmployees == null)
                throw new ArgumentNullException(nameof(operationEmployees));
            foreach (var operationWorker in operationEmployees)
                AddOperationEmployee(operationWorker);
        }
        public void RemoveOperationEmployee(Employee emploee, Position position)
        {
            if (emploee == null)
                throw new ArgumentNullException(nameof(emploee));
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            foreach (var operationWorker in _operationWorkers)
                if (operationWorker.Employee == emploee && operationWorker.Position == position)
                    _operationWorkers.Remove(operationWorker);
        }
        public OperationEmployee[] OperationEmployees
        {
            get
            {
                return _operationWorkers.ToArray();
            }
        }
        public short OperationEmployeesCount
        {
            get 
            {
                return (short)_operationWorkers.Count;
            }
        }
        public IDictionary<Position, IList<Employee>> GetEmployeesByPositions()
        {
            var result = new Dictionary<Position, IList<Employee>>();
            foreach (var operationWorker in _operationWorkers)
            {
                if (!result.ContainsKey(operationWorker.Position))
                    result.Add(operationWorker.Position, new List<Employee>());
                result[operationWorker.Position].Add(operationWorker.Employee);
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
            var positionsWorkers = GetEmployeesByPositions();
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
                var positionsWorkers = GetEmployeesByPositions();
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
