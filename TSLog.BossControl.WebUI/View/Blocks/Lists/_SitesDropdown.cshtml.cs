using TSLog.Staff.BO;

namespace TSLog.BossControl.WebUI.Models
{
    public class SitesModel
    {
        public Site[] Sites { get; private set; }
        public string SelectedSite { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public SitesModel(Site[] sites, string selectedSite, bool isEnabled, bool isSmall, string defaultText, string divClasses, string selectClasses)
        {
            Sites = sites;
            SelectedSite = !string.IsNullOrWhiteSpace(selectedSite) ? selectedSite.Trim() : string.Empty ;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText;
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}