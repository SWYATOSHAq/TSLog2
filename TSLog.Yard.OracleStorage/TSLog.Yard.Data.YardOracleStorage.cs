using TSLog.Common.Data;
using TSLog.LegalEntities.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Data
{
    public class YardOracleStorage : OracleStorage, IYardWmsStorage
    {
        public YardOracleStorage(string dataSource) : base(dataSource)
        {
        }

        public WarehouseAction GetAction(LegalEntity partner, ActionKind actionKind, string partnerActionId)
        {
            throw new NotImplementedException();
        }
        public LegalEntity GetPartner(string id)
        {
            throw new NotImplementedException();
        }
        public void GetEmployee(int id) 
        { 
        }
    }
}