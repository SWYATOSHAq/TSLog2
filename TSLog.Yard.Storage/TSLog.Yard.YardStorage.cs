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
    /// Хранилище данных для модуля двора
    /// </summary>
    public class YardStorage : IDisposable, IYardStorage
    {
        private readonly YardOracleStorage _oracleStorage;
        private readonly YardPgSqlStorage _pgsqlStorage;

        public YardStorage(string oracleDataSource, string pgsqlHost, string pgsqlDatabase)
        {
            _oracleStorage = new YardOracleStorage(oracleDataSource);
            _pgsqlStorage = new YardPgSqlStorage(pgsqlHost, pgsqlDatabase)
            {
                GetEmployeeById = _oracleStorage.GetEmployee,
                GetPartnerById = _oracleStorage.GetPartner,
                GetPositionById = _oracleStorage.GetPosition,
                GetEmployeesByShift = _oracleStorage.GetShiftEmployees,
                GetEmployees = _oracleStorage.GetEmployees
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
        #endregion

        #region Operations
        public short GetDuration(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetDuration(operationType, vehicleKind);
        }
        public int GetLaborCost(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetLaborCost(operationType, vehicleKind);
        }
        public Interval[] GetAllIntervals(DateTime from, DateTime to)
        {
            return _pgsqlStorage.GetAllIntervals(from, to);
        }
        public Interval[] GetAvalibleIntervals(DateTime from, DateTime to, OperationType operationType, Entity vehicleKind)
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
        public Operation[] GetOperations(DateTime? from, DateTime? to,
            OperationType? type,
            VehicleKind? vehicleKind,
            Entity? state,
            Gate? gate,
            Partner? partner,
            string? vehicleBrand,
            string? vehicleNumber,
            string? driverName,
            string? partnerActionId,
            Employee[] workers)
        {
            return _pgsqlStorage.GetOperations(from, to, type, vehicleKind, state, gate, partner, 
                vehicleBrand, 
                vehicleNumber, 
                driverName, 
                partnerActionId,
                workers);
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
        public WarehouseAction GetAction(ActionKind actionKind, string id)
        {
            return _oracleStorage.GetAction(actionKind, id);
        }
        public WarehouseAction GetAction(ActionKind actionKind, Partner partner, string partnerActionId)
        {
            return _oracleStorage.GetAction(actionKind, partner, partnerActionId);
        }
        public WarehouseAction[] GetActions(Operation operation)
        {
            return _pgsqlStorage.GetActions(operation);
        }
        public WarehouseAction[] FindActions(ActionKind kind, Partner? partner, string pattern)
        {
            return _oracleStorage.FindActions(kind, partner, pattern);
        }
        public void RemoveAction(Operation operation, WarehouseAction action)
        {
            _pgsqlStorage.RemoveAction(operation, action);
        }
        #endregion

        #region Workers
        public bool IsWorkerBusy(Employee employee)
        {
            return _pgsqlStorage.IsWorkerBusy(employee);
        }
        public void AddEmployee(Operation operation, Employee employee, Position position, double cost, MeasureUnit costCurrency)
        {
            _pgsqlStorage.AddEmployee(operation, employee, position, cost, costCurrency);
        }
        public Operation.OperationEmployee[] GetOperationEmployees(Operation operation)
        {
            return _pgsqlStorage.GetOperationEmployees(operation);
        }
        public void AllocateEmployees(Operation operation, DateTime? date = null)
        {
            _pgsqlStorage.AllocateEmployees(operation);
        }
        public void RemoveEmployee(Operation operation, Employee employee, Position position)
        {
            _pgsqlStorage.RemoveEmployee(operation, employee, position);
        }
        #endregion

        #region Reporting
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
        public dynamic[] GetYardData(DateTime fromDate, DateTime toDate)
        {
            return _pgsqlStorage.GetYardData(fromDate, toDate);
        }
        public dynamic[] GetBcData(DateTime fromDate, DateTime toDate, string siteCode)
        {
            return _oracleStorage.GetBcData(fromDate, toDate, siteCode);
        }
        #endregion

        public void Dispose()
        {
            _oracleStorage.Dispose();
            _pgsqlStorage.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}