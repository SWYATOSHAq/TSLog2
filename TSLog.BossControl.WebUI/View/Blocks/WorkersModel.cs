using TSLog.Authorization;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Models
{
    public class WorkersModel
    {
        /// <summary>
        /// Объект для проверки прав
        /// </summary>
        public IRightsChecker RightsChecker { get; private set; }
        /// <summary>
        /// Хранилище данных
        /// </summary>
        public IBossControlStorage Storage { get; private set; }
        /// <summary>
        /// Признак - "показывать доп. информацию"
        /// </summary>
        public bool DisplayAdditoonalInfo { get; private set; }
        /// <summary>
        /// HTTP запрос
        /// </summary>
        public HttpRequest Request { get; private set; }
        public WorkersModel(IRightsChecker rightsChecker, IBossControlStorage storage,
            bool displayAdditoonalInfo, HttpRequest request)
        {
            RightsChecker = rightsChecker;
            Storage = storage;
            DisplayAdditoonalInfo = displayAdditoonalInfo;
            Request = request;
        }
    }
}
