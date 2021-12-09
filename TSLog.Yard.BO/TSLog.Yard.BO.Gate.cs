using TSLog.Common.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Ворота для ПРР
    /// </summary>
    public class Gate : Entity
    {
        public Gate(long id, string code, 
            string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", 
            bool isActive = true) : base(id, code, name, shortName, altName, displayName, note, isActive)
        {
            //
        }
    }
}
