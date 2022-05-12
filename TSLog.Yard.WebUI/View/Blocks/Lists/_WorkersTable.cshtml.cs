using TSLog.Staff.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class WorkersModel
    {
        public bool IsEnabled { get; private set; }
        public Position[] Positions { get; private set; }
        public WorkersModel(bool isEnabled, Position[] positions)
        {
            IsEnabled = isEnabled;
            Positions = positions ?? throw new ArgumentNullException(nameof(positions));
        }
    }
}
