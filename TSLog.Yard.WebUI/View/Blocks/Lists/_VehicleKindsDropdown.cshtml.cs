using TSLog.Yard.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class VehicleKindsModel
    {
        /// <summary>
        /// Виды автомашин
        /// </summary>
        public VehicleKind[] VehicleKinds { get; private set; }
        /// <summary>
        /// Выбранный вид автомашины (название)
        /// </summary>
        public string SelectedKind { get; private set; }
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
        public VehicleKindsModel(VehicleKind[] kinds, string selectedKind, bool isEnabled, bool isSmall, string defaultText, 
            string divClasses, string selectClasses)
        {
            VehicleKinds = kinds;
            SelectedKind = !string.IsNullOrWhiteSpace(selectedKind) ? selectedKind.Trim() : string.Empty;
            IsEnabled = isEnabled;
            IsSmall = isSmall;
            IsRequired = false;
            DefaultText = defaultText.Trim();
            DivClasses = divClasses;
            SelectClasses = selectClasses;
        }
    }
}
