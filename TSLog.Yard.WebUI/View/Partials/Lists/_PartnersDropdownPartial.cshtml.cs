using TSLog.Partners.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class PartnersModel
    {
        public Partner[] Partners { get; private set; }
        public string SelectedPartner { get; private set; }
        public bool IsDisabled { get; private set; }
        public bool IsSmall { get; private set; }        
        public string DefaultText { get; private set; }
        public PartnersModel(Partner[] partners, string selectedPartner, bool isDisabled, bool isSmall, string defaultText)
        {
            Partners = partners;
            SelectedPartner = !string.IsNullOrWhiteSpace(selectedPartner) ? selectedPartner.Trim() : string.Empty;
            IsDisabled = isDisabled;
            IsSmall = isSmall;            
            DefaultText = defaultText.Trim();
        }
    }
}
