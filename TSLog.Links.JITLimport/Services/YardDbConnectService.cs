using TSLog.Common.BO;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using OperationType = TSLog.Yard.BO.OperationType;

namespace TSLog.Links.JITLimport.Services;

public class YardDbConnectService : IYardDbConnectService
{
    private IYardStorage _storage;
    public YardDbConnectService(IYardStorage storage)
    {
        _storage = storage;
    }

    public OperationType? GetYardOperationType(string typeCode) => _storage.GetOperationType(typeCode);
    public Entity? GetYardOperationState() => _storage.GetOperationState("CREATED");
    public Partner? GetYardPartner() => _storage.GetPartner("0042");
    public Employee? GetYardPartnerManagers(Partner partner) => _storage.GetPartnerManagers(partner)[0];
    public VehicleKind GetYardVehicleKind(string typeOfVehicle) => _storage.GetVehicleKind(typeOfVehicle);
    public Int16 GetYardOperationDuration(OperationType operationType, VehicleKind vehicleKind, out int standardConstantDuration, out int standardVariableDuration) =>
        _storage.GetDuration(operationType, vehicleKind, out standardConstantDuration, out standardVariableDuration);
    public void CreateYardOperation(Operation operation, string user, WarehouseAction[] actions) 
    {
        var createdOperation = _storage.AddOperation(operation, user);
        foreach (var action in actions)
        {
            _storage.AddAction(createdOperation, action);
        }        
    }
    public LineBase[] GetYardWarehouseActionLines(dynamic kind, string id) => _storage.GetWarehouseActionLines(kind, id);
    public WarehouseAction GetYardWarehouseAction(ActionKind actionKind, string id) => _storage.GetWarehouseAction(actionKind, id);
    public WarehouseAction GetYardWarehouseAction(ActionKind actionKind, Partner partner, string id) => _storage.GetWarehouseAction(actionKind, partner, id);
    public void GetYardAssembliedOutgo(string id, out double palletsCount, out double eachesCount) => _storage.GetAssembliedOutgo(id, out palletsCount, out eachesCount);
    public WarehouseAction[] FindWarehouseActions(ActionKind kind, Partner? partner, string pattern, bool isPartnerUser) => _storage.FindWarehouseActions(kind, partner, pattern, isPartnerUser);
}
