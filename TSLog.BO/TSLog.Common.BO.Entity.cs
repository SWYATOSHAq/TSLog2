namespace TSLog.Common.BO
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
        public string Note { get; set; }

        /// <summary>
        /// Признак архивной сущности
        /// </summary>
        public bool IsActive { get; protected set; }

        public Entity(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "",
            string note = "", bool isActive = true)
        {
            Id = id >= 0 ? id : -1;
            Code = !string.IsNullOrWhiteSpace(code) ? code.Trim() : string.Empty;
            Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : string.Empty;
            ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName.Trim() : string.Empty;
            AltName = !string.IsNullOrWhiteSpace(altName) ? altName.Trim() : string.Empty;
            DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : string.Empty;
            Note = !string.IsNullOrWhiteSpace(note) ? note.Trim() : string.Empty;
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
                    _null = new Entity(-1, "NULL", "NULL", "NULL", "NULL", "NULL", "NULL");
                return _null;
            }
        }
    }
}