using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationTypesModel
    {
        public OperationType[] OperationTypes { get; private set; }
        public string SelectedType { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public OperationTypesModel(OperationType[] types, string selectedType, bool isEnabled, bool isSmall, string defaultText)
        {
            OperationTypes = types;
            SelectedType = !string.IsNullOrWhiteSpace(selectedType) ? selectedType.Trim() : string.Empty;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText.Trim();
        }
    }
}