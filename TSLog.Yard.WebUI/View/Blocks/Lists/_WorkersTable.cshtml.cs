namespace TSLog.Yard.WebUI.Models
{
    public class WorkersModel
    {
        public bool IsEnabled { get; private set; }
        public WorkersModel(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}
