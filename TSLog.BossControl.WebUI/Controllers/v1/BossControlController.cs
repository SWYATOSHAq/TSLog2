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
        private dynamic WorkerToDynamic(Employee worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            dynamic result = new ExpandoObject();
            result.id = worker.Id;
            result.surname = worker.Surname;
            result.firstName = worker.FirstName;
            result.patronymic = worker.Patronymic;
            return result;
        }
        private dynamic SiteToDynamic(Site site)
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
        private dynamic PartnerToDynamic(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner));
            dynamic result = new ExpandoObject();
            result.id = partner.Id;
            result.name = partner.Name;
            return result;
        }
        private dynamic SiteWorkerToDynamic(SiteWorker siteWorker)
        {
            if (siteWorker == null)
                throw new ArgumentNullException(nameof(siteWorker));
            dynamic result = new ExpandoObject();
            result.site = SiteToDynamic(siteWorker.Site);
            result.partner = PartnerToDynamic(siteWorker.Partner);
            result.worker = WorkerToDynamic(siteWorker.Worker);
            result.startDate = siteWorker.StartDate.ToString("HH:mm:ss (yyyy.MM.dd)");
            result.startDay = siteWorker.StartDate.ToString("dd.MM.yyyy"); ;
            result.startTime = siteWorker.StartDate.ToString("HH:mm:ss"); ;

            return result;
        }
        private dynamic[] SiteWorkersToDynamic(SiteWorker[] siteWorkers)
        {
            var result = new List<dynamic>();
            foreach (var siteWorker in siteWorkers)
                if (siteWorker != null)
                    result.Add(SiteWorkerToDynamic(siteWorker));
            return result.ToArray();
        }
        private dynamic[] WorkersToDynamic(Employee[] workers)
        {
            var result = new List<dynamic>();
            foreach (var worker in workers)
                if (worker != null)
                    result.Add(WorkerToDynamic(worker));
            return result.ToArray();
        }
        [HttpGet("BusyWorkers")]
        public string GetBusyWorkers()
        {
            var siteWorkers = _storage.GetBusyWorkers();
            var objects = SiteWorkersToDynamic(siteWorkers);
            var text = JsonSerializer.Serialize(objects);
            return text;
        }
        [HttpGet("FreeWorkers")]
        public string GetFreeWorkers()
        {
            var workers = _storage.GetFreeWorkers();
            var objects = WorkersToDynamic(workers);
            var text = JsonSerializer.Serialize(objects);
            return text;
        }
        [HttpGet("FreeShiftWorkers")]
        public string GetFreeShiftWorkers()
        {
            var siteWorkers = _storage.GetFreeShiftWorkers();
            var objects = SiteWorkersToDynamic(siteWorkers);
            var text = JsonSerializer.Serialize(objects);
            return text;
        }
    }
}