using TSLog.Common;
using TSLog.Common.BO;
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
                GetPositionById = _oracleStorage.GetPosition
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
        public Employee GetEmployee(int id)
        {
            return _oracleStorage.GetEmployee(id);
        }
        public Employee[] GetEmployees()
        {
            return _oracleStorage.GetEmployees();
        }
        public Employee[] FindEmployees(string pattern)
        {
            return _oracleStorage.FindEmployees(pattern);
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
        public short GetTime(OperationType operationType, VehicleKind vehicleKind)
        {
            return _pgsqlStorage.GetTime(operationType, vehicleKind);
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
        public Operation AddOperation(Operation operation)
        {
            return _pgsqlStorage.AddOperation(operation);
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
            string? driverName)
        {
            return _pgsqlStorage.GetOperations(from, to, type, vehicleKind, state, gate, partner, vehicleBrand, vehicleNumber, driverName);
        }
        public Partner[] GetOperationsPartners(DateTime from, DateTime to)
        {
            return _pgsqlStorage.GetOperationsPartners(from, to);
        }
        public void EditOperation(Operation operation)
        {
            _pgsqlStorage.EditOperation(operation);
        }
        public Gate SetOperationGate(Operation operation, Gate? gate = null)
        {
            return _pgsqlStorage.SetOperationGate(operation, gate);
        }
        public void CancelOperationGate(Operation operation)
        {
            _pgsqlStorage.CancelOperationGate(operation);
        }
        public void SetResponsibleOfficer(Operation operation, Employee officer)
        {
            _pgsqlStorage.SetResponsibleOfficer(operation, officer);
        }
        public void StartOperation(Operation operation)
        {
            _pgsqlStorage.StartOperation(operation);
        }
        public void StartOperationWork(Operation operation)
        {
            _pgsqlStorage.StartOperationWork(operation);
        }
        public void FinishOperationWork(Operation operation)
        {
            _pgsqlStorage.FinishOperationWork(operation);
        }
        public void FinishOperation(Operation operation)
        {
            _pgsqlStorage.FinishOperation(operation);
        }
        public void CancelOperation(Operation operation)
        {
            _pgsqlStorage.CancelOperation(operation);
        }
        public void DeleteOperation(Operation operation)
        {
            _pgsqlStorage.DeleteOperation(operation);
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
        public void RemoveAction(Operation operation, WarehouseAction action)
        {
            _pgsqlStorage.RemoveAction(operation, action);
        }
        #endregion

        #region Workers
        public void AddWorker(Operation operation, Employee worker, Position position)
        {
            _pgsqlStorage.AddWorker(operation, worker, position);
        }
        public Operation.OperationWorker[] GetOperationWorkers(Operation operation)
        {
            return _pgsqlStorage.GetOperationWorkers(operation);
        }
        public void RemoveWorker(Operation operation, Employee worker, Position position)
        {
            _pgsqlStorage.RemoveWorker(operation, worker, position);
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
