using TSLog.Partners.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class PartnersModel
    {
        public Partner[] Partners { get; private set; }
        public string SelectedPartner { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsSmall { get; private set; }        
        public string DefaultText { get; private set; }
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public PartnersModel(Partner[] partners, string selectedPartner, bool isEnabled, bool isSmall, string defaultText, 
            string divClasses, string selectClasses)
        {
            Partners = partners;
            SelectedPartner = !string.IsNullOrWhiteSpace(selectedPartner) ? selectedPartner.Trim() : string.Empty;
            IsEnabled = isEnabled;
            IsSmall = isSmall;            
            DefaultText = defaultText.Trim();
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}
