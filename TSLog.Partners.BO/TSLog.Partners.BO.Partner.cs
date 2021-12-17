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

        private static Partner? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new Partner Null
        {
            get
            {
                if (_null == null)
                    _null = new Partner("NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }

        public static string Serialize(Partner[] partnerList)
        {
            var ret = "";

            var i = 1;
            foreach (var partner in partnerList)
            {
                ret += partner.Name + ((i == partnerList.Length) ? "" : ", ");
                i++;
            }
            return ret;
        }
    }
}