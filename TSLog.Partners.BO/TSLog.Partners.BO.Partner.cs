using TSLog.LegalEntities.BO;

namespace TSLog.Partners.BO
{
    /// <summary>
    /// Партнёр (покупатель наших складских и логистических услуг)
    /// </summary>
    public class Partner : LegalEntity
    {
        public Partner(string id, string name = "", string note = "", string info1 = "", string info2 = "",
            bool isActive = true, long displayOrder = -1)
            : base(id, name, name, name, name, info1, info2, isActive, displayOrder, note)
        { }
        private Partner() : base()
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
        private static Partner? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Partner Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Partner();
                return _empty;
            }
        }

        public static string Serialize(Partner[] partners)
        {
            if (partners == null || partners.Length <= 0)
                return string.Empty;
            var result = string.Empty;
            foreach (var partner in partners)
                result += ((result.Length > 0) ? ", " : "") + partner.Name;
            return result;
        }
    }
}