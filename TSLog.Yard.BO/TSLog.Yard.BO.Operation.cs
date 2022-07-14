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
        public DateTime PlanWorkStartDate { get; set; }
        /// <summary>
        /// Фактическая дата и время начала непосредственно работ
        /// </summary>
        public DateTime FactWorkStartDate { get; set; }
        /// <summary>
        /// Плановая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime PlanWorkFinishDate { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания непосредственно работ
        /// </summary>
        public DateTime FactWorkFinishDate { get; set; }
        /// <summary>
        /// Плановая продолжительность
        /// </summary>
        public double PlanDuration
        {
            get
            {
                return (PlanWorkFinishDate - PlanWorkStartDate).TotalSeconds;
            }
        }
        /// <summary>
        /// Фактическая продолжительность
        /// </summary>
        public double FactDuration
        {
            get
            {
                return FactWorkStartDate != DateTime.MinValue ? ((FactWorkFinishDate != DateTime.MinValue ? FactWorkFinishDate : DateTime.Now) - FactWorkStartDate).TotalSeconds : 0.0;
            }
        }
        /// <summary>
        /// Фактическая дата и время окончания
        /// </summary>
        public DateTime FinishDate { get; set; }
        /// <summary>
        /// Время присутствия машины на территории
        /// </summary>
        public double PresenceTime
        {
            get 
            {
                return StartDate != DateTime.MinValue ? ((FinishDate != DateTime.MinValue ? FinishDate : DateTime.Now) - StartDate).TotalSeconds : 0.0;
            }
        }
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
        /// Полное Ф. И. О. ответственного сотрудника склада
        /// </summary>
        public string ResponsibleOfficerFullName
        {
            get
            {
                return ResponsibleOfficer != null && ResponsibleOfficer != Employee.Null && ResponsibleOfficer != Employee.Empty
                    ? ResponsibleOfficer.FullName : string.Empty;
            }
        }
        /// <summary>
        /// Приоритет
        /// </summary>
        public short Priority { get; set; }
        /// <summary>
        /// Ф. И. О. менеджера со стороны партнёра (клиента, манданта)
        /// </summary>
        public string PartnerManagerName { get; set; }
        /// <summary>
        /// Контактный телефон менеджера со стороны партнёра (клиента, манданта)
        /// </summary>
        public string PartnerManagerPhoneNumber { get; set; }
        /// <summary>
        /// Адрес эл. почты менеджера со стороны партнёра (клиента, манданта)
        /// </summary>
        public string PartnerManagerEmail { get; set; }
        /// <summary>
        /// Дата (и время) создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Кто создал (имя пользователя)
        /// </summary>
        public string Creator { get; set; } = string.Empty;
        /// <summary>
        ///  Дата (и время) последних изменений
        /// </summary>
        public DateTime EditDate { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Кто внёс последние изменения (имя пользователя)
        /// </summary>
        public string Editor { get; set; } = string.Empty;
        
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
            VehicleBrand = vehicleBrand.Trim();
            if (string.IsNullOrWhiteSpace(vehicleModel))
                throw new ArgumentException("vehicle model is empty");
            VehicleModel = vehicleModel.Trim();
            if (string.IsNullOrWhiteSpace(vehicleNumber))
                throw new ArgumentException("vehicle number is empty");
            VehicleNumber = vehicleNumber.Trim().ToUpper();
            TrailerNumber = string.IsNullOrWhiteSpace(trailerNumber) ? string.Empty : trailerNumber.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(driverName))
                throw new ArgumentException("driver name is empty");
            DriverName = driverName.Trim();
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
        public string GetDate()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"({PlanWorkStartDate:dd.MM.yyyy})");
        }
        public string GetStartTime()
        {
            return (PlanWorkStartDate == DateTime.MinValue ? "" : $"{PlanWorkStartDate:dd.MM.yyyy HH:mm} (план.)") +
                (FactWorkStartDate == DateTime.MinValue ? "" : $" / {FactWorkStartDate:dd.MM.yyyy HH:mm} (факт.)");
        }
        public string GetFinishTime()
        {
            return (PlanWorkFinishDate == DateTime.MinValue ? "" : $"{PlanWorkFinishDate:dd.MM.yyyy HH:mm} (план.)") +
                (FactWorkFinishDate == DateTime.MinValue ? "" : $" / {FactWorkFinishDate:dd.MM.yyyy HH:mm} (факт.)");
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
            Partner partner, OperationType operationType, VehicleKind vehicleKind, Gate gate, Entity operationState,
            string vehicleNumber = "", string driverName = "", string partnerActionId = "")
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

            if (!string.IsNullOrWhiteSpace(vehicleNumber))
            {
                vehicleNumber = vehicleNumber.Trim().ToUpper();
                temp = temp.FindAll(o => o.VehicleNumber.ToUpper().Contains(vehicleNumber));
            }
            if (!string.IsNullOrWhiteSpace(driverName))
            {
                driverName = driverName.Trim().ToUpper();
                temp = temp.FindAll(o => o.DriverName.ToUpper().Contains(driverName));
            }
            if (!string.IsNullOrWhiteSpace(partnerActionId))
            {
                partnerActionId = partnerActionId.Trim().ToUpper();
                temp = temp.FindAll(o => o.HasActions(partnerActionId));
            }

            return temp.ToArray();
        }    
        /// <summary>
        /// Цвет, соответствующий состоянию, для отображения в интерфейсе
        /// </summary>
        public string DisplayColor
        {
            get
            {
                var result = State.Code switch
                {
                    "CREATED" => "#ffffff",// white
                    "CONFIRMED" => "#e5e5e5",// gray
                    "STARTED" => "#b8ffc7",// green
                    "WORK_STARTED" => "#ff8282",// red
                    "WORK_FINISHED" => "#e9ff91",// yellow
                    "FINISHED" => "#d3e7ff",// blue
                    _ => "#e5e5e5",// gray
                };
                return result;
            }
        }
        /// <summary>
        /// Сравнение  операций двора плановой дате начала работ
        /// </summary>
        /// <param name="operation1">Первая операция двора</param>
        /// <param name="operation2">Вторая операция двора</param>
        /// <returns></returns>
        public static int CompareByPlanWorkStartDate(Operation operation1, Operation operation2)
        {
            return operation2.PlanWorkStartDate.CompareTo(operation1.PlanWorkStartDate);
        }
    }
}