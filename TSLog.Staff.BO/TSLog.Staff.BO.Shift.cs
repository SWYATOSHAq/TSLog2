namespace TSLog.Staff.BO
{
    /// <summary>
    /// Рабочая смена
    /// </summary>
    public class Shift
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Дата и время начала
        /// </summary>
        public DateTime From { get; private set; }
        /// <summary>
        /// Дата и время окончания
        /// </summary>
        public DateTime To { get; private set; }
        /// <summary>
        /// Режим работы (номер смены)
        /// </summary>
        public string WorkMode { get; private set; } = string.Empty;     
        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public string Note { get; set; }
        public Shift(long id, DateTime from, DateTime to, string workMode, string note = "")
        {
            Id = id;
            From = from;
            To = to;
            WorkMode = workMode;
            Note = string.IsNullOrWhiteSpace(note) ? string.Empty : note.Trim();
        }
        private readonly IDictionary<int, Employee> _employees = new Dictionary<int, Employee>();
        public void AddEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (_employees.ContainsKey((int)employee.Id))
                throw new ArgumentException($"Employee {employee.Id} already in shift");
            _employees.Add((int)employee.Id, employee);
        }
        /// <summary>
        /// Возвращает сотрудника смены по идентификатору сотрудника
        /// </summary>
        /// <param name="id">Идентификатор сотрудника</param>
        /// <returns>Сотрудник</returns>
        /// <exception cref="ArgumentException"></exception>
        public Employee GetEmployee(int id)
        {
            if (!_employees.ContainsKey(id))
                throw new ArgumentException($"Employee with id {id} not found");
            return _employees[id];
        }
        /// <summary>
        /// Проверяет наличие конкретного сотрудника в данной смене
        /// </summary>
        /// <param name="employee">Сотрудник</param>
        /// <returns>Признак наличия</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ContainsEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            return _employees.ContainsKey(employee.Id);
        }
        /// <summary>
        /// Все сотрудники смены
        /// </summary>
        public Employee[] Employees
        {
            get
            {
                return _employees.Values.ToArray();
            }
        }
        /// <summary>
        /// Удаление сотрудника из смены
        /// </summary>
        /// <param name="employee">Сотрудник</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof (employee));
            if (!_employees.ContainsKey((int)employee.Id))
                throw new ArgumentException($"Employee with id {employee.Id} not found");
            _employees.Remove((int)employee.Id);
        }
    }
}
