using TSLog.LegalEntities.BO;

namespace TSLog.BossControl.WebUI.Models
{
    public class CompaniesModel
    {
        public LegalEntity[] Companies { get; private set; }
        public string SelectedCompany { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string DefaultText { get; private set; }
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public CompaniesModel(LegalEntity[] companies, string selectedCompany, bool isEnabled, bool isSmall, string defaultText, string divClasses, string selectClasses)
        {
            Companies = companies;
            SelectedCompany = !string.IsNullOrWhiteSpace(selectedCompany) ? selectedCompany.Trim() : string.Empty ;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            DefaultText = defaultText;
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}