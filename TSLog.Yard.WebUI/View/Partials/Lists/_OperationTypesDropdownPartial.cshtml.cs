using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class OperationTypesModel
    {
        /// <summary>
        /// Типы операций
        /// </summary>
        public OperationType[] OperationTypes { get; private set; }
        /// <summary>
        /// Выбранный тип операции (название)
        /// </summary>
        public string SelectedType { get; private set; }
        /// <summary>
        /// Признак доступности
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Признак - "маленький"
        /// </summary>
        public bool IsSmall { get; private set; }
        /// <summary>
        /// Признак обязательного атрибута
        /// </summary>
        public bool IsRequired { get; private set; }
        /// <summary>
        /// Текст по умолчанию
        /// </summary>
        public string DefaultText { get; private set; }
        public OperationTypesModel(OperationType[] types, string selectedType, bool isEnabled, bool isSmall, string defaultText, bool isRequired = false)
        {
            OperationTypes = types;
            SelectedType = !string.IsNullOrWhiteSpace(selectedType) ? selectedType.Trim() : string.Empty;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            IsRequired = isRequired;
            DefaultText = defaultText.Trim();            
        }
    }
}