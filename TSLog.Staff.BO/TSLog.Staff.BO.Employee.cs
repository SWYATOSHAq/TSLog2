using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

namespace TSLog.Staff.BO
{
    /// <summary>
    /// Сотрудник
    /// </summary>
    public class Employee : Entity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new int Id
        {
            get
            {
                return (int)base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        private const double DefaultCost = 100.00;
        /// <summary>
        /// Имя пользователя в WMS системе
        /// </summary>
        public string WmsLogin { get; set; } = string.Empty;
        /// <summary>
        /// Имя пользователя в Active Directory
        /// </summary>
        public string AdLogin { get; set; } = string.Empty;
        /// <summary>
        /// Фамилия
        /// </summary>
        public string Surname { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }
        /// <summary>
        /// Отчество
        /// </summary>
        public string Patronymic { get; set; } = string.Empty;
        public new string DisplayName
        {
            get
            {
                return $"{Surname} {FirstName} {Patronymic}";
            }
        }
        /// <summary>
        /// Полное Ф. И. О.
        /// </summary>
        public string FullName
        {
            get 
            {
                return DisplayName;
            }
        }
        /// <summary>
        /// Пол
        /// </summary>
        public string Sex { get; set; } = "M";
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime BirthDate { get; set; }
        /// <summary>
        /// Гражданство (подданство)
        /// </summary>
        public string Nationality { get; set; } = string.Empty;
        /// <summary>
        /// Контактный телефон
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        /// <summary>
        /// Адрес эл. почты
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Документ, удосоверяющий личность
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;
        /// <summary>
        /// Серия документа, удостоверяющего личность
        /// </summary>
        public string DocumentSeries { get; set; } = string.Empty;
        /// <summary>
        /// Номер документа, удостоверяющего личность
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;
        /// <summary>
        /// Дата выдачи документа, удостоверяющего личность
        /// </summary>
        public DateTime DocumentIssueDate { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Организация, выдавшая документ, удостоверяющий личность
        /// </summary>
        public string DocumentIssueOrganization { get; set; } = string.Empty;
        /// <summary>
        /// Дата приёма на работу
        /// </summary>
        public DateTime EmploymentDate { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Дата увольнения
        /// </summary>
        public DateTime DismissalDate { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Компания (ТСЛ или подрядчик)
        /// </summary>
        public LegalEntity Company { get; set; }
        /// <summary>
        /// Подразделение
        /// </summary>
        public Department? Department { get; set; } = null;
        /// <summary>
        /// Стоимость часа работы
        /// </summary>
        public double Cost { get; set; } = DefaultCost;
        /// <summary>
        /// Валюта стоимости часа работы
        /// </summary>
        public MeasureUnit CostMeasureUnit { get; set; } = MeasureUnit.Empty;
        private const string DefaultWorkMode = "5/2-40";
        /// <summary>
        /// Режим работы (номер смены)
        /// </summary>
        public string WorkMode { get; set; } = DefaultWorkMode;
        /// <summary>
        /// Продолжительность рабочей смены (в секундах)
        /// </summary>
        public int ShiftLength
        {
            get
            {
                return WorkMode == "I" || WorkMode == "II" || WorkMode == "III" || WorkMode == "IV" ? 
                    (int)Math.Round(10.5 * 60 * 60) :
                    8 * 60 * 60;
            }
        }
        /// <summary>
        /// "Основная" должность
        /// </summary>
        public Position MajorPosition { get; set; } = Position.Null;

        public Employee(long id, string code,
            string surname, string firstName,
            DateTime birthDate,
            LegalEntity company,
            bool isActive,
            string note = "")
            : base(id, code, firstName, string.Empty, string.Empty, string.Empty, isActive, -1, note)
        {
            if (string.IsNullOrWhiteSpace(surname))
                throw new ArgumentException("surname is empty");
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("firstName is empty");
            Surname = surname.Trim();
            Name = firstName.Trim();
            BirthDate = birthDate;
            Company = company ?? throw new ArgumentNullException(nameof(company));
        }
        public Employee() : base()
        {
            Surname = string.Empty;
            BirthDate = DateTime.MinValue;
            Company = LegalEntity.Empty;
        }
        private static Employee? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new Employee Null
        {
            get
            {
                if (_null == null)
                    _null = new Employee(-1, "NULL", "NULL", "NULL", DateTime.MinValue, LegalEntity.Null, true, "NULL");
                return _null;
            }
        }
        private static Employee? _empty = null;
        /// <summary>
        /// Empty object
        /// </summary>
        public static new Employee Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Employee();
                return _empty;
            }
        }
    }

    public class EmployeeesFullNameComparer: IComparer<Employee>
    {
        public int Compare(Employee? employee1, Employee? employee2)
        {
            if (employee1 == null && employee2 == null)
                return 0;
            else if (employee1 == null)
                return 1;
            else if (employee2 == null)
                return -1;
            return employee1.FullName.CompareTo(employee2.FullName);
        }
    }
}