using TSLog.Common.BO;

namespace TSLog.WarehouseActions.BO
{
    public class WarehouseAction : Entity
    {
        public WarehouseAction(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", string note = "", bool isActive = true) 
            : base(id, code, name, shortName, altName, displayName, note, isActive)
        { } 
    }
}
