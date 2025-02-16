namespace TSLog.Staff.BO;

/// <summary>
/// Рабочая смена.
/// </summary>
public class Shift : IComparable<Shift>
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// Дата и время начала.
    /// </summary>
    public DateTime From { get; private set; }
    /// <summary>
    /// Дата и время окончания
    /// </summary>
    public DateTime To { get; private set; }
    /// <summary>
    /// Продолжительность смены в секундах.
    /// </summary>
    public int Length 
    {
        get
        {
            return (int)(To - From).TotalSeconds;
        }
    }
    /// <summary>
    /// Режим работы (номер смены).
    /// </summary>
    public string WorkMode { get; private set; } = string.Empty;
    /// <summary>
    /// Вид смены (день, ночь).
    /// </summary>
    public string Kind { get; set; } = "D";
    /// <summary>
    /// Примечания и комментарии на естественном языке.
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
        ArgumentNullException.ThrowIfNull(employee);
        if (_employees.ContainsKey((int)employee.Id))
            throw new ArgumentException($"Employee {employee.Id} already in shift");
        _employees.Add((int)employee.Id, employee);
    }
    /// <summary>
    /// Возвращает сотрудника смены по идентификатору сотрудника.
    /// </summary>
    /// <param name="id">Идентификатор сотрудника.</param>
    /// <returns>Сотрудник.</returns>
    /// <exception cref="ArgumentException"></exception>
    public Employee GetEmployee(int id)
    {
        if (!_employees.ContainsKey(id))
            throw new ArgumentException($"Employee with id {id} not found");
        return _employees[id];
    }
    /// <summary>
    /// Проверяет наличие конкретного сотрудника в данной смене.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Признак наличия.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool ContainsEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);
        return _employees.ContainsKey(employee.Id);
    }
    /// <summary>
    /// Все сотрудники смены.
    /// </summary>
    public Employee[] Employees
    {
        get
        {
            return _employees.Values.ToArray();
        }
    }
    /// <summary>
    /// Удаление сотрудника из смены.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void RemoveEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);
        if (!_employees.ContainsKey((int)employee.Id))
            throw new ArgumentException($"Employee with id {employee.Id} not found");
        _employees.Remove((int)employee.Id);
    }
    /// <summary>
    /// Возвращает массив "промежутков" - смен в указанном временном интервале.
    /// </summary>
    /// <param name="fromDate">Начало интервала.</param>
    /// <param name="toDate">Окончание интервала.</param>
    /// <returns>Массив "промежутков" - смен.</returns>
    public static Shift[] GetShifts(DateTime fromDate, DateTime toDate)
    {
        if (fromDate >= toDate)
            throw new ArgumentException($"Wrong dates - from: {fromDate}, to: {toDate}");

        //var shifts = Shift.GetShifts(DateTime.Parse("01.06.2022 9:00:00"), DateTime.Parse("01.06.2022 21:00:00"));
        //var shifts = Shift.GetShifts(DateTime.Parse("01.06.2022 9:00:00"), DateTime.Parse("02.06.2022 9:00:00"));
        //var shifts = Shift.GetShifts(DateTime.Parse("01.06.2022 0:00:00"), DateTime.Parse("02.06.2022 0:00:00"));
        //var shifts = Shift.GetShifts(DateTime.Parse("01.06.2022 13:00:00"), DateTime.Parse("01.06.2022 14:00:00"));

        var shifts = new List<Shift>();
        var shiftStart = fromDate;
        DateTime shiftFinish;
        if (fromDate < fromDate.Date.AddHours(9))
            shiftFinish = fromDate.Date.AddHours(9); // 9:00.
        else if (fromDate < fromDate.Date.AddHours(21))
            shiftFinish = fromDate.Date.AddHours(21); // 21:00.
        else
            shiftFinish = fromDate.Date.AddHours(33); // 9:00 след. суток

        do
        {                
            var shift = new Shift(-1,
                shiftStart,
                shiftFinish <= toDate ? shiftFinish : toDate,
                string.Empty);

            if (shiftStart >= shiftStart.Date.AddHours(9) && shiftStart < shiftStart.Date.AddHours(21))
                shift.Kind = "D";
            else
                shift.Kind = "N";

            shifts.Add(shift);
            shiftStart = shiftFinish;
            shiftFinish = shiftStart.AddHours(12);
        } while (shiftStart < toDate);

        return shifts.ToArray();
    }
    /// <summary>
    /// Возвращает скорректированную плановую длину смены на случай сокращенной смены. Сокращенная смена формируется
    /// при привязке и последующей отвязке на участке 1053 "Сокращенная смена".
    /// </summary>
    /// <param name="startTime">Начало работы по БК.</param>
    /// <param name="finishTime">Окончание работы по БК.</param>
    /// <param name="siteId">Участок (укороченная смена на участке 1053).</param>
    /// <param name="employeeShiftLength">Стандартная длина рабочей смены сотрудника.</param>
    /// <param name="shiftKind">Дневная или ночная типы смен.</param>
    /// <returns></returns>
    public static int GetShortShiftLength(DateTime startTime, DateTime finishTime, int siteId, int employeeShiftLength, string shiftKind) {
        int shortShiftLengthSeconds;
        if (siteId == 1053 && employeeShiftLength == 27612) // Дневной сотрудник.
            shortShiftLengthSeconds = (int)Math.Round((finishTime - finishTime.Date.AddHours(9)).TotalSeconds); // 18:00.
        else if (siteId == 1053 && employeeShiftLength == 13800) // Костыль под Петрова Никиту.
            shortShiftLengthSeconds = (int)Math.Round((finishTime - finishTime.Date.AddHours(13)).TotalSeconds); 
        else if (siteId == 1053 && employeeShiftLength == 21000) // Костыль под Охотникова Святослава.
            shortShiftLengthSeconds = (int)Math.Round((finishTime - finishTime.Date.AddHours(12)).TotalSeconds); 
        else if (siteId == 1053 && employeeShiftLength == 38412) // Сменный сотрудник.
        {
            if (shiftKind == "D") // Для дневной смены.
                shortShiftLengthSeconds = (int)Math.Round((finishTime - finishTime.Date.AddHours(9)).TotalSeconds); // 21:00.
            else // Для ночной смены.
            {
                if (startTime >= startTime.Date.AddHours(21)) // В случае окончания работы до 23:59.
                    shortShiftLengthSeconds = (int)Math.Round((finishTime - finishTime.Date.AddHours(21)).TotalSeconds); // 9:00 след. суток.
                else // В случае окончания работы после 00:00.
                    shortShiftLengthSeconds = (int)Math.Round((finishTime - finishTime.Date.AddHours(-3)).TotalSeconds); // 9:00.
            }
        }
        else
            shortShiftLengthSeconds = employeeShiftLength;
        return shortShiftLengthSeconds;
    }
    
    public static string GetKey(DateTime date)
    {
        var day = date.Date;
        if (date < day.AddHours(9))
            return $"{day.AddDays(-1):yyyyMMdd}N";
        else if (date < day.AddHours(21))
            return $"{day:yyyyMMdd}D";
        else
            return $"{day:yyyyMMdd}N";
    }
    public int CompareTo(Shift? other)
    {
        if (other == null)
            return 1;
        var key = Kind != "TOTAL" ? GetKey(From) : Kind;
        var otherKey = other.Kind != "TOTAL" ? GetKey(other.From) : other.Kind;
        return key.CompareTo(otherKey);
    }
}
