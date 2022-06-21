using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Models
{
    public class WorkersFiltersBlockModel
    {
        public IBossControlStorage Storage;
        public WorkersFiltersBlockModel(IBossControlStorage storage)
        {
            Storage = storage;
        }
    }
}
