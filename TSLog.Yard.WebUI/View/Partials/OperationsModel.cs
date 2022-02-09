using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationsModel
    {
        public RightsChecker RightsChecker { get; private set; }
        public IYardStorage Storage { get; private set; }
        public Operation[] Operations { get; private set; }
        public bool DisplayAdditoonalInfo { get; private set; }
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
