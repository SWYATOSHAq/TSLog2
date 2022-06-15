using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Models
{
    public class PositionsModel
    {
        public Position[] Positions { get; private set; }
        public string SelectedPosition { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public PositionsModel(Position[] positions, string selectedPosition, bool isEnabled, bool isSmall, string defaultText, string divClasses, string selectClasses)
        {
            Positions = positions;
            SelectedPosition = !string.IsNullOrWhiteSpace(selectedPosition) ? selectedPosition.Trim() : string.Empty ;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText;
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}