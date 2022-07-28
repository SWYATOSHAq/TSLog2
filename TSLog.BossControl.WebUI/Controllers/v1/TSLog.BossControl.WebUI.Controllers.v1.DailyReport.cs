using System.Dynamic;
using TSLog.BossControl.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.WebUI.Controllers.v1
{
    /// <summary>
    /// Отчёт по дням по отработанному времени сотрудников из БК
    /// </summary>
    public class DailyReport
    {
        public class Mark: IComparable<Mark>
        {
            public string Key { get; private set; }
            public string Name { get; private set; }
            public string DataType { get; private set; }
            public Mark(string key, string name, string dataType)
            {
                Key = key;
                Name = name;
                DataType = dataType;
            }
            public int CompareTo(Mark? other)
            {
                return Key.CompareTo(other.Key);
            }
        }

        public class ShiftData
        {
            public string Key { get; private set; }
            public IDictionary<Employee, double> WorkersTime = new Dictionary<Employee, double>();

            // Показатели
            public IDictionary<string, double> MarkValues = new Dictionary<string, double>();
            public void AddMarkValue(string markKey, double value)
            {
                if (MarkValues.ContainsKey(markKey))
                    MarkValues[markKey] += value;
                else
                    MarkValues.Add(markKey, value);
            }

            public double AuxiliaryTime { get; set; } = 0.0;
            public ShiftData(string key) 
            { 
                Key = key;
            }
        }

        private const double K = 0.1;
        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }        

        private readonly IBossControlStorage _storage;
        public DailyReport(DateTime fromDate, DateTime toDate, IBossControlStorage storage)
        {
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");
            FromDate = fromDate;
            ToDate = toDate;
            _storage = storage;
        }
        private HashSet<Shift> _shifts = new HashSet<Shift>();
        private readonly IDictionary<string, Mark> _marks = new Dictionary<string, Mark>();
        public readonly IDictionary<string, ShiftData> _shiftsData = new Dictionary<string, ShiftData>();

        private void ClearData()
        {
            _shifts.Clear();
            _shiftsData.Clear();
            _marks.Clear();
        }
        private void GetShifts()
        {
            // Получаем все смены, которые попадают в указанный промежуток
            var shifts = Shift.GetShifts(FromDate, ToDate);
            _shifts = new HashSet<Shift>(shifts);
        }
        private void GetData()
        {
            // Проходим по всем сменам, попадающим в указанный промежуток,
            // формируем данные по работающим сотрудникам за смену - 
            // Для каждого работника в смене отработанное им время плюс время всех работников на вспомогательных участках
            foreach (var shift in _shifts)
            {
                var works = _storage.GetWorks(shift.From, shift.To);

                var shiftKey = Shift.GetKey(shift.From);
                var shiftData = new ShiftData(shiftKey);
                foreach (var work in works)
                {
                    if (work.Site.Kind != "AUXILIARY")
                    {
                        if (shiftData.WorkersTime.ContainsKey(work.Worker))
                            shiftData.WorkersTime[work.Worker] += work.Duration;
                        else
                            shiftData.WorkersTime.Add(work.Worker, work.Duration);
                    }
                    else
                        shiftData.AuxiliaryTime += work.Duration;
                }
                _shiftsData.Add(shiftKey, shiftData);
            }
        }
        private void PrepareData()
        {
            // Обработка полученных данных:
            // Не учитываем время тех работников, которые отработали менее 10% времени смены
            foreach (var shiftData in _shiftsData.Values)
            {                
                foreach (var pair in shiftData.WorkersTime)
                {
                    if (pair.Value < pair.Key.ShiftLength * K)
                        shiftData.WorkersTime.Remove(pair.Key);
                }
            }
        }
        private void CalculateMarks()
        {
            // Вычисление показателей
            // Идём по всем сменам, считаем и при случае добавляем показатели
            foreach (var shiftData in _shiftsData.Values)
            {
                void AddMark(string markKey, string markName, string markDataType, ShiftData shiftData, double value)
                {
                    if (!_marks.ContainsKey(markKey))
                        _marks.Add(markKey, new Mark(markKey, markName, markDataType));
                    shiftData.AddMarkValue(markKey, value);
                }

                var markKey = "AUXILIARY_TIME";
                var markName = "Время на МОП";
                AddMark(markKey, markName, "DOUBLE", shiftData, shiftData.AuxiliaryTime);
                foreach (var workerTime in shiftData.WorkersTime)
                {
                    markKey = workerTime.Key.MajorPosition.Id + "_1WORKERS_COUNT";
                    markName = workerTime.Key.MajorPosition.ShortName + " - людей";
                    AddMark(markKey, markName, "INTEGER", shiftData, 1.0);
                    AddMark("TOTAL_1WORKERS_COUNT", "Всего людей", "INTEGER", shiftData, 1.0);

                    markKey = workerTime.Key.MajorPosition.Id + "_2PLAN_TIME";
                    markName = workerTime.Key.MajorPosition.ShortName + " - вр. (пл.)";
                    AddMark(markKey, markName, "DOUBLE", shiftData, workerTime.Key.ShiftLength);
                    AddMark("TOTAL_2PLAN_TIME", "Всего вр. (пл.)", "DOUBLE", shiftData, workerTime.Key.ShiftLength);

                    markKey = workerTime.Key.MajorPosition.Id + "_3FACT_TIME";
                    markName = workerTime.Key.MajorPosition.ShortName + " - вр. (ф.)";
                    AddMark(markKey, markName, "DOUBLE", shiftData, workerTime.Value);
                    AddMark("TOTAL_3FACT_TIME", "Всего вр. (ф.)", "DOUBLE", shiftData, workerTime.Value);

                    markKey = workerTime.Key.MajorPosition.Id + "_4PLAN_COST";
                    markName = workerTime.Key.MajorPosition.ShortName + " - затр. (ф.)";
                    AddMark(markKey, markName, "DOUBLE", shiftData, workerTime.Key.ShiftLength / 60 / 60 * workerTime.Key.MajorPosition.LaborHourCost);
                    AddMark("TOTAL_4PLAN_COST", "Всего затр. (пл.)", "DOUBLE", shiftData, workerTime.Key.ShiftLength / 60 / 60 * workerTime.Key.MajorPosition.LaborHourCost);

                    markKey = workerTime.Key.MajorPosition.Id + "_5FACT_COST";
                    markName = workerTime.Key.MajorPosition.ShortName + " - затр. (пл.)";
                    AddMark(markKey, markName, "DOUBLE", shiftData, workerTime.Value / 60 / 60 * workerTime.Key.MajorPosition.LaborHourCost);
                    AddMark("TOTAL_5FACT_COST", "Всего затр. (пл.)", "DOUBLE", shiftData, workerTime.Value / 60 / 60 * workerTime.Key.MajorPosition.LaborHourCost);
                }
            }
        }
        private void CalculateTotalShift()
        {
            var totalShift = new Shift(9999, FromDate, ToDate, "TOTAL");
            totalShift.Kind = "TOTAL";
            _shifts.Add(totalShift);

            var totalShiftData = new ShiftData("TOTAL");
            foreach (var shiftData in _shiftsData.Values)
            {
                foreach (var markKey in shiftData.MarkValues.Keys)
                    totalShiftData.AddMarkValue(markKey, shiftData.MarkValues[markKey]);
            }
            _shiftsData.Add("TOTAL", totalShiftData);
        }
        public void Create()
        {
            ClearData();
            GetShifts();
            GetData();
            PrepareData();
            CalculateMarks();
            CalculateTotalShift();
        }
        private static dynamic MarkToDynamic(Mark mark)
        {
            dynamic result = new ExpandoObject();
            result.key = mark.Key;
            result.name = mark.Name;
            result.dataType = mark.DataType;
            return result;
        }
        private dynamic[] MarksToDynamic(Mark[] marks)
        {
            var result = new List<dynamic>();
            foreach (var mark in marks)
            {
                var dynamicMark = DailyReport.MarkToDynamic(mark);
                result.Add(dynamicMark);
            }
            return result.ToArray();
        }
        private static dynamic ShiftToDynamic(Shift shift)
        {
            dynamic result = new ExpandoObject();
            result.key = shift.Kind != "TOTAL" ? Shift.GetKey(shift.From) : shift.Kind;
            result.startDate = shift.From;
            result.finishDate = shift.To;
            result.kind = shift.Kind;
            var textKind = string.Empty;
            if (shift.Kind == "D")
                textKind = "день";
            else if (shift.Kind == "N")
                textKind = "ночь";
            result.caption = shift.Kind != "TOTAL" ? $"{shift.From.ToString("dd.MM")} {textKind}" : "Всего";
            return result;
        }
        private dynamic[] ShiftsToDynamic(Shift[] shifts)
        {
            var result = new List<dynamic>();
            foreach (var shift in shifts)
            {
                var dynamicShift = DailyReport.ShiftToDynamic(shift);
                result.Add(dynamicShift);
            }
            return result.ToArray();
        }
        public dynamic ToDynamic()
        {
            // Готовим окончательную структуру данных отчёта
            dynamic data = new ExpandoObject();
            var elements = new List<dynamic>();
            var marks = _marks.Values.ToArray();
            Array.Sort(marks);
            data.marks = MarksToDynamic(marks);
            var shifts = _shifts.ToArray();
            Array.Sort(shifts);
            data.shifts = ShiftsToDynamic(shifts);
            foreach (var mark in marks)
            {
                dynamic element = new ExpandoObject();
                element.markKey = mark.Key;
                var values = new List<dynamic>();
                
                var total = 0.0;
                dynamic value;
                foreach (var shift in shifts)
                {
                    var shiftKey = shift.Kind != "TOTAL" ? Shift.GetKey(shift.From) : shift.Kind;
                    value = new ExpandoObject();
                    value.shiftKey = shiftKey;
                    if (!_shiftsData.ContainsKey(shiftKey))
                        value.value = 0.0;
                    else 
                    {
                        var shiftData = _shiftsData[shiftKey];
                        if (shiftData.MarkValues.ContainsKey(mark.Key))
                        {
                            value.value = shiftData.MarkValues[mark.Key];
                            total += shiftData.MarkValues[mark.Key];
                        }                            
                        else
                            value.value = 0.0;
                    }                    
                    values.Add(value);
                }
                element.values = values.ToArray();
                elements.Add(element);
            }
            data.elements = elements.ToArray();
            return data;
        }
    }
}