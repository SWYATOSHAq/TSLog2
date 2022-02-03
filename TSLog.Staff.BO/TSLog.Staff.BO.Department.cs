using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

namespace TSLog.Staff.BO
{
    /// <summary>
    /// Подразделение
    /// </summary>
    public class Department : Entity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new short Id
        {
            get 
            {
                return (short)base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        private Department? _parent = null;
        /// <summary>
        /// Вышестоящее подразделение
        /// </summary>
        public Department Parent
        {
            get => _parent ?? Null;
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
                _company = value ?? throw new ArgumentNullException(nameof(value));
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
                _head = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        public Department(short id, string code, 
            string name, string shortName, string altName, string displayName,
            bool isActive, short displayOrder, 
            LegalEntity company, Employee head,
            string note = "")
            : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _head = head ?? throw new ArgumentNullException(nameof(head));
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
