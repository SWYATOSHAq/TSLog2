using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO
{
    /// <summary>
    /// Операция двора (разгрузка или погрузка автомашины)
    /// </summary>
    public partial class Operation : Entity
    {
        private const short DefaultPriority = 1;
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new long Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        /// <summary>
        /// Тип
        /// </summary>
        public OperationType Type { get; set; }
        /// <summary>
        /// Фактическая дата и время начала
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Плановая дата и время начала непосредственно работ
        /// </summary>
        public DateTime PlanWorkStartDate { get; private set; }
        /// <summary>
        /// Фактическая дата и время начала непосредственно работ
        /// </summary>
        public DateTime FactWorkStartDate { get; set; }
        /// <summary>
        /// Плановая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime PlanWorkFinishDate { get; private set; }
        /// <summary>
        /// Фактическая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime FactWorkFinishDate { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания
        /// </summary>
        public DateTime FinishDate { get; set; }
        /// <summary>
        /// Вид автомашины
        /// </summary>
        public VehicleKind VehicleKind { get; set; }
        /// <summary>
        /// Марка автомашины
        /// </summary>
        public string VehicleBrand { get; set; }
        /// <summary>
        /// Модель автомашины
        /// </summary>
        public string VehicleModel { get; set; }
        /// <summary>
        /// Номер автомашины
        /// </summary>
        public string VehicleNumber { get; set; }
        /// <summary>
        /// Номер прицепа
        /// </summary>
        public string TrailerNumber { get; set; }
        /// <summary>
        /// Ф. И. О. шофёра
        /// </summary>
        public string DriverName { get; set; }
        /// <summary>
        /// Номер телефона шофёра
        /// </summary>
        public string DriverPhoneNumber { get; set; }
        /// <summary>
        /// Логист (транспортно-логистическая компания)
        /// </summary>
        public Logist Logist { get; set; } = Logist.Null;
        /// <summary>
        /// Ворота
        /// </summary>
        public Gate Gate { get; set; } = Gate.Null;
        /// <summary>
        /// Состояние (статус)
        /// </summary>
        public Entity State { get; set; }
        /// <summary>
        /// Ответственный менеджер
        /// </summary>
        public Employee Manager { get; set; }
        /// <summary>
        /// Ответственный сотрудник склада
        /// </summary>
        public Employee ResponsibleOfficer { get; set; } = Employee.Null;
        /// <summary>
        /// Приоритет
        /// </summary>
        public int Priority { get; set; }
        public string PartnerManagerName { get; set; }
        public string PartnerManagerPhoneNumber { get; set; }
        public string PartnerManagerEmail { get; set; }
        /// <summary>
        /// Список партнеров
        /// </summary>        

        public Operation(long id, string? code, OperationType type,
            DateTime planWorkStartDate, DateTime planWorkFinishDate,
            VehicleKind vehicleKind, string vehicleBrand, string vehicleModel, string vehicleNumber, string trailerNumber,
            string driverName, string driverPhoneNumber,
            Entity state,
            short priority,
            string partnerManagerName, string partnerManagerPhoneNumber, string partnerManagerEmail,
            Employee manager)
    : base(id, code)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            PlanWorkStartDate = planWorkStartDate;
            PlanWorkFinishDate = planWorkFinishDate;
            VehicleKind = vehicleKind ?? throw new ArgumentNullException(nameof(vehicleKind));
            if (string.IsNullOrWhiteSpace(vehicleBrand))
                throw new ArgumentException("vehicle brand is empty");
            VehicleBrand = vehicleBrand.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(vehicleModel))
                throw new ArgumentException("vehicle model is empty");
            VehicleModel = vehicleModel.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(vehicleNumber))
                throw new ArgumentException("vehicle number is empty");
            VehicleNumber = vehicleNumber.Trim().ToUpper();
            TrailerNumber = string.IsNullOrWhiteSpace(trailerNumber) ? string.Empty : trailerNumber.Trim();
            if (string.IsNullOrWhiteSpace(driverName))
                throw new ArgumentException("driver name is empty");
            DriverName = driverName.Trim().ToUpper();
            DriverPhoneNumber = string.IsNullOrWhiteSpace(driverPhoneNumber) ? string.Empty : driverPhoneNumber.Trim();
            State = state ?? throw new ArgumentNullException(nameof(state));
            Priority = priority > 0 ? priority : DefaultPriority;
            PartnerManagerName = string.IsNullOrWhiteSpace(partnerManagerName) ? string.Empty : partnerManagerName.Trim();
            PartnerManagerPhoneNumber = string.IsNullOrWhiteSpace(partnerManagerPhoneNumber) ? string.Empty : partnerManagerPhoneNumber.Trim();
            PartnerManagerEmail = string.IsNullOrWhiteSpace(partnerManagerEmail) ? string.Empty : partnerManagerEmail.Trim();
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        private Operation() : base()
        {
            Type = OperationType.Empty;
            PlanWorkStartDate = DateTime.MinValue;
            PlanWorkFinishDate = DateTime.MinValue;
            VehicleKind = VehicleKind.Empty;
            VehicleBrand = string.Empty;
            VehicleModel = string.Empty;
            VehicleNumber = string.Empty;
            TrailerNumber = string.Empty;
            DriverName = string.Empty;
            DriverPhoneNumber = string.Empty;
            State = Entity.Empty;
            Priority = DefaultPriority;
            Manager = Employee.Empty;
            PartnerManagerName = string.Empty;
            PartnerManagerPhoneNumber = string.Empty;
            PartnerManagerEmail = string.Empty;
        }
        //Getdate TODO
        public string GetDate()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"( {PlanWorkStartDate:dd.MM.yyyy})");
        }
        public string GetStartTime()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"план. {PlanWorkStartDate:dd.MM.yyyy HH:mm}") +
                (FactWorkStartDate == DateTime.MinValue ? "" : $" (факт.: {FactWorkStartDate:dd.MM.yyyy HH:mm})");
        }
        public string GetFinishTime()
        {
            return (PlanWorkFinishDate == DateTime.MinValue ? "" : $"план. {PlanWorkFinishDate:dd.MM.yyyy HH:mm}") +
                (FactWorkFinishDate == DateTime.MinValue ? "" : $" (факт.: {FactWorkFinishDate:dd.MM.yyyy HH:mm})");
        }
        public string GetGateTime()
        {
            throw new NotImplementedException();
        }
        public string GetWorkTime()
        {
            var from = FactWorkStartDate == DateTime.MinValue ? "//" : $"{FactWorkStartDate:dd.MM.yyyy HH:mm}";
            var to = (FactWorkFinishDate == DateTime.MinValue) ? "//" : $"{FactWorkFinishDate:dd.MM.yyyy HH:mm}";
            return $"{from} - {to}";
        }

        public string GetPlanTime()
        {
            var from = PlanWorkStartDate == DateTime.MinValue ? "//" : $"{PlanWorkStartDate:HH:mm}";
            var to = (PlanWorkFinishDate == DateTime.MinValue) ? "//" : $"{PlanWorkFinishDate:HH:mm}";
            return $"{from} - {to}";
        }        
        private static Operation? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Operation Null
        {
            get
            {
                if (_null == null)
                    _null = new Operation(-1, string.Empty, OperationType.Null,
                        DateTime.MinValue, DateTime.MinValue,
                        VehicleKind.Null, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL",
                        Entity.Null,
                        -1,
                        "NULL", "NULL", "NULL",
                        Employee.Null);
                return _null;
            }
        }
        private static Operation? _empty = null;
        /// <summary>
        /// Empty object
        /// </summary>
        public static new Operation Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new TSLog.Yard.BO.Operation();
                return _empty;
            }
        }
        public static Operation[] Filter(Operation[] operations,
            Partner partner, OperationType operationType, VehicleKind vehicleKind, Gate gate, Entity operationState)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));
            var temp = operations.ToList();
            if (partner != null && partner != Partner.Null)
                temp = temp.FindAll(o => o.Partners.Contains(partner));
            if (operationType != null && operationType != OperationType.Null)
                temp = temp.FindAll(o => o.Type == operationType);
            if (vehicleKind != null && vehicleKind != VehicleKind.Null)
                temp = temp.FindAll(o => o.VehicleKind == vehicleKind);
            if (gate != null && gate != Gate.Null)
                temp = temp.FindAll(o => o.Gate == gate);
            if (operationState != null && operationState != Entity.Null)
                temp = temp.FindAll(o => o.State == operationState);
            return temp.ToArray();
        }        
    }
}