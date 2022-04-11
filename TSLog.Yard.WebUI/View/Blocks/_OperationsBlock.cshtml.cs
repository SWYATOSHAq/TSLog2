using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationsBlockModel
    {
        public RightsChecker RightsChecker { get; private set; }
        public IYardStorage Storage { get; private set; }
        public bool IsEnabled { get; private set; }
        public HttpRequest Request { get; private set; }
        public OperationsBlockModel(RightsChecker rightsChecker, IYardStorage storage, bool isEnabled, HttpRequest request)
        {
            RightsChecker = rightsChecker;
            Storage = storage;
            IsEnabled = isEnabled;
            Request = request;
        }
    }
}
