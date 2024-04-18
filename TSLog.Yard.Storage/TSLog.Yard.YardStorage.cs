using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TSLog.BossControl.BO;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using TSLog.Yard.Data;
using OperationType = TSLog.Yard.BO.OperationType;

namespace TSLog.Yard
{
    /// <summary>
    /// Хранилище данных для модуля двора.
    /// </summary>
    public class YardStorage : IDisposable, IYardStorage
    {
        private const int DefaultMaxRowsCount = 16;
        private readonly YardOracleStorage _oracleStorage;
        private readonly YardPgSqlStorage _pgsqlStorage;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Максимальное количество строк для вывода.
        /// </summary>
        public int MaxRowsCount
        {
            set
            {
                _oracleStorage.MaxRowsCount = value > 0 ? value : DefaultMaxRowsCount;
            }
        }

        public YardStorage(string oracleDataSource, string pgsqlHost, string pgsqlDatabase, ILoggerFactory loggerFactory,
            IDictionary<string, object>? pgsqlExtensionParameters = null)
        {
            _oracleStorage = new YardOracleStorage(oracleDataSource);
            _loggerFactory = loggerFactory;
            _pgsqlStorage = new YardPgSqlStorage(pgsqlHost, pgsqlDatabase, _loggerFactory, pgsqlExtensionParameters)
            {
                GetEmployeeById = _oracleStorage.GetEmployee,
                GetPartnerById = _oracleStorage.GetPartner,
                GetPositionById = _oracleStorage.GetPosition,
                GetEmployeesByShift = _oracleStorage.GetShiftEmployees,
                GetEmployees = _oracleStorage.GetEmployees,
                GetWarehouseAction = _oracleStorage.GetWarehouseAction                
            };
        }
        public void ConnectOracle(string userName, string password)
        {
            _oracleStorage.Connect(userName, Utils.StringToSecureString(password));
        }
        public void SetOracleCommonSchema(string commonSchema)
        {
            _oracleStorage.CommonSchema = commonSchema;
        }
        public void SetOracleStaffSchema(string staffShcema)
        {
            _oracleStorage.StaffSchema = staffShcema;
        }
        public void SetOracleLegalEntitiesSchema(string legalEntitiesSchema)
        {
            _oracleStorage.LegalEntitiesSchema = legalEntitiesSchema;
        }
        public void SetOraclePartnersSchema(string partnersSchema)
        {
            _oracleStorage.PartnersSchema = partnersSchema;
        }
        public void DisconnectOracle()
        {
            _oracleStorage.Disconnect();
        }
        public void ConnectPgSql(string userName, string password)
        {
            _pgsqlStorage.Connect(userName, Utils.StringToSecureString(password));
        }
        public void SetPgsqlYardSchema(string yardSchema)
        {
            _pgsqlStorage.YardSchema = yardSchema;
        }
        public void DisconnectPgsql()
        {
            _pgsqlStorage.Disconnect();
        }

        #region Domains
        public MeasureUnit GetMeasureUnit(string code)
        {
            return _oracleStorage.GetMeasureUnit(code);
        }
        public MeasureUnit[] GetMeasureUnits()
        {
            return _oracleStorage.GetMeasureUnits();
        }
        public Employee GetEmployee(int id)
        {
            return _oracleStorage.GetEmployee(id);
        }
        public Employee GetEmployeeByAdLogin(string login)
        {
            return _oracleStorage.GetEmployeeByAdLogin(login);
        }
        public Employee[] GetLinearEmployees()
        {
            return _oracleStorage.GetLinearEmployees();
        }
        public Employee[] GetEmployees()
        {
            return _oracleStorage.GetEmployees();
        }
        public Employee[] GetPlanShiftEmployees(DateTime? date = null)
        {
            return _oracleStorage.GetPlanShiftEmployees(date);
        }
        public Employee[] GetShiftEmployees(DateTime date)
        {
            return _oracleStorage.GetShiftEmployees(date);
        }
        public Employee[] FindEmployees(string pattern)
        {
            return _oracleStorage.FindEmployees(pattern);
        }
        public Employee[] FindEmployees(Position position, string pattern, string workMode = "")
        {
            return _oracleStorage.FindEmployees(position, pattern, workMode);
        }
        public Department GetDepartment(short id)
        {
            return _oracleStorage.GetDepartment(id);
        }
        public Department GetDepartment(string code)
        {
            return _oracleStorage.GetDepartment(code);
        }
        public Department[] GetDepartments()
        {
            return _oracleStorage.GetDepartments();
        }
        public Position GetPosition(short id)
        {
            return _oracleStorage.GetPosition(id);
        }
        public Position GetPosition(string code)
        {
            return _oracleStorage.GetPosition(code);
        }
        public Position[] GetPositions()
        {
            return _oracleStorage.GetPositions();
        }
        public Position[] GetLinePositions()
        {
            return _oracleStorage.GetLinePositions();
        }
        public Gate GetGate(short id)
        {
            return _pgsqlStorage.GetGate(id);
        }
        public Gate GetGate(string code)
        {
            return _pgsqlStorage.GetGate(code);
        }
        public Gate[] GetGates()
        {
            return _pgsqlStorage.GetGates();
        }
        public List<string> GetPartnerEmails(string partnerId)
        {
            return _pgsqlStorage.GetPartnerEmails(partnerId);
        }
        public Gate[] GetAvalibleGates(OperationType operationType, VehicleKind vehicleKind, DateTime startDate, DateTime finishDate, long operationId)
        {
            return _pgsqlStorage.GetAvalibleGates(operationType, vehicleKind, startDate, finishDate, operationId);
        }
        public Logist GetLogist(int id)
        {
            return _pgsqlStorage.GetLogist(id);
        }
        public Logist GetLogist(string code)
        {
            return _pgsqlStorage.GetLogist(code);
        }
        public Logist[] GetLogists()
        {
            return _pgsqlStorage.GetLogists();
        }
        public Entity GetOperationState(short id)
        {
            return _pgsqlStorage.GetOperationState(id);
        }
        public Entity GetOperationState(string code)
        {
            return _pgsqlStorage.GetOperationState(code);
        }
        public Entity[] GetOperationStates()
        {
            return _pgsqlStorage.GetOperationStates();
        }
        public void AddOperationLog(Operation operation, string status, string user)
        {
            _pgsqlStorage.AddOperationLog(operation, status, user);
        }
        public OperationType GetOperationType(short id)
        {
            return _pgsqlStorage.GetOperationType(id);
        }
        public OperationType GetOperationType(string code)
        {
            return _pgsqlStorage.GetOperationType(code);
        }
        public OperationType[] GetOperationTypes(Partner partner)
        {
            return _pgsqlStorage.GetOperationTypes(partner);
        }
        public OperationType[] GetOperationTypes()
        {
            return _pgsqlStorage.GetOperationTypes();
        }
        public LegalEntity GetLegalEntity(string id)
        {
            return _oracleStorage.GetLegalEntity(id);
        }
        public Partner GetPartner(string id)
        {
            return _oracleStorage.GetPartner(id);
        }
        public Employee[] GetPartnerManagers(Partner partner)
        {
            return _oracleStorage.GetPartnerManagers(partner);
        }
        public Partner[] GetPartners(bool getOnlyActive = true)
        {
            return _oracleStorage.GetPartners(getOnlyActive);
        }
        public VehicleKind GetVehicleKind(short id)
        {
            return _pgsqlStorage.GetVehicleKind(id);
        }
        public VehicleKind GetVehicleKind(string code)
        {
            return _pgsqlStorage.GetVehicleKind(code);
        }
        public VehicleKind[] GetVehicleKinds()
        {
            return _pgsqlStorage.GetVehicleKinds();
        }
        public Work GetWorkById(short id)
        {
            return _pgsqlStorage.GetWorkById(id);
        }
        public Work GetWorkByCode(string code)
        {
            return _pgsqlStorage.GetWorkByCode(code);
        }
        public Work[] GetWorks()
        {
            return _pgsqlStorage.GetWorks();
        }
        public string GetGateCodeBySiteCode(string siteCode)
        {
            return _pgsqlStorage.GetGateCodeBySiteCode(siteCode);
        }
        public string GetSiteCodeByGateCode(string gateCode)
        {
            return _oracleStorage.GetSiteCodeByGateCode(gateCode);
        }
        public int GetSiteIdBySiteCode(string siteCode)
        {
            return _oracleStorage.GetSiteIdBySiteCode(siteCode);
        }
        public bool CheckWorkerLeaveSite(int workerId, int siteId, string partnerId, DateTime factStartWorkDate)
        {
            return _oracleStorage.CheckWorkerLeaveSite(workerId, siteId, partnerId, factStartWorkDate);
        }
        public bool CheckGateForFreeBeforeFinishWork(string gateCode)
        {
            return _oracleStorage.CheckGateForFreeBeforeFinishWork(gateCode);
        }
        #endregion

        #region Operations
        public short GetDuration(OperationType operationType, VehicleKind vehicleKind, out int constantDuration, out int variableDuration)
        {
            return _pgsqlStorage.GetDuration(operationType, vehicleKind, out constantDuration, out variableDuration);
        }
        public short GetPlanLinearWorkersCount(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetPlanLinearWorkersCount(operationType, vehicleKind);
        }
        public int GetLaborCost(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetLaborCost(operationType, vehicleKind);
        }
        public int GetLinearLaborCost(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetLinearLaborCost(operationType, vehicleKind);
        }
        public Interval[] GetAllIntervals(DateTime from, DateTime to)
        {
            return _pgsqlStorage.GetAllIntervals(from, to);
        }
        public Interval[] GetAvalibleIntervals(DateTime from, DateTime to, OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetAvalibleIntervals(from, to, operationType, vehicleKind);
        }
        public Interval[] GetGateIntervals(Gate gate, DateTime from, DateTime to)
        {
            return _pgsqlStorage.GetGateIntervals(gate, from, to);
        }
        public Operation AddOperation(Operation operation, string user = "")
        {
            return _pgsqlStorage.AddOperation(operation, user);
        }
        public Operation GetOperation(long id)
        {
            return _pgsqlStorage.GetOperation(id);
        }
        public Operation.OperationWork[] GetOperationWorks(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetOperationWorks(operationType, vehicleKind);
        }
        public Operation.OperationWork[] GetOperationWorks(Operation operation)
        {
            return _pgsqlStorage.GetOperationWorks(operation);
        }
        public Operation[] GetOperations(DateTime from, DateTime to, Partner[]? partners)
        {
            return _pgsqlStorage.GetOperations(from, to, partners);
        }
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
            Employee[] workers)
        {
            return _pgsqlStorage.GetOperations(id, from, to, type, vehicleKind, state, gate, partner,
                vehicleBrand,
                vehicleNumber,
                driverName,
                driverPhoneNumber,
                partnerActionId,
                workers);
        }
        public Operation[] GetOperationsByFactWork(DateTime? from, DateTime? to)
        {
            return _pgsqlStorage.GetOperationsByFactWork(from, to);
        }
        public Operation[] GetActiveOperations()
        {
            return _pgsqlStorage.GetActiveOperations();
        }
        public Operation[] GetGuardOperations()
        {
            return _pgsqlStorage.GetGuardOperations();
        }
        public Partner[] GetOperationsPartners(DateTime from, DateTime to)
        {
            return _pgsqlStorage.GetOperationsPartners(from, to);
        }
        public void EditOperation(Operation operation, string user = "")
        {
            _pgsqlStorage.EditOperation(operation, user);
        }
        public Gate SetOperationGate(Operation operation, Gate? gate = null, string user = "")
        {
            return _pgsqlStorage.SetOperationGate(operation, gate, user);
        }
        public void CancelOperationGate(Operation operation, string user = "")
        {
            _pgsqlStorage.CancelOperationGate(operation, user);
        }
        public void SetResponsibleOfficer(Operation operation, Employee officer, string user = "")
        {
            _pgsqlStorage.SetResponsibleOfficer(operation, officer, user);
        }
        public void StartOperation(Operation operation, string user = "")
        {
            _pgsqlStorage.StartOperation(operation, user);
        }
        public void StartOperationWork(Operation operation, string user = "")
        {
            _pgsqlStorage.StartOperationWork(operation, user);
        }
        public void FinishOperationWork(Operation operation, string user = "")
        {
            _pgsqlStorage.FinishOperationWork(operation, user);
        }
        public void SetOperationNote(Operation operation, string user = "")
        {
            _pgsqlStorage.SetOperationNote(operation, user);
        }
        public void FinishOperation(Operation operation, string user = "")
        {
            _pgsqlStorage.FinishOperation(operation, user);
        }
        public void CancelOperation(Operation operation, string user = "")
        {
            _pgsqlStorage.CancelOperation(operation, user);
        }
        public void DeleteOperation(Operation operation, string user = "")
        {
            _pgsqlStorage.DeleteOperation(operation, user);
        }
        #endregion

        #region Actions
        public void AddAction(Operation operation, WarehouseAction action)
        {
            _pgsqlStorage.AddAction(operation, action);
        }
        public LineBase[] GetWarehouseActionLines(ActionKind actionKind, string actionId)
        {
            return _oracleStorage.GetWarehouseActionLines(actionKind, actionId);
        }
        public WarehouseAction GetWarehouseAction(ActionKind actionKind, string id)
        {
            return _oracleStorage.GetWarehouseAction(actionKind, id);
        }
        public WarehouseAction GetWarehouseAction(ActionKind actionKind, Partner partner, string partnerActionId)
        {
            return _oracleStorage.GetWarehouseAction(actionKind, partner, partnerActionId);
        }
        public WarehouseAction[] GetActions(Operation operation)
        {
            return _pgsqlStorage.GetActions(operation);
        }
        public WarehouseAction[] FindWarehouseActions(ActionKind kind, Partner? partner, string pattern, bool isPartnerUser)
        {
            return _oracleStorage.FindWarehouseActions(kind, partner, pattern, isPartnerUser);
        }
        public void GetAssembliedOutgo(string id, out double palletsCount, out double eachesCount)
        {
            _oracleStorage.GetAssembliedOutgo(id, out palletsCount, out eachesCount);
        }
        public long GetOutgoPlanTime(string outgoId, bool isPallet, Partner partner)
        {
            return _oracleStorage.GetOutgoPlanTime(outgoId, isPallet, partner);
        }
        public int GetIngoPlanTime(string outgoId, bool isPallet, Partner partner)
        {
            return _oracleStorage.GetIngoPlanTime(outgoId, isPallet, partner);
        }
        public void RemoveAction(Operation operation, WarehouseAction action)
        {
            _pgsqlStorage.RemoveAction(operation, action);
        }
        public void RemoveActions(Operation operation)
        {
            _pgsqlStorage.RemoveActions(operation);
        }
        #endregion

        #region Workers
        public bool IsWorkerBusy(Employee employee)
        {
            return _pgsqlStorage.IsWorkerBusy(employee);
        }
        public void AddOperationWorker(Operation operation, Employee employee, Position position, double cost, MeasureUnit costCurrency)
        {
            _pgsqlStorage.AddOperationWorker(operation, employee, position, cost, costCurrency);
        }
        public Operation.OperationWorker[] GetOperationWorkers(Operation operation)
        {
            return _pgsqlStorage.GetOperationWorkers(operation);
        }
        public void AllocateOperationWorkers(Operation operation, DateTime? date = null)
        {
            _pgsqlStorage.AllocateOperationWorkers(operation);
        }
        public void RemoveOperationWorker(Operation operation, Employee employee, Position position)
        {
            _pgsqlStorage.RemoveOperationWorker(operation, employee, position);
        }
        public dynamic[] GetBossControlWorks(DateTime startDate, DateTime finishDate, string siteCode)
        {
            return _oracleStorage.GetBossControlWorks(startDate, finishDate, siteCode);
        }
        public int GetBcWorkersCount(DateTime fromDate, DateTime toDate, string siteCode)
        {
            return _oracleStorage.GetBcWorkersCount(fromDate, toDate, siteCode);
        }
        public SiteWorker[] GetSitesWorkers(DateTime startDate, DateTime finishDate, string siteCode)
        {
            return _oracleStorage.GetSitesWorkers(startDate, finishDate, siteCode);
        }
        #endregion

        #region Reporting
        public Operation[] GetOperationalReportData(DateTime? fromDate, DateTime? toDate)
        {
            return _pgsqlStorage.GetOperationalReportData(fromDate, toDate);
        }
        public Operation[] GetDispatcherReportData(DateTime? fromDate, DateTime? toDate)
        {
            return _pgsqlStorage.GetDispatcherReportData(fromDate, toDate);
        }
        public dynamic[] GetYardStandardLabocCosts(DateTime fromDate, DateTime toDate)
        {
            return _pgsqlStorage.GetYardStandardLabocCosts(fromDate, toDate);
        }
        public dynamic[] GetYardAllocatedLabocCosts(DateTime fromDate, DateTime toDate)
        {
            return _pgsqlStorage.GetYardAllocatedLabocCosts(fromDate, toDate);
        }
        public dynamic[] GetBcLabocCosts(DateTime fromDate, DateTime toDate)
        {
            return _oracleStorage.GetBcLabocCosts(fromDate, toDate);
        }
        public dynamic[] GetBcShiftLabocCosts(DateTime date)
        {
            return _oracleStorage.GetBcShiftLabocCosts(date);
        }
        public dynamic[] GetYardShiftLabocCosts(DateTime date)
        {
            return _pgsqlStorage.GetYardShiftLabocCosts(date);
        }
        public dynamic[] GetYardPerfomanceData(DateTime fromDate, DateTime toDate)
        {
            return _pgsqlStorage.GetYardPerfomanceData(fromDate, toDate);
        }
        public dynamic[] GetGateManagementData(DateTime fromDate, DateTime toDate)
        {
            return _pgsqlStorage.GetGateManagementData(fromDate, toDate);
        }
        public dynamic[] GetBcPerfomanceData(DateTime fromDate, DateTime toDate, string siteCode)
        {
            return _oracleStorage.GetBcWorks(fromDate, toDate, siteCode);
        }
        public dynamic[] GetEcData(DateTime fromDate, DateTime toDate, string[] partnersIds)
        {
            return _oracleStorage.GetEcData(fromDate, toDate, partnersIds);
        }
        public dynamic GetEcDetailedData(DateTime fromDate, DateTime toDate, string partnersId)
        {
            return _oracleStorage.GetEcDetailedData(fromDate, toDate, partnersId);
        }
        #endregion

        #region Testing

        public void TruncateReferenceTables()
        {
            // На всякий случай забью сюда этот костыль что бы нельзя было почистить данные на проде!
            if (_pgsqlStorage.Host == "10.50.254.14")
                throw new ApplicationException("You can't truncate tables on production database!");

            _pgsqlStorage.TruncateReferenceTables();
        }
        public void TruncateDataTables()
        {
            // На всякий случай забью сюда этот костыль что бы нельзя было почистить данные на проде!
            if (_pgsqlStorage.Host == "10.50.254.14")
                throw new ApplicationException("You can't truncate tables on production database!");

            _pgsqlStorage.TruncateDataTables();
        }

        # endregion
        public void Dispose()
        {
            _oracleStorage.Dispose();
            _pgsqlStorage.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
