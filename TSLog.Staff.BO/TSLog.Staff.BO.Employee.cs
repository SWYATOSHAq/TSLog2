using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

namespace TSLog.Staff.BO
{
    /// <summary>
    /// Сотрудник
    /// </summary>
    public class Employee : Entity
    {
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
        public string Surname { get; private set; }
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
        public string Patronymic { get; set; } = string.Empty;
        public new string DisplayName
        {
            get
            {
                return $"{Surname} {FirstName} {Patronymic}";
            }
        }
        public string Sex { get; set; } = "M";
        public DateTime BirthDate { get; private set; }
        public string Nationality { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentSeries { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime DocumentIssueDate { get; set; } = DateTime.MinValue;
        public string DocumentIssueOrganization { get; set; } = string.Empty;
        public DateTime EmploymentDate { get; set; } = DateTime.MinValue;
        public DateTime DismissalDate { get; set; } = DateTime.MinValue;
        public LegalEntity Company { get; private set; }
        public Department? Department { get; set; } = null;
        public double Cost { get; set; } = DefaultCost;
        public MeasureUnit CostMeasureUnit { get; set; } = MeasureUnit.Null;
        private const string DefaultWorkMode = "5/2-40";
        public string WorkMode { get; set; } = DefaultWorkMode;

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
        private Employee() : base()
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
}