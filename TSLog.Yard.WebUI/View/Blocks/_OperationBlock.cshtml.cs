using TSLog.Authorization;
using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationModel
    {
        public RightsChecker RightChecker { get; private set; }
        public IYardStorage Storage { get; private set; }
        public Operation Operation { get; private set; }
        public bool IsEnabled { get; private set; }
        public OperationModel(RightsChecker rightsChecker, IYardStorage storage, Operation operation, bool isEnabled)
        {
            RightChecker = rightsChecker;
            Storage = storage;
            Operation = operation;
            IsEnabled = isEnabled;
        }
    }
}
