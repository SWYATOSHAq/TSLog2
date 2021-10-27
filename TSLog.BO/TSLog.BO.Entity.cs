namespace TSLog.BO
{
    /// <summary>
    /// Базовый класс для всех бизнес-оъектов
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; protected set; }
        /// <summary>
        /// Код (альтернативный строковый идентификатор)
        /// </summary>
        public string Code { get; protected set; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; protected set; } 
        /// <summary>
        /// Короткое название
        /// </summary>
        public string ShortName { get; protected set; }
        /// <summary>
        /// Альтернативное название
        /// </summary>        
        public string AltName { get; protected set; }
        /// <summary>
        /// Название для отображения
        /// </summary>
        public string DisplayName { get; protected set; }
        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; protected set; }
        /// <summary>
        /// Признак архивной сущности
        /// </summary>
        public bool IsActive { get; protected set; }
        public Entity(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "", 
            string note = "", bool isActive = true)
        {
            Id = id;
            Code = code;
            Name = name;
            ShortName = shortName;
            AltName = altName;
            DisplayName = displayName;
            Note = note;
            IsActive = isActive;
        }
        
        private static Entity _null = null;
        /// <summary>
        /// Null object
        /// </summary>
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
