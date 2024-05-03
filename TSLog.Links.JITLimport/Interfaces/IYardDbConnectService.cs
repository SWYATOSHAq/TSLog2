using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport.Interfaces
{
    public interface IYardDbConnectService
    {
        short GetYardOperationDuration(Yard.BO.OperationType operationType, VehicleKind vehicleKind, out int standardConstantDuration, out int standardVariableDuration);
        Entity? GetYardOperationState();
        Yard.BO.OperationType? GetYardOperationType(string typeCode);
        Partner? GetYardPartner();
        Employee? GetYardPartnerManagers(Partner partner);
        VehicleKind GetYardVehicleKind(string typeOfVehicle);
        void CreateYardOperation(Operation operation, string user, WarehouseAction action);
        LineBase[] GetYardWarehouseActionLines(dynamic kind, string id);
        void GetYardAssembliedOutgo(string id, out double palletsCount, out double eachesCount);
        WarehouseAction GetYardWarehouseAction(ActionKind actionKind, string id);
        WarehouseAction[] FindWarehouseActions(ActionKind kind, Partner? partner, string pattern, bool isPartnerUser);
    }
}