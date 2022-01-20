using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

namespace TSLog.Staff.BO
{
    /// <summary>
    /// Сотрудник
    /// </summary>
    public class Employee : Entity
    {
        private const double DefaultCost = 100.00;
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
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
        public LegalEntity Company { get; set; }
        public Department? Department { get; set; } = null;
        public double Cost { get; set; } = DefaultCost;
        public MeasureUnit CostMeasureUnit { get; set; } = MeasureUnit.Null;

        public Employee(long id, string code, 
            string name = "", string shortName = "", string altName = "", string displayName = "",
            bool isActive = true, short displayOrder = -1, 
            string note = "") 
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        { }
        private Employee() : base()
        {
            Surname = string.Empty;
            FirstName = string.Empty;
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
                    _null = new Employee(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, "NULL");
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