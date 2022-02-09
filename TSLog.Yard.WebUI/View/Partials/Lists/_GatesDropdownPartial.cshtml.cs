using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class GatesModel
    {
        public Gate[] Gates { get; private set; }
        public string SelectedGate { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public GatesModel(Gate[] gates, string selectedGate, bool isEnabled, bool isSmall, string defaultText)
        {
            Gates = gates;
            SelectedGate = selectedGate;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText;
        }
    }
}