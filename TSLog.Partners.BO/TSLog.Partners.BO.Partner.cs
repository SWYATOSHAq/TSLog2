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
    }
}