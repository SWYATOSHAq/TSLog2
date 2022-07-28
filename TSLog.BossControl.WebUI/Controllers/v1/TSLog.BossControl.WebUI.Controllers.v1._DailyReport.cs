using System.Dynamic;
using TSLog.BossControl.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.WebUI.Controllers.v1
{
    /// <summary>
    /// Не работает, переписываем!
    /// </summary>
    public class _DailyReport
    {
        private double K = 0.1;
        public class Cell
        {
            public string RowKey { get; private set; }
            public string ColumnKey { get; private set; }
            public short WorkersCount { get; set; } = 0;
            public double PlanTime { get; set; } = 0.0;
            public double FactTime { get; set; } = 0.0;
            public double PlanCost { get; set; } = 0.0;
            public double FactCost { get; set; } = 0.0;
            public readonly IDictionary<Employee, double> WorkersTimes = new Dictionary<Employee, double>();

            public Cell(string rowKey, string columnKey)
            {
                RowKey = rowKey;
                ColumnKey = columnKey;
            }

            public void AddTime(Employee worker, double time)
            {
                if (WorkersTimes.ContainsKey(worker))
                    WorkersTimes[worker] += time;
                else
                    WorkersTimes.Add(worker, time);
            }
        }

        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }
        private readonly IBossControlStorage _storage;

        private readonly IDictionary<string, IDictionary<string, Cell>> _rows = new Dictionary<string, IDictionary<string, Cell>>();
        private void AddData(string shiftCode, SiteWorker work)
        {
            string rowKey;
            if (work.Site.Kind != "AUXILIARY")
                rowKey = work.Worker.MajorPosition.Code;
            else
                rowKey = "AUXILIARY";

            if (!_rows.ContainsKey(rowKey))
                _rows.Add(rowKey, new Dictionary<string, Cell>());
            var row = _rows[rowKey];
            if (!row.ContainsKey(shiftCode))
                row.Add(shiftCode, new Cell(rowKey, shiftCode));
            row[shiftCode].AddTime(work.Worker, work.Duration);
        }


        private void AddRowTotal(Cell cell)
        {
            var row = _rows[cell.RowKey];
            if (!row.ContainsKey("TOTAL"))
                row.Add("TOTAL", new Cell(cell.RowKey, "TOTAL"));
            var totalCell = row["TOTAL"];
            totalCell.FactTime += cell.FactTime;
            if (cell.RowKey != "AUXILIARY")
            {
                totalCell.PlanTime += cell.PlanTime;
                totalCell.PlanCost += cell.PlanCost;
                totalCell.FactCost += cell.FactCost;
            }
        }
        private void AddColumnTotal(Cell cell)
        {
            if (cell.RowKey == "AUXILIARY")
                return;
            if (!_rows.ContainsKey("TOTAL"))
                _rows.Add("TOTAL", new Dictionary<string, Cell>());
            var totalRow = _rows["TOTAL"];

            if (!totalRow.ContainsKey(cell.ColumnKey))
                totalRow.Add(cell.ColumnKey, new Cell("TOTAL", cell.ColumnKey));
            var totalCell = totalRow[cell.ColumnKey];
            totalCell.FactTime += cell.FactTime;
            totalCell.PlanTime += cell.PlanTime;
            totalCell.PlanCost += cell.PlanCost;
            totalCell.FactCost += cell.FactCost;

            AddRowTotal(totalCell);
        }

        private void Calculate()
        {
            var rowKeys = _rows.Keys;
            foreach (var rowKey in rowKeys)
            {
                var row = _rows[rowKey];
                var columnKeys = row.Keys;
                foreach (var columnKey in columnKeys)
                {
                    var cell = row[columnKey];
                    var workers = cell.WorkersTimes.Keys;
                    foreach (var worker in workers)
                    {
                        if (cell.WorkersTimes[worker] < K * worker.ShiftLength)
                        {
                            cell.WorkersTimes.Remove(worker);
                            continue;
                        }                        
                        cell.FactTime += cell.WorkersTimes[worker];
                        if (rowKey != "AUXILIARY")
                        {
                            cell.PlanTime += worker.ShiftLength;
                            cell.PlanCost += worker.ShiftLength / 60 / 60 * worker.MajorPosition.LaborHourCost;
                            cell.FactCost += cell.WorkersTimes[worker] / 60 / 60 * worker.MajorPosition.LaborHourCost;
                        }
                    }
                    AddRowTotal(cell);
                    AddColumnTotal(cell);
                }
            }
        }

        public _DailyReport(DateTime fromDate, DateTime toDate, IBossControlStorage storage)
        {
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");
            FromDate = fromDate;
            ToDate = toDate;
            _storage = storage;
        }        

        public void Create()
        {
            // Получаем все смены, которые попадают в указанный промежуток
            var shifts = Shift.GetShifts(FromDate, ToDate);

            // Проходим по всем сменам, попадающим в указанный промежуток            )
            _rows.Clear();
            foreach (var shift in shifts)
            {
                var shiftCode = Shift.GetKey(shift.From);
                var works = _storage.GetWorks(shift.From, shift.To);
                foreach (var work in works)
                    AddData(shiftCode, work);
            }

            // Вычисляем значение по должностям по каждой смене
            Calculate();
        }

        public dynamic[] ToDynamic()
        {
            dynamic elements = new List<dynamic>();

            foreach (var rowKey in _rows.Keys)
            {
                var row = _rows[rowKey];
                dynamic element = new ExpandoObject();
                element.mark = rowKey;

                var shifts = new Dictionary<string, dynamic>();
                foreach (var columnKey in row.Keys)
                {
                    var cell = row[columnKey];
                    dynamic shift = new ExpandoObject();
                    shift.planTime = cell.PlanTime;
                    shift.factTime = cell.FactTime;
                    shift.planCost = cell.PlanCost;
                    shift.factCost = cell.FactCost;
                    shifts.Add(columnKey, shift);
                }

                element.shifts = shifts;
                elements.Add(element);
            }

            return elements.ToArray();
        }
    }
}