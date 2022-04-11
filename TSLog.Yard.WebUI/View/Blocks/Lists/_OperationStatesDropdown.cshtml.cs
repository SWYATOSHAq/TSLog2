using TSLog.Common.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationStatesModel
    {
        public Entity[] OperationStates { get; private set; }
        public string SelectedState { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public OperationStatesModel(Entity[] states, string selectedState, bool isEnabled, bool isSmall, string defaultText, string divClasses, string selectClasses)
        {
            OperationStates = states;
            SelectedState = !string.IsNullOrWhiteSpace(selectedState) ? selectedState.Trim() : string.Empty;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText.Trim();
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}