using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

namespace TSLog.Staff.BO
{
    /// <summary>
    /// Подразделение
    /// </summary>
    public class Department : Entity
    {
        private Department _parent = Null;
        /// <summary>
        /// Вышестоящее подразделение
        /// </summary>
        public Department Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (this == Null || this == Empty)
                    return;
                _parent = value;
            }
        }
        /// <summary>
        /// Компания
        /// </summary>
        private LegalEntity _company;
        /// <summary>
        /// Компания
        /// </summary>
        public LegalEntity Company
        {
            get
            {
                return _company;
            }
            set
            {
                if (this == Null || this == Empty)
                    return;
                if (value == null || value == LegalEntity.Null || value == LegalEntity.Empty)
                    throw new ArgumentException("Company is null or empty");
                _company = value;
            }
        }
        private Employee _head;
        /// <summary>
        /// Руководитель подразделения
        /// </summary>
        public Employee Head 
        {
            get
            {
                return _head;   
            }
            set
            {
                if (this == Null || this == Empty)
                    return;
                if (value == null || value == Employee.Null || value == Employee.Empty)
                    throw new ArgumentException("Head is null or empty");
                _head = value;
            }
        }
        public Department(short id, string code, 
            string name, string shortName, string altName, string displayName,
            bool isActive, long displayOrder, 
            LegalEntity company, Employee head,
            string note = "")
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            if (company == null || company == LegalEntity.Null || company == LegalEntity.Empty)
                throw new ArgumentException("Company is null or empty");
            _company = company;
            if (head == null || head == Employee.Null || head == Employee.Empty)
                throw new ArgumentException("Head is null or empty");
            _head = head;
        }
        private Department(): base()
        {
            _company = LegalEntity.Null;
            _head = Employee.Null;
        }

        private static Department? _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public static new Department Null
        {
            get
            {
                if (_null == null)
                    _null = new Department(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, 
                        LegalEntity.Null, Employee.Null, "NULL");
                return _null;
            }
        }
        private static Department? _empty = null;

        /// <summary>
        /// Empty object
        /// </summary>
        public static new Department Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new Department();
                return _empty;
            }
        }
    }

    
}
