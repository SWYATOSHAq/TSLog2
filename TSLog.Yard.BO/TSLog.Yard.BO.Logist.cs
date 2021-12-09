using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    public class Logist : Entity
    {
        public Logist(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", bool isActive = true) : base(id, code, name, shortName, altName, displayName, note, isActive)
        {
            //
        }
    }
}
