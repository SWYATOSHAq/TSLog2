namespace TSLog.Yard.WebUI.Models
{
    public class OperationsFiltersBlockModel
    {
        public bool FilterByDates { get; private set; }
        public OperationTypesModel? OperationTypesModel { get; private set; }
        public VehicleKindsModel? VehicleKindsModel { get; private set; }
        public LogistsModel? LogistsModel { get; private set; }
        public GatesModel? GatesModel { get; private set; }
        public OperationStatesModel? OperationStatesModel { get; private set; }
        public PartnersModel? PartnersModel { get; private set; }        
        public bool FilterByVehicleNumber { get; private set; }
        public bool FilterByDriveName { get; private set; }
        public bool FilterByPartnerActionId { get; private set; }

        public OperationsFiltersBlockModel(bool filterByDates,
            OperationTypesModel? operationTypesModel, 
            VehicleKindsModel? vehicleKindsModel,
            LogistsModel? logistsModel,
            GatesModel? gatesModel,
            OperationStatesModel? operationStatesModel,
            PartnersModel? partnersModel,
            bool filterByVehicleNumber,
            bool filterByDriveName,
            bool filterByPartnerActionId)
        {
            FilterByDates = filterByDates;
            OperationTypesModel = operationTypesModel;
            VehicleKindsModel = vehicleKindsModel;
            LogistsModel = logistsModel;
            GatesModel = gatesModel;
            OperationStatesModel = operationStatesModel;
            PartnersModel = partnersModel;
            FilterByVehicleNumber = filterByVehicleNumber;
            FilterByDriveName = filterByDriveName;
            FilterByPartnerActionId = filterByPartnerActionId;
        }
    }
}
