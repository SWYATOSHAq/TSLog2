using System.Text;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;
using TSLog.Partners.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Yard.BO;

/// <summary>
/// Операция двора (разгрузка или погрузка автомашины).
/// </summary>
public partial class Operation : Entity
{
    private readonly IDictionary<string, WarehouseAction> _actions = new Dictionary<string, WarehouseAction>();

    public WarehouseAction GetAction(ActionKind kind, string id)
    {
        var k = kind == ActionKind.Outgo ? "O" : "I";
        var key = $"{k}{id}";
        if (_actions.TryGetValue(key, out WarehouseAction? value))
            return value;
        throw new WarehouseActionNotFoundException(id);
    }
    public WarehouseAction GetAction(ActionKind kind, Partner partner, string partnerActionNumber)
    {
        var actions = _actions.Values.ToList();
        var action = actions.Find(a => a.Kind == kind
            && a.Partner.Id == partner.Id
            && a.PartnerActionId == partnerActionNumber);
        if (action != null)
            return action;
        throw new WarehouseActionNotFoundException(partner.Id, partnerActionNumber);
    }
    public WarehouseAction[] GetActions()
    {
        return _actions.Values.ToArray();
    }
    public void AddAction(WarehouseAction action)
    {
        if (_actions == null)
            throw new ArgumentNullException(nameof(action));

        var kind = action.Kind == ActionKind.Outgo ? "O" : "I";
        var key = $"{kind}{action.Id}";
        if (!_actions.ContainsKey(key))
            _actions.Add(key, action);
    }
    public void AddActions(WarehouseAction[] actions)
    {
        foreach (var action in actions)
            AddAction(action);
    }
    public void RemoveAction(WarehouseAction action)
    {
        if (_actions == null)
            throw new ArgumentNullException(nameof(action));

        var kind = action.Kind == ActionKind.Outgo ? "O" : "I";
        var key = $"{kind}{action.Id}";
        _actions.Remove(key);
    }
    public void RemoveActions()
    {
        _actions.Clear();
    }
    public bool HasActions(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return true;
        pattern = pattern.Trim().ToUpper();
        foreach (var action in _actions.Values)
            if (action.PartnerActionId.ToUpper().Contains(pattern))
                return true;
        return false;
    }
    /// <summary>
    /// Идентификатор первого попавшегося партнёра (клиента, манданта).
    /// </summary>
    public string AnyPartnerId
    {
        get
        {
            var partners = GetPartners();
            return partners != null && partners.Length > 0 ? partners[0].Id : string.Empty;
        }
    }
    /// <summary>
    /// Название первого попавшегося партнёра (клиента, манданта).
    /// </summary>
    public string AnyPartnerName
    {
        get
        {
            var partners = GetPartners();
            return partners != null && partners.Length > 0 ? partners[0].Name : string.Empty;
        }
    }
    public Partner[] GetPartners()
    {
        var partners = new HashSet<Partner>();
        foreach (var action in _actions.Values)
        {
            if (!partners.Contains(action.Partner))
                partners.Add(action.Partner);
        }
        return partners.ToArray();
    }
    public Partner[] Partners
    {
        get
        {
            return GetPartners();
        }
    }
    /// <summary>
    /// Идентификаторы всех клиентов (партнёров, мандантов) операции через запятую.
    /// </summary>
    public string PartnersIds
    {
        get
        {
            var names = new StringBuilder();
            var partners = GetPartners();
            foreach (var partner in partners)
            {
                if (names.Length > 0)
                    names.Append(", ");
                names.Append(partner.Id);
            }
            return names.ToString();
        }
    }
    /// <summary>
    /// Названия всех клиентов (партнёров, мандантов) операции через запятую.
    /// </summary>
    public string PartnersNames
    {
        get
        {
            var names = new StringBuilder();
            var partners = GetPartners();
            foreach (var partner in partners)
            {
                if (names.Length > 0)
                    names.Append(", ");
                names.Append(partner.Name);
            }
            return names.ToString();
        }
    }
    /// <summary>
    /// Общее количество складских действ (приходов, расходов) в операции двора.
    /// </summary>
    public int ActionsCount
    {
        get
        {
            return _actions.Count;
        }
    }
    /// <summary>
    /// Список идентификаторов (из WMS) всех складских действ.
    /// </summary>
    public string ActionsIds
    {
        get
        {
            var ids = new StringBuilder();
            var actions = GetActions();
            foreach (var action in actions)
            {
                if (ids.Length > 0)
                    ids.Append(", ");
                ids.Append(action.Id);
            }
            return ids.ToString();
        }
    }
    /// <summary>
    /// Список клиентских (партнёрских) идентификаторов всех складских действ.
    /// </summary>
    public string PartnerActionsIds
    {
        get
        {
            var ids = new StringBuilder();
            var actions = GetActions();
            foreach (var action in actions)
            {
                if (ids.Length > 0)
                    ids.Append(", ");
                ids.Append(!string.IsNullOrWhiteSpace(action.PartnerActionId) ? action.PartnerActionId : "Б/Н");
            }
            return ids.ToString();
        }
    }
    /// <summary>
    /// Плановый вес НЕТТО.
    /// </summary>
    public double PlanNetWeight
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.PlanNetWeight;
            return result;
        }
    }
    /// <summary>
    /// Плановый вес БРУТТО.
    /// </summary>
    public double PlanGrossWeight
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.PlanGrossWeight;
            return result;
        }
    }
    /// <summary>
    /// Фактический вес НЕТТО.
    /// </summary>
    public double FactNetWeight
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.FactNetWeight;
            return result;
        }
    }
    /// <summary>
    /// Фактический вес БРУТТО.
    /// </summary>
    public double FactGrossWeight
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.FactGrossWeight;
            return result;
        }
    }
    /// <summary>
    /// Плановый объём.
    /// </summary>
    public double PlanVolume
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.PlanVolume;
            return result;
        }
    }
    /// <summary>
    /// Фактический объём.
    /// </summary>
    public double FactVolume
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.FactVolume;
            return result;
        }
    }
    /// <summary>
    /// Плановое колличество паллет.
    /// </summary>
    public double PlanPalletsCount
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.PlanPalletsCount;
            return result;
        }
    }
    /// <summary>
    /// Фактическое количество паллет.
    /// </summary>
    public double FactPalletsCount
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.FactPalletsCount;
            return result;
        }
    }
    /// <summary>
    /// Плановое количество коробок.
    /// </summary>
    public double PlanBoxesCount
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.PlanBoxeCount;
            return result;
        }
    }
    /// <summary>
    /// Фактическое количество коробок.
    /// </summary>
    public double FactBoxesCount
    {
        get
        {
            var result = 0.0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.FactBoxeCount;
            return result;
        }
    }
    /// <summary>
    /// Количество ТЕ в операции двора.
    /// </summary>
    public int TusCount
    {
        get
        {
            var result = 0;
            var actions = GetActions();
            foreach (var action in actions)
                result += action.TusCount;
            return result;
        }
    }
}
