using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO
{
    public interface IYardStorage : IYardCommonStorage, IYardWmsStorage { }
    public interface IYardCommonStorage
    {
        public Entity GetOperationType(int id);
        public Entity GetOperationType(string code);
        public Entity[] GetOperationTypes();
        public Entity GetOperationState(int id);
        public Entity GetOperationState(string code);
        public Entity[] GetOperationStates();
        public Entity GetVehicleKind(int id);
        public Entity GetVehicleKind(string code);
        public Entity[] GetVehicleKinds();
        public Gate GetGate(int id);
        public Gate GetGate(string code);
        public Gate[] GetGates();
        public Entity GetLogist(int id);
        public Entity GetLogist(string code);
        public Entity[] GetLogists();

        public Interval[] GetIntervals(DateTime from, DateTime to, Entity vehicleKind, Entity operationKind);
        public YardOperation AddOperation(YardOperation operation);
        public YardOperation GetOperation(long id);
        public YardOperation[] GetOperations(DateTime from, DateTime to, Partner[] partners);
        public Gate SetOperationGate(YardOperation operation, Gate gate = null);
        public void EditOperation(YardOperation operation);        
        public void StartOperation(YardOperation operation);
        public void MoveToGate(YardOperation operation);
        public void SetResponsibleOfficer(YardOperation operation, Employee officer);
        public void StartOperationWork(YardOperation operation);
        public void FinishOperationWork(YardOperation operation);
        public void LeaveGate(YardOperation operation);
        public void FinishOperation(YardOperation operation);
        public void CancelOperation(YardOperation operation);
        public void DeleteOperation(YardOperation operation);

        public void AddAction(YardOperation operation, WarehouseAction action);        
        public WarehouseAction[] GetActions(YardOperation operation);
        public void RemoveAction(YardOperation operation, WarehouseAction action);
    }

    public interface IYardWmsStorage
    {
        public WarehouseAction GetAction(Partner partner, ActionKind actionKind, string partnerActionId);
        public Partner GetPartner(string id);
        public Partner[] GetPartners();
        public Employee GetEmployee(int id);
        public Employee[] GetEmployees();
    }
}
