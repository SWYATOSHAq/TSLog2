using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class LogistsModel
    {
        public Logist[] Logists { get; private set; }
        public string SelectedLogist { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public LogistsModel(Logist[] logists, string selectedLogist, bool isEnabled, bool isSmall, string defaultText, string divClasses, string selectClasses)
        {
            Logists = logists;
            SelectedLogist = selectedLogist.Trim();
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText.Trim();
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}
