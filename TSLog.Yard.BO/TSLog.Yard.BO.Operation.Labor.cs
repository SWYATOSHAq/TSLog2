using TSLog.Common.BO;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO;

/// <summary>
/// Операция двора (разгрузка или погрузка автомашины).
/// </summary>
public partial class Operation : Entity
{
    /// <summary>
    /// Возвращает трудозатраты (в человекосекундах) для конкретной должности.
    /// </summary>
    /// <param name="position">Должности.</param>
    /// <returns>Трудозатраты (в человекосекундах).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int GetLaborCost(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));
        var result = 0;
        foreach (var operationWork in _operationWorks)
            if (operationWork.Position == position)
                result += operationWork.WorkersCount * operationWork.Time;
        return result;
    }
    /// <summary>
    /// Возвращает трудозатраты (в человекосекундах) для конертной элементарной работы.
    /// </summary>
    /// <param name="work">Трудозатраты.</param>
    /// <returns>Трудозатраты (в человекосекундах).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int GetLaborCost(Work work)
    {
        if (work == null)
            throw new ArgumentNullException(nameof(work));
        var result = 0;
        foreach (var operationWork in _operationWorks)
            if (operationWork.Work == work)
                result += operationWork.WorkersCount * operationWork.Time;
        return result;
    }
    /// <summary>
    /// Трудозатраты по должностям (с человекосекундах).
    /// </summary>
    public IDictionary<Position, int> PositionsLaborCosts()
    {
        var result = new Dictionary<Position, int>();
        foreach (var operstionWork in _operationWorks)
        {
            var position = operstionWork.Position;
            var labor = operstionWork.WorkersCount * operstionWork.Time;
            if (!result.ContainsKey(position))
                result.Add(position, labor);
            else
                result[position] += labor;
        }
        return result;
    }
    /// <summary>
    /// Трудозатраты по элементарным работам (с человекосекундах).
    /// </summary>
    public IDictionary<Work, int> WorksLaborCosts()
    {
        var result = new Dictionary<Work, int>();
        foreach (var operstionWork in _operationWorks)
        {
            var work = operstionWork.Work;
            var labor = operstionWork.WorkersCount * operstionWork.Time;
            if (!result.ContainsKey(work))
                result.Add(work, labor);
            else
                result[work] += labor;
        }
        return result;
    }
    /// <summary>
    /// Общие ПЛАНОВЫЕ трудозатраты в человекосекундах.
    /// </summary>
    public int PlanLaborCost
    {
        get
        {
            var result = 0;
            foreach (var operationWork in _operationWorks)
                result += operationWork.Time * operationWork.WorkersCount;
            return result;
        }
    }
    /// <summary>
    /// Плановые трудозатраты на постоянные микроработы.
    /// </summary>
    public int PlanConstantLaborCost
    {
        get
        {
            var result = 0;
            foreach (var operationWork in _operationWorks)
            {
                if (!operationWork.Work.IsVariable)
                    result += operationWork.Time * operationWork.WorkersCount;
            }
            return result;
        }
    }
    /// <summary>
    /// Плановые трудозатраты на переменные микроработы.
    /// </summary>
    public int PlanVariableLaborCost
    {
        get
        {
            var result = 0;
            foreach (var operationWork in _operationWorks)
            {
                if (operationWork.Work.IsVariable)
                    result += operationWork.Time * operationWork.WorkersCount;
            }
            return result;
        }
    }
    /// <summary>
    /// Общие ПЛАНОВЫЕ трудозатраты в человекосекундах ТОЛЬКО "линейного" персонала.
    /// </summary>
    public int PlanLinearLaborCost
    {
        get
        {
            var result = 0;
            foreach (var operationWork in _operationWorks)
                result += operationWork.Position.IsLinear ? operationWork.Time * operationWork.WorkersCount : 0;
            return result;
        }
    }
    /// <summary>
    /// Общие ПЛАНОВЫЕ трудозатраты в человекосекундах ТОЛЬКО "линейного" персонала на постоянных микроработах.
    /// </summary>
    public int PlanLinearConstantLaborCost
    {
        get
        {
            var result = 0;
            foreach (var operationWork in _operationWorks)
            {
                if (!operationWork.Work.IsVariable)
                    result += operationWork.Position.IsLinear ? operationWork.Time * operationWork.WorkersCount : 0;
            }
                
            return result;
        }
    }
    /// <summary>
    /// Общие ПЛАНОВЫЕ трудозатраты в человекосекундах ТОЛЬКО "линейного" персонала на переменных микроработах.
    /// </summary>
    public int PlanLinearVariableLaborCost
    {
        get
        {
            var result = 0;
            foreach (var operationWork in _operationWorks)
            {
                if (operationWork.Work.IsVariable)
                    result += operationWork.Position.IsLinear ? operationWork.Time * operationWork.WorkersCount : 0;
            }

            return result;
        }
    }
    /// <summary>
    /// Общие ФАКТИЧЕСКИЕ трудозатраты в человекосекундах.
    /// </summary>
    public int FactLaborCost
    {
        get 
        {
            return FactWorkFinishDate != DateTime.MinValue && FactWorkStartDate != DateTime.MinValue ?
                (int)Math.Round((FactWorkFinishDate - FactWorkStartDate).TotalSeconds) * WorkersCount : 
                0;
        }
    }
}
