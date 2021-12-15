using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using TSLog.Yard.Data;

namespace TSLog.Yard
{
    /// <summary>
    /// Хранилище данных для модуля двора
    /// </summary>
    public class YardStorage : IDisposable, IYardStorage
    {
        private readonly YardOracleStorage _oracleStorage = null;
        private readonly YardPgSqlStorage _pgsqlStorage = null;

        public YardStorage(string oracleDataSource, string pgsqlHost, string pgsqlDatabase)
        {
            _oracleStorage = new YardOracleStorage(oracleDataSource);
            _pgsqlStorage = new YardPgSqlStorage(pgsqlHost, pgsqlDatabase);
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

        public void AddAction(YardOperation operation, WarehouseAction action)
        {
            _pgsqlStorage.AddAction(operation, action);
        }
        public YardOperation AddOperation(YardOperation operation)
        {
            return _pgsqlStorage.AddOperation(operation);
        }
        public YardOperation GetOperation(long id)
        {
            return _pgsqlStorage.GetOperation(id);
        }
        public YardOperation[] GetOperations(DateTime from, DateTime to, Partner[] partners)
        {
            return _pgsqlStorage.GetOperations(from, to, partners);
        }
        public void CancelOperation(YardOperation operation)
        {
            _pgsqlStorage.CancelOperation(operation);
        }
        public void DeleteOperation(YardOperation operation)
        {
            _pgsqlStorage.DeleteOperation(operation);
        }
        public void EditOperation(YardOperation operation)
        {
            _pgsqlStorage.EditOperation(operation);
        }
        public void FinishOperation(YardOperation operation)
        {
            _pgsqlStorage.FinishOperation(operation);
        }
        public void FinishOperationWork(YardOperation operation)
        {
            _pgsqlStorage.FinishOperationWork(operation);
        }
        public WarehouseAction GetAction(Partner partner, ActionKind actionKind, string partnerActionId)
        {
            return _oracleStorage.GetAction(partner, actionKind, partnerActionId);
        }
        public WarehouseAction[] GetActions(YardOperation operation)
        {
            return _pgsqlStorage.GetActions(operation);
        }
        public Employee GetEmployee(int id)
        {
            return _oracleStorage.GetEmployee(id);
        }
        public Employee[] GetEmployees()
        {
            return _oracleStorage.GetEmployees();
        }
        public Gate GetGate(int id)
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
        public Interval[] GetIntervals(DateTime from, DateTime to, Entity vehicleKind, Entity operationKind)
        {
            return _pgsqlStorage.GetIntervals(from, to, vehicleKind, operationKind);
        }
        public Entity GetLogist(int id)
        {
            return _pgsqlStorage.GetLogist(id);
        }
        public Entity GetLogist(string code)
        {
            return _pgsqlStorage.GetLogist(code);
        }
        public Entity[] GetLogists()
        {
            return _pgsqlStorage.GetLogists();
        }
        public Entity GetOperationState(int id)
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
        public Entity GetOperationType(int id)
        {
            return _pgsqlStorage.GetOperationType(id);
        }
        public Entity GetOperationType(string code)
        {
            return _pgsqlStorage.GetOperationType(code);
        }
        public Entity[] GetOperationTypes()
        {
            return _pgsqlStorage.GetOperationTypes();
        }
        public Partner GetPartner(string id)
        {
            return _oracleStorage.GetPartner(id);
        }
        public Partner[] GetPartners()
        {
            return _oracleStorage.GetPartners();
        }
        public Entity GetVehicleKind(int id)
        {
            return _pgsqlStorage.GetVehicleKind(id);
        }
        public Entity GetVehicleKind(string code)
        {
            return _pgsqlStorage.GetVehicleKind(code);
        }
        public Entity[] GetVehicleKinds()
        {
            return _pgsqlStorage.GetVehicleKinds();
        }
        public void LeaveGate(YardOperation operation)
        {
            _pgsqlStorage.LeaveGate(operation);
        }
        public void MoveToGate(YardOperation operation)
        {
            _pgsqlStorage.MoveToGate(operation);
        }
        public void RemoveAction(YardOperation operation, WarehouseAction action)
        {
            _pgsqlStorage.RemoveAction(operation, action);
        }
        public Gate SetOperationGate(YardOperation operation, Gate gate = null)
        {
            return _pgsqlStorage.SetOperationGate(operation, gate);
        }
        public void SetResponsibleOfficer(YardOperation operation, Employee officer)
        {
            _pgsqlStorage.SetResponsibleOfficer(operation, officer);
        }
        public void StartOperation(YardOperation operation)
        {
            _pgsqlStorage.StartOperation(operation);
        }
        public void StartOperationWork(YardOperation operation)
        {
            _pgsqlStorage.StartOperationWork(operation);
        }

        public void Dispose()
        {
            _oracleStorage.Dispose();
            _pgsqlStorage.Dispose();
        }
    }
}
