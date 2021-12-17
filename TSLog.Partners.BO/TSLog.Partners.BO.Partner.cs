using TSLog.LegalEntities.BO;

namespace TSLog.Partners.BO
{
    /// <summary>
    /// Партнёр (покупатель наших складских и логистических услуг)
    /// </summary>
    public class Partner : LegalEntity
    {
        public Partner(string id, string name = "", string note = "", string info1 = "", string info2 = "")
            : base(id, name, note, info1, info2)
        { }

        public new static Partner Null => null;

        public static string Serialize(Partner[] partnerList)
        {
            var ret = "";

            var i = 0;
            foreach (var partner in partnerList)
            {
                ret += partner.DisplayName + ((i == partnerList.Length) ? "" : ", ");
                i++;
            }
            return ret;
        }
    }
}