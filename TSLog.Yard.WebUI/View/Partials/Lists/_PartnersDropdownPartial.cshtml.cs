using TSLog.Partners.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class PartnersModel
    {
        public Partner[] Partners { get; private set; }
        public bool IsDisabled { get; private set; }
        public bool IsSmall { get; private set; }
        public string SelectedCode { get; private set; }
        public string DefaultText { get; private set; }
        public PartnersModel(Partner[] partners, bool isDisabled, bool isSmall, string selectedCode, string defaultText)
        {
            Partners = partners;
            IsDisabled = isDisabled;
            IsSmall = isSmall;
            SelectedCode = selectedCode.Trim();
            DefaultText = defaultText.Trim();
        }
    }
}
