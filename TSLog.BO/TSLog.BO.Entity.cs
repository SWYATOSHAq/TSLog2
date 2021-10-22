namespace TSLog.BO
{
    public class Entity
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public Entity(long id, string code, string name = "", string note = "")
        {
            Id = id;
            Code = code;
            Name = name;
            Note = note;
        }
        
        private static Entity _null = null;
        public static Entity Null
        {
            get 
            {
                if (_null == null)
                    _null = new Entity(-1, string.Empty);
                return _null;
            }
        }
    }
}
