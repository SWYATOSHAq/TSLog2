using TSLog.Staff.BO;

namespace TSLog.BossControl.BO
{
    public interface IBossControlStorage
    {
        public SiteWorker[] GetBusyWorkers();
        public Employee[] GetFreeWorkers();
        public SiteWorker[] GetFreeShiftWorkers();
    }
}
