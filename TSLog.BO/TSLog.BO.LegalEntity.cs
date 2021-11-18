namespace TSLog.BO
{
    /// <summary>
    /// Юридическое лицо (организация)
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={DD97C915-2553-4B50-8753-EFDF79AEC6C0}
    /// </summary>
    public class LegalEntity : Entity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public new string Id
        {
            get
            {
                return Code;
            }
        }

        /// <summary>
        /// Дополнительная информация (адрес)
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string Info1 { get; set; }

        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string Info2 { get; set; }

        public LegalEntity(string id, string name = "", string note = "", string info1 = "", string info2 = "")
            : base(-1, id, name, note)
        {
            Info1 = info1;
            Info2 = info2;
        }

        private static LegalEntity _null = null;

        /// <summary>
        /// Null object
        /// </summary>
        public new static LegalEntity Null
        {
            get
            {
                if (_null == null)
                    _null = new LegalEntity("NULL", "Заглушка", "Заглушка");
                return _null;
            }
        }
    }
}