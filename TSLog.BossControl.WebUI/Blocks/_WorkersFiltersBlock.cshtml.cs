namespace TSLog.BossControl.WebUI.Models
{
    public class WorkersFiltersBlockModel
    {
        public PositionModel? PositionModel { get; private set; }
        public VehicleKindsModel? VehicleKindsModel { get; private set; }
        public LogistsModel? LogistsModel { get; private set; }
        public GatesModel? GatesModel { get; private set; }
        public OperationStatesModel? OperationStatesModel { get; private set; }
        public PartnersModel? PartnersModel { get; private set; }
        public bool FilterById { get; private set; }
        public bool FilterByVehicleNumber { get; private set; }
        public bool FilterByDriveName { get; private set; }
        public bool FilterByPartnerActionId { get; private set; }
        public bool FilterByEmployee { get; private set; }
        public bool FilterByPhoneNumber { get; private set; }

        public OperationsFiltersBlockModel(
            PositionModel? positionModel,
            VehicleKindsModel? vehicleKindsModel,
            LogistsModel? logistsModel,
            GatesModel? gatesModel,
            OperationStatesModel? operationStatesModel,
            PartnersModel? partnersModel,
            bool filterById,
            bool filterByVehicleNumber,
            bool filterByDriveName,
            bool filterByPartnerActionId,
            bool filterByEmployee,
            bool filterByPhoneNumber)
        {
            PositionTypesModel = positionModel;
            VehicleKindsModel = vehicleKindsModel;
            LogistsModel = logistsModel;
            GatesModel = gatesModel;
            OperationStatesModel = operationStatesModel;
            PartnersModel = partnersModel;
            FilterById = filterById;
            FilterByVehicleNumber = filterByVehicleNumber;
            FilterByDriveName = filterByDriveName;
            FilterByPartnerActionId = filterByPartnerActionId;
            FilterByEmployee = filterByEmployee;
            FilterByPhoneNumber = filterByPhoneNumber;
        }
    }
}
