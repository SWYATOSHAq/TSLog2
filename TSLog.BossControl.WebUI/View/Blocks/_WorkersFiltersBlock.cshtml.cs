using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Models
{
    /// <summary>
    /// Модель блока ВСЕХ фильтров
    /// </summary>
    public class WorkersFiltersBlockModel
    {
        /// <summary>
        /// Хранилище объектов
        /// </summary>
        public IBossControlStorage Storage { get; private set; }
        public bool FilterBySite { get; private set; }
        public bool FilterByPartner { get; private set; }
        public WorkersFiltersBlockModel(IBossControlStorage storage, bool filterBySite = true, bool filterByPartner = true)
        {
            Storage = storage;
            FilterBySite = filterBySite;
            FilterByPartner = filterByPartner;
        }
    }
}
