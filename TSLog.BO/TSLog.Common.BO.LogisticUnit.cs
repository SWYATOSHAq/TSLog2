using System;

namespace TSLog.Common.BO
{
    /// <summary>
    /// Логистическая единица (паллет, ...)
    /// </summary>
    public class LogisticUnit
    {
        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Serial shipment container code
        /// </summary>
        public string Sscc { get; private set; }

        public LogisticUnit(string type, string code)
        {
            Type = !string.IsNullOrWhiteSpace(type) ? type.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            Sscc = code.Trim();
        }
    }
}