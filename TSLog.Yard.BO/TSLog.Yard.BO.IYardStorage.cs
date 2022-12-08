using TSLog.BossControl.BO;
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
    public interface IYardStorage : IYardCommonStorage, IYardReportingStorage, IYardWmsStorage, IBcReportingStorage { }
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
        public string GetGateCodeBySiteCode(string siteCode);
        public Logist GetLogist(int id);
        public Logist GetLogist(string code);
        public Logist[] GetLogists();
        public short GetDuration(OperationType operationType, VehicleKind vehicleKind, out int constantDuration, out int variableDuration);
        public short GetPlanLinearWorkersCount(OperationType operationType, VehicleKind vehicleKind);
        public int GetLaborCost(OperationType operationType, VehicleKind vehicleKind);
        public int GetLinearLaborCost(OperationType operationType, VehicleKind vehicleKind);
        public Interval[] GetAllIntervals(DateTime from, DateTime to);
        public Interval[] GetAvalibleIntervals(DateTime from, DateTime to, OperationType operationType, VehicleKind vehicleKind);
        public Interval[] GetGateIntervals(Gate gate, DateTime from, DateTime to);
        public Operation AddOperation(Operation operation, string user = "");
        public Operation GetOperation(long id);
        public Operation.OperationWork[] GetOperationWorks(OperationType operationType, VehicleKind vehicleKind);
        public Operation.OperationWork[] GetOperationWorks(Operation operation);
        public Operation[] GetOperations(DateTime from, DateTime to, Partner[]? partners);
        public Operation[] GetOperations(string? id,
            DateTime? from, DateTime? to,
            OperationType? type,
            VehicleKind? vehicleKind,
            Entity? state,
            Gate? gate,
            Partner? partner,
            string? vehicleBrand,
            string? vehicleNumber,
            string? driverName,
            string? driverPhoneNumber,
            string? partnerActionId,
            Employee[] workers);
        public Operation[] GetOperationsByFactWork(DateTime? from, DateTime? to);
        public Operation[] GetActiveOperations();
        public Operation[] GetGuardOperations();
        public Partner[] GetOperationsPartners(DateTime from, DateTime to);
        public Gate SetOperationGate(Operation operation, Gate? gate = null, string user = "");
        public void CancelOperationGate(Operation operation, string user = "");
        public void EditOperation(Operation operation, string user = "");
        public void StartOperation(Operation operation, string user = "");
        public void SetResponsibleOfficer(Operation operation, Employee officer, string user = "");
        public void StartOperationWork(Operation operation, string user = "");
        public void FinishOperationWork(Operation operation, string user = "");
        public void SetOperationNote(Operation operation, string user = "");
        public void FinishOperation(Operation operation, string user = "");
        public void CancelOperation(Operation operation, string user = "");
        public void DeleteOperation(Operation operation, string user = "");
        public void AddAction(Operation operation, WarehouseAction action);
        public void RemoveAction(Operation operation, WarehouseAction action);
        public void RemoveActions(Operation operation);
        public bool IsWorkerBusy(Employee employee);
        public void AddOperationWorker(Operation operation, Employee employee, Position position, double cost, MeasureUnit costCurrency);
        public Operation.OperationWorker[] GetOperationWorkers(Operation operation);
        public void AllocateOperationWorkers(Operation operation, DateTime? date = null);
        public void RemoveOperationWorker(Operation operation, Employee employee, Position position);
    }
    public interface IYardWmsStorage
    {
        public string GetSiteCodeByGateCode(string gateCode);
        public MeasureUnit GetMeasureUnit(string code);
        public MeasureUnit[] GetMeasureUnits();
        public LineBase[] GetWarehouseActionLines(ActionKind actionKind, string actionId);
        public WarehouseAction GetWarehouseAction(ActionKind actionKind, string id);
        public WarehouseAction GetWarehouseAction(ActionKind actionKind, Partner partner, string partnerActionId);
        public WarehouseAction[] FindWarehouseActions(ActionKind kind, Partner? partner, string pattern, bool isPartnerUser);
        public void GetAssembliedOutgo(string id, out double palletsCount, out double eachesCount);
        public long GetOutgoPlanTime(string outgoId, bool isPallet, Partner partner);
        public int GetIngoPlanTime(string outgoId, bool isPallet, Partner partner);
        public LegalEntity GetLegalEntity(string id);
        public Partner GetPartner(string id);
        public Employee[] GetPartnerManagers(Partner partner);
        public Partner[] GetPartners(bool getOnlyActive = true);
        public Employee GetEmployee(int id);
        public Employee GetEmployeeByAdLogin(string login);
        public Employee[] FindEmployees(string pattern);
        public Employee[] FindEmployees(Position position, string pattern, string workMode = "");
        public Employee[] GetLinearEmployees();
        public Employee[] GetEmployees();
        public Employee[] GetPlanShiftEmployees(DateTime? date = null);
        public Employee[] GetShiftEmployees(DateTime date);
        public Department GetDepartment(short id);
        public Department GetDepartment(string code);
        public Department[] GetDepartments();
        public Position GetPosition(short id);
        public Position GetPosition(string code);
        public Position[] GetPositions();
        public Position[] GetLinePositions();
        public dynamic[] GetBossControlWorks(DateTime startDate, DateTime finishDate, string siteCode);
        public int GetBcWorkersCount(DateTime fromDate, DateTime toDate, string siteCode);
        public SiteWorker[] GetSitesWorkers(DateTime startDate, DateTime finishDate, string siteCode);
    }
    public interface IYardReportingStorage
    {
        public Operation[] GetOperationalReportData(DateTime? fromDate, DateTime? toDate);
        public Operation[] GetDispatcherReportData(DateTime? fromDate, DateTime? toDate);
        public dynamic[] GetYardStandardLabocCosts(DateTime fromDate, DateTime toDate);
        public dynamic[] GetYardAllocatedLabocCosts(DateTime fromDate, DateTime toDate);
        public dynamic[] GetYardShiftLabocCosts(DateTime date);
        public dynamic[] GetYardPerfomanceData(DateTime fromDate, DateTime toDate);
    }
    public interface IBcReportingStorage
    {
        public dynamic[] GetBcLabocCosts(DateTime fromDate, DateTime toDate);
        public dynamic[] GetBcShiftLabocCosts(DateTime date);
        public dynamic[] GetBcPerfomanceData(DateTime fromDate, DateTime toDate, string siteCode);
        public dynamic[] GetEcData(DateTime fromDate, DateTime toDate, string[] partnersIds);
    }
}