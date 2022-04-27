using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.BO
{
    /// <summary>
    /// Сотрудник, находящийся на участке работы
    /// </summary>
    public class SiteWorker : IComparable<SiteWorker>
    {
        public Site Site { get; set; }
        public Partner Partner { get; set; }
        public Employee Worker { get; set; }
        public DateTime StartDate { get; set; }
        public SiteWorker(Site site, Partner partner, Employee worker, DateTime startDate)
        {
            Site = site;
            Partner = partner;
            Worker = worker;
            StartDate = startDate;
        }

        public int CompareTo(SiteWorker? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return other.StartDate.CompareTo(StartDate);
        }
    }
}
