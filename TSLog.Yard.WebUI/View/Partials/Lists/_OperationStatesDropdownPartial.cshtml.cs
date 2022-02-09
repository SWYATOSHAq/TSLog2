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
        public OperationStatesModel(Entity[] states, string selectedState, bool isEnabled, bool isSmall, string defaultText)
        {
            OperationStates = states;
            SelectedState = selectedState.Trim();
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText.Trim();
        }
    }
}