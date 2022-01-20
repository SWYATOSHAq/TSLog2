using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Интерфейс хранилища данных для модуля упраления двором
    /// </summary>
    public interface IYardStorage : IYardCommonStorage, IYardWmsStorage { }
    public interface IYardCommonStorage
    {
        public OperationType GetOperationType(short id);
        public OperationType GetOperationType(string code);        
        public OperationType[] GetOperationTypes(Partner partner);
        public OperationType[] GetOperationTypes();
        public Entity GetOperationState(short id);
        public Entity GetOperationState(string code);
        public Entity[] GetOperationStates();
        public VehicleKind GetVehicleKind(short id);
        public VehicleKind GetVehicleKind(string code);
        public VehicleKind[] GetVehicleKinds();
        public Work GetWorkById(short id);
        public Work GetWorkByCode(string code);
        public Work[] GetWorks();
        public Gate GetGate(short id);
        public Gate GetGate(string code);
        public Gate[] GetGates();
        public Logist GetLogist(int id);
        public Logist GetLogist(string code);
        public Logist[] GetLogists();
        public short GetTime(OperationType operationType, VehicleKind vehicleKind);
        public Interval[] GetAllIntervals(DateTime from, DateTime to);
        public Interval[] GetAvalibleIntervals(DateTime from, DateTime to, OperationType operationType, Entity vehicleKind);
        public Interval[] GetGateIntervals(Gate gate, DateTime from, DateTime to);
        public Operation AddOperation(Operation operation);
        public Operation GetOperation(long id);
        public Operation.OperationWork[] GetOperationWorks(OperationType operationType, VehicleKind vehicleKind);
        public Operation.OperationWork[] GetOperationWorks(Operation operation);
        public Operation[] GetOperations(DateTime from, DateTime to, Partner[]? partners);
        public Operation[] GetOperations(DateTime? from, DateTime? to,
            OperationType? type,
            VehicleKind? vehicleKind,
            Entity? state,
            Gate? gate,
            Partner? partner,
            string? vehicleBrand,
            string? vehicleNumber,
            string? driverName);
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
        public void AddWorker(Operation operation, Employee worker, Position position);
        public Operation.OperationWorker[] GetOperationWorkers(Operation operation);
        public void RemoveWorker(Operation operation, Employee worker, Position position);
    }
    public interface IYardWmsStorage
    {
        public WarehouseAction GetAction(ActionKind actionKind, string id);
        public WarehouseAction GetAction(ActionKind actionKind, Partner partner, string partnerActionId);
        public LegalEntity GetLegalEntity(string id);        
        public Partner GetPartner(string id);
        public Employee[] GetPartnerManagers(Partner partner);
        public Partner[] GetPartners(bool getOnlyActive = false);
        public Employee GetEmployee(int id);
        public Employee[] FindEmployees(string pattern);
        public Employee[] FindEmployees(Position position, string pattern);
        public Employee[] GetEmployees();
        public Department GetDepartment(short id);
        public Department GetDepartment(string code);
        public Department[] GetDepartments();
        public Position GetPosition(short id);
        public Position GetPosition(string code);
        public Position[] GetPositions();
    }
}
