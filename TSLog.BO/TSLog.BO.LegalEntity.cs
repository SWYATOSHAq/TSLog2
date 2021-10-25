namespace TSLog.BO
{
    /// <summary>
    /// Юридическое лицо (организация)
    /// </summary>
    public class LegalEntity : Entity
    {
        // https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={DD97C915-2553-4B50-8753-EFDF79AEC6C0}
        public string Address { get; set; } // Дополнительная информация (адрес) 
        public string Info2 { get; set; } // Дополнительная информация 
		public string Zip { get; set; }

        public LegalEntity(long id, string code, string name = "", string note = "")
            : base(id, code, name, note)
        { }

        private static LegalEntity _null = null;
        new public static LegalEntity Null
        {
            get
            {
                if (_null == null)
                    _null = new LegalEntity(0, "NULL", "Заглушка");
                return _null;
            }
        }
    }
}
