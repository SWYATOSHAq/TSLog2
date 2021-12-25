using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO
{
    public interface IYardStorage : IYardCommonStorage, IYardWmsStorage { }
    public interface IYardCommonStorage
    {
        public OperationType GetOperationType(int id);
        public OperationType GetOperationType(string code);        
        public OperationType[] GetOperationTypes(Partner partner);
        public OperationType[] GetOperationTypes();
        public Entity GetOperationState(int id);
        public Entity GetOperationState(string code);
        public Entity[] GetOperationStates();
        public Entity GetVehicleKind(int id);
        public Entity GetVehicleKind(string code);
        public Entity[] GetVehicleKinds();
        public Gate GetGate(int id);
        public Gate GetGate(string code);
        public Gate[] GetGates();
        public Logist GetLogist(int id);
        public Logist GetLogist(string code);
        public Logist[] GetLogists();

        public short GetTime(OperationType operationType, Entity vehicleKind);
        public Interval[] GetAllIntervals(DateTime from, DateTime to);
        public Interval[] GetAvalibleIntervals(DateTime from, DateTime to, OperationType operationType, Entity vehicleKind);
        public Interval[] GetGateIntervals(Gate gate, DateTime from, DateTime to);
        public Operation AddOperation(Operation operation);
        public Operation GetOperation(long id);
        public IDictionary<Position, short> GetWorkers(OperationType operationType, Entity vehicleKind);
        public IDictionary<Position, short> GetWorkers(Operation operation);
        public Operation[] GetOperations(DateTime from, DateTime to, Partner[] partners);
        public Partner[] GetOperationsPartners(DateTime from, DateTime to);
        public Gate SetOperationGate(Operation operation, Gate? gate = null);
        public void CancelOperationGate(Operation operation);
        public void EditOperation(Operation operation);        
        public void StartOperation(Operation operation);        
        public void SetResponsibleOfficer(Operation operation, Employee officer);
        public void StartOperationWork(Operation operation);
        public void FinishOperationWork(Operation operation);        
        public void FinishOperation(Operation operation);
        public void CancelOperation(Operation operation);
        public void DeleteOperation(Operation operation);

        public void AddAction(Operation operation, WarehouseAction action);        
        public WarehouseAction[] GetActions(Operation operation);
        public void RemoveAction(Operation operation, WarehouseAction action);
    }
    public interface IYardWmsStorage
    {
        public WarehouseAction GetAction(ActionKind actionKind, string id);
        public WarehouseAction GetAction(ActionKind actionKind, Partner partner, string partnerActionId);        
        public Partner GetPartner(string id);
        public Employee[] GetPartnerManagers(Partner partner);
        public Partner[] GetPartners();
        public Employee GetEmployee(int id);
        public Employee GetManager(Partner partner);
        public Employee[] GetEmployees();
        public Position GetPosition(int id);
        public Position GetPosition(string code);
        public Position[] GetPositions();
    }
}
