using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Common.BO;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.Yard;
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
    public Operation CreateYardOperation(Operation operation, string user) => _storage.AddOperation(operation, user);
    public Operation GetYardOperation() => _storage.GetOperation(10042780);



    //public CreateYardOperation(YardStorage storage)
    //{
    //    storage.AddOperation
    //}
}
