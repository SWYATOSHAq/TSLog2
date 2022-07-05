using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.BossControl.BO
{
    /// <summary>
    /// Сотрудник, находящийся на участке работы
    /// </summary>
    public class SiteWorker : IComparable<SiteWorker>
    {
        /// <summary>
        /// Участок
        /// </summary>
        public Site Site { get; set; }
        /// <summary>
        /// Партнёр (мандант)
        /// </summary>
        public Partner Partner { get; set; }
        /// <summary>
        /// Работник (сотрудник)
        /// </summary>
        public Employee Worker { get; set; }
        /// <summary>
        /// Дата начала работы на участке
        /// </summary>
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