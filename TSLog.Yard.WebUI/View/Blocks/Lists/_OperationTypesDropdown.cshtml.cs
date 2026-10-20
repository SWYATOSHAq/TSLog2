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
        public string DivClasses { get; private set; }
        public string SelectClasses { get; private set; }
        public OperationTypesModel(OperationType[] types, string selectedType, bool isEnabled, bool isSmall, string defaultText, 
            string divClasses, string selectClasses)
        {
            // Компактные списки используются в фильтрах заявок и машин во дворе.
            // Полный список нужен карточке и печати для отображения исторических типов.
            OperationTypes = isSmall
                ? types.Where(type => type.Code is "I_PALLET_MIX_MIX" or "I_BOX_MIX" or "O_PALLET" or "O_BOX_MIX").ToArray()
                : types;
            SelectedType = !string.IsNullOrWhiteSpace(selectedType) ? selectedType.Trim() : string.Empty;
            if (isSmall && !OperationTypes.Any(type => type.Code == SelectedType))
                SelectedType = string.Empty;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            IsRequired = false;
            DefaultText = defaultText.Trim();
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}
