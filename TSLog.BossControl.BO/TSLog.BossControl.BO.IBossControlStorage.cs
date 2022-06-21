using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.BO
{
    public interface IBossControlStorage
    {
        public Position[] GetPositions();
        public LegalEntity[] GetCompanies();
        public Site[] GetSites();
        public Partner[] GetPartners();
        public Employee GetEmployee(int id);        

        public SiteWorker[] GetBusyWorkers();
        public Employee[] GetFreeWorkers();
        public SiteWorker[] GetFreeShiftWorkers();

        public void EnterSite(Employee worker, Site site, Partner partner, string note = "");
        public void LeaveSite(Employee worker);

        public dynamic[] CreatePositionsShiftReport(DateTime date);
    }
}
