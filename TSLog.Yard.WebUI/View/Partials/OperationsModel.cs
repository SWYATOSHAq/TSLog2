using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationsModel
    {
        /// <summary>
        /// Объект для проверки прав
        /// </summary>
        public RightsChecker RightsChecker { get; private set; }
        /// <summary>
        /// Хранилище данных
        /// </summary>
        public IYardStorage Storage { get; private set; }
        /// <summary>
        /// Операции
        /// </summary>
        public Operation[] Operations { get; private set; }
        /// <summary>
        /// Признак - "показывать доп. информацию"
        /// </summary>
        public bool DisplayAdditoonalInfo { get; private set; }
        /// <summary>
        /// HTTP запрос
        /// </summary>
        public HttpRequest Request { get; private set; }
        public OperationsModel(RightsChecker rightsChecker, IYardStorage storage, Operation[] operations, 
            bool displayAdditoonalInfo, HttpRequest request)
        {
            RightsChecker = rightsChecker;
            Storage = storage;
            Operations = operations;
            DisplayAdditoonalInfo = displayAdditoonalInfo;
            Request = request;
        }
    }
}
