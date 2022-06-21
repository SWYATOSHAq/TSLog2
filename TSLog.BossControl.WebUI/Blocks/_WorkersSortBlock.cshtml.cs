namespace TSLog.BossControl.WebUI.Models
{
    public class WorkersSortBlockModel
    {
        public IDictionary<string, string> Elements;
        public string DefaultValue { get; private set; }
        public OperationsSortBlockModel(IDictionary<string, string> elements, string defaultValue)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
            if (!Elements.ContainsKey(defaultValue))
                throw new ArgumentException($"Default value {defaultValue} not found!");
            DefaultValue = defaultValue;
        }
    }
}
