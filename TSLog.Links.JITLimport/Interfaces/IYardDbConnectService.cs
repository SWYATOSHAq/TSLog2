using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
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
        Operation CreateYardOperation(Operation operation, string user);
        Operation GetYardOperation();
    }
}