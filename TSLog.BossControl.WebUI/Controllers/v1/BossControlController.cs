using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using TSLog.BossControl.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.WebUI.Controllers.v1
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class BossControlController : ControllerBase
    {
        private readonly IBossControlStorage _storage;
        public BossControlController(IBossControlStorage storage)
        {
            _storage = storage;
        }

        private static async Task<dynamic?> GetInputData(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(body);
            string json = await reader.ReadToEndAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }

        private static dynamic WorkerToDynamic(Employee worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            dynamic result = new ExpandoObject();
            result.id = worker.Id;
            result.surname = worker.Surname;
            result.firstName = worker.FirstName;
            result.patronymic = worker.Patronymic;
            result.fullName = worker.FullName;
            result.majorPositionCode = worker.MajorPosition != null && worker.MajorPosition != Position.Null && worker.MajorPosition != Position.Empty
                ? worker.MajorPosition.Code
                : string.Empty;
            result.majorPositionName = worker.MajorPosition != null && worker.MajorPosition != Position.Null && worker.MajorPosition != Position.Empty
                ? worker.MajorPosition.Name
                : string.Empty;
            result.companyId = worker.Company.Id;
            result.companyName = worker.Company.Name;
            result.wmsLogin = worker.WmsLogin;
            result.adLogin = worker.AdLogin;
            return result;
        }
        private static dynamic SiteToDynamic(Site site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));
            dynamic result = new ExpandoObject();
            result.id = site.Id;
            result.code = site.Code;
            result.kind = site.Kind;
            result.name = site.Name;
            return result;
        }
        private static dynamic PartnerToDynamic(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            dynamic result = new ExpandoObject();
            result.id = partner.Id;
            result.name = partner.Name;
            return result;
        }
        private static dynamic SiteWorkerToDynamic(SiteWorker siteWorker)
        {
            if (siteWorker == null)
                throw new ArgumentNullException(nameof(siteWorker));
            dynamic result = new ExpandoObject();
            //result.site = BossControlController.SiteToDynamic(siteWorker.Site);
            result.siteId = siteWorker.Site.Id;
            result.siteCode = siteWorker.Site.Code;
            result.siteName = siteWorker.Site.Name;
            //result.partner = BossControlController.PartnerToDynamic(siteWorker.Partner);
            result.partnerId = siteWorker.Partner.Id;
            result.partnerName = siteWorker.Partner.Name;
            //result.worker = BossControlController.WorkerToDynamic(siteWorker.Worker);
            result.workerId = siteWorker.Worker.Id;
            result.workerFullName = siteWorker.Worker.FullName;

            result.workerMajorPositionId = -1;
            result.workerMajorPositionCode = "";
            result.workerMajorPositionName = string.Empty;
            if (siteWorker.Worker.MajorPosition != null
                && siteWorker.Worker.MajorPosition != Position.Null
                && siteWorker.Worker.MajorPosition != Position.Empty)
            {
                result.workerMajorPositionId = siteWorker.Worker.MajorPosition.Id;
                result.workerMajorPositionCode = siteWorker.Worker.MajorPosition.Code;
                result.workerMajorPositionName = siteWorker.Worker.MajorPosition.Name;
            }

            result.workerCompanyId = siteWorker.Worker.Company.Id;
            result.workerCompanyName = siteWorker.Worker.Company.Name;

            result.workerWmsLogin = siteWorker.Worker.WmsLogin;
            result.workerAdLogin = siteWorker.Worker.AdLogin;

            result.startDate = siteWorker.StartDate.ToString("HH:mm:ss (yyyy.MM.dd)");
            result.startDay = siteWorker.StartDate.ToString("dd.MM.yyyy");
            result.startTime = siteWorker.StartDate.ToString("HH:mm:ss");

            return result;
        }
        private static dynamic[] SiteWorkersToDynamic(SiteWorker[] siteWorkers)
        {
            var result = new List<dynamic>();
            foreach (var siteWorker in siteWorkers)
                if (siteWorker != null)
                    result.Add(BossControlController.SiteWorkerToDynamic(siteWorker));
            return result.ToArray();
        }
        private static dynamic[] WorkersToDynamic(Employee[] workers)
        {
            var result = new List<dynamic>();
            foreach (var worker in workers)
                if (worker != null)
                    result.Add(BossControlController.WorkerToDynamic(worker));
            return result.ToArray();
        }

        [HttpPatch("EnterSite")]
        async public Task<string> EmployeeEnterSite() 
        {
            try
            {
                Response.StatusCode = 500;
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                Response.StatusCode = 200;
                return "";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpPatch("LeaveSite")]
        async public Task<string> EmployeeLeaveSite()
        {
            try
            {
                Response.StatusCode = 500;
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                string temp = data.workerId;
                if (string.IsNullOrWhiteSpace(temp))
                    return "Worker is is empty";
                if (!int.TryParse(temp, out int workerId))
                    return $"Wrong worker id ({temp})";

                var worker = _storage.GetEmployee(workerId);
                _storage.LeaveSite(worker);
                var result = WorkerToDynamic(worker);
                var text = JsonSerializer.Serialize(result);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        [HttpGet("BusyWorkers")]
        public string GetBusyWorkers()
        {
            try
            {
                Response.StatusCode = 200;
                var siteWorkers = _storage.GetBusyWorkers();
                var objects = SiteWorkersToDynamic(siteWorkers);
                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("FreeWorkers")]
        public string GetFreeWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var workers = _storage.GetFreeWorkers();
                var objects = WorkersToDynamic(workers);
                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("FreeShiftWorkers")]
        public string GetFreeShiftWorkers()
        {
            try
            {
                Response.StatusCode = 500;
                var siteWorkers = _storage.GetFreeShiftWorkers();
                var objects = SiteWorkersToDynamic(siteWorkers);
                var text = JsonSerializer.Serialize(objects);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("PositionsShiftReport")]
        public string PositionsShiftReport()
        {
            try
            {
                Response.StatusCode = 500;
                string stringDate = Request.Query["date"];
                var date = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(stringDate))
                {
                    stringDate = stringDate.Trim();
                    if (!DateTime.TryParse(stringDate, out date))
                        return $"Wrong to date {stringDate}";
                }
                string shift = Request.Query["shift"];
                if (string.IsNullOrWhiteSpace(shift))
                    shift = "day";
                shift = shift.Trim().ToLower();
                if (shift != "day" && shift != "night")
                    return $"Unknown shift {shift}";

                date = shift == "day" ? date.Date.AddHours(15) : date.Date.AddHours(27);

                var data = _storage.CreatePositionsShiftReport(date);
                var text = JsonSerializer.Serialize(data);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        [HttpGet("StaffReport")]
        public string StaffReport()
        {
            try
            {
                Response.StatusCode = 500;
                string temp = Request.Query["from_date"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "From date is empty!";
                if (!DateTime.TryParse(temp.Trim(), out DateTime fromDate))
                    return $"Wrong to date {temp}";

                temp = Request.Query["to_date"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "From date is empty!";
                if (!DateTime.TryParse(temp.Trim(), out DateTime toDate))
                    return $"Wrong to date {temp}";

                temp = Request.Query["only_linear"];
                var onlyLinear = !string.IsNullOrWhiteSpace(temp) && temp == "1";

                fromDate = fromDate.Date;
                toDate = toDate.AddDays(1);
                var data = _storage.CreateStaffReport(fromDate, toDate, onlyLinear);
                var text = JsonSerializer.Serialize(data);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }            
        }
        [HttpGet("DailyReport")]
        public string DailyReport()
        {
            try
            {
                Response.StatusCode = 500;
                string temp = Request.Query["from_date"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "From date is empty!";
                if (!DateTime.TryParse(temp.Trim(), out DateTime fromDate))
                    return $"Wrong to date {temp}";

                temp = Request.Query["to_date"];
                if (string.IsNullOrWhiteSpace(temp))
                    return "From date is empty!";
                if (!DateTime.TryParse(temp.Trim(), out DateTime toDate))
                    return $"Wrong to date {temp}";

                temp = Request.Query["only_linear"];
                var onlyLinear = !string.IsNullOrWhiteSpace(temp) && temp == "1";

                fromDate = fromDate.Date;
                toDate = toDate.AddDays(1);

                // Получаем все смены, которые попадают в указанный промежуток
                var shifts = Shift.GetShifts(fromDate, toDate);

                // Получаем массив должостей, сотрудники которых работали в течение периода
                var distinctPositions = _storage.GetDistinstPositions(fromDate, toDate);

                var elements = new List<dynamic>();
                // Проходим по всем сменам, попадающим в указанный промежуток
                foreach (var shift in shifts)
                {
                    // Для каждой смены получаем массив промежутков работ
                    var works = _storage.GetWorks(shift.From, shift.To);
                    var workers = new Dictionary<Employee, double>();
                    var auxiliaryTime = 0.0;
                    // Определяем работников, которые работали на смене и фактическое время отработки на смене
                    foreach (var work in works)
                    {
                        if (!workers.ContainsKey(work.Worker))
                            workers.Add(work.Worker, 0.0);
                        if (work.Site.Kind != "AUXILIARY")
                            workers[work.Worker] += work.Duration;
                        else
                            auxiliaryTime += work.Duration;
                    }

                    // Определяем должности, работающие на смене
                    var positions = new Dictionary<short, dynamic>();
                    foreach (var p in distinctPositions)
                    {
                        dynamic position = new ExpandoObject();
                        position.id = p.Id;
                        position.code = p.Code;
                        position.name = p.Name;
                        position.workersCount = 0;
                        position.planTime = 0.0;
                        position.factTime = 0.0;
                        position.planCost = 0.0;
                        position.factCost = 0.0;
                        positions.Add(p.Id, position);
                    }

                    foreach (var worker in workers.Keys)
                    {
                        // Если продолжительности работы работника на смене менее 10% то этого работника не учитываем
                        if (workers[worker] * 10 < worker.ShiftLength)
                            continue;                        
                        positions[worker.MajorPosition.Id].workersCount++;
                        positions[worker.MajorPosition.Id].planTime += worker.ShiftLength;
                        positions[worker.MajorPosition.Id].factTime += workers[worker];
                        positions[worker.MajorPosition.Id].planCost += worker.ShiftLength * worker.MajorPosition.LaborHourCost / 60 / 60;
                        positions[worker.MajorPosition.Id].factCost += workers[worker] * worker.MajorPosition.LaborHourCost / 60 / 60;
                    }

                    dynamic element = new ExpandoObject();
                    element.date = shift.From;
                    element.shift = shift.Kind;                    
                    element.positions = positions;
                    element.auxiliaryTime = auxiliaryTime;

                    elements.Add(element);
                }

                dynamic data = new ExpandoObject();
                data.positions = distinctPositions;
                data.elements = elements;

                var text = JsonSerializer.Serialize(data);
                Response.StatusCode = 200;
                return text;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
    }
}