using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
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
        public Operation AddOperation(Operation operation);
        public Gate SetOperationGate(Operation operation, Gate gate = null);
        public void EditOperation(Operation operation);        
        public void StartOperation(Operation operation);
        public void MoveToGate(Operation operation);
        public void SetResponsibleOfficer(Operation operation, Employee officer);
        public void StartOperationWork(Operation operation);
        public void FinishOperationWork(Operation operation);
        public void LeaveGate(Operation operation);
        public void FinishOperation(Operation operation);
        public void CancelOperation(Operation operation);
        public void DeleteOperation(Operation operation);

        public void AddAction(Operation operation, WarehouseAction action);        
        public WarehouseAction[] GetActions(Operation operation);
        public void RemoveAction(Operation operation, WarehouseAction action);
    }

    public interface IYardWmsStorage
    {
        public WarehouseAction GetAction(LegalEntity partner, ActionKind actionKind, string partnerActionId);
        public LegalEntity GetPartner(string id);
        public LegalEntity[] GetPartners();
        public Employee GetEmployee(int id);
        public Employee[] GetEmployees();
    }
}
