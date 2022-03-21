namespace TSLog.Yard.WebUI.Models
{
    public class FieldsModel
    {
        /// <summary>
        /// Элемент списка атрибутов (отображение атрибута)
        /// </summary>
        public class Field
        {
            /// <summary>
            /// Код (id элемента)
            /// </summary>
            public string Code { get; private set; }
            /// <summary>
            /// Подпись (заголовок)
            /// </summary>
            public string Caption { get; private set; }
            /// <summary>
            /// Значение
            /// </summary>
            public string Value { get; private set; }
            /// <summary>
            /// Признак видимости
            /// </summary>
            public bool IsVisible { get; private set; }
            /// <summary>
            /// Признак доступности
            /// </summary>
            public bool IsEnabled { get; private set; }
            /// <summary>
            /// Признак обязательного поля
            /// </summary>
            public bool IsRequired { get; private set; }
            public Field(string code, string caption, string value, bool isVisible, bool isEnabled, bool isRequired = false)
            {
                Code = code.Trim();
                Caption = caption.Trim();
                Value = value.Trim();
                IsVisible = isVisible;
                IsEnabled = isEnabled;
                IsRequired = isRequired;
            }
        }
        /// <summary>
        /// Список элементов
        /// </summary>
        public IList<Field> Elements { get; private set;}
        public FieldsModel(IList<Field> elements)
        {
            Elements = elements;
        }
    }
}
