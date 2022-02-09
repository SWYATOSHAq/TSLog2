using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class VehicleKindsModel
    {
        public VehicleKind[] VehicleKinds { get; private set; }
        public string SelectedKind { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public VehicleKindsModel(VehicleKind[] kinds, string selectedKind, bool isEnabled, bool isSmall, string defaultText)
        {
            VehicleKinds = kinds;
            SelectedKind = selectedKind.Trim();
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText.Trim();
        }
    }
}
