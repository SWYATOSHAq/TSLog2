using TSLog.Common.BO;
using TSLog.Staff.BO;

namespace TSLog.Yard.BO;

/// <summary>
/// Операция двора (разгрузка или погрузка автомашины).
/// </summary>
public partial class Operation : Entity
{
    /// <summary>
    /// Количество работников по определённой должности.
    /// </summary>
    /// <param name="position">Должность.</param>
    /// <returns>Уоличество работников.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public short GetWorkersCount(Position position)
    {
        ArgumentNullException.ThrowIfNull(position);
        var result = (short)0;
        foreach (var operationWork in _operationWorks)
            if (operationWork.Position == position && result < operationWork.WorkersCount)
                result = operationWork.WorkersCount;
        return result;
    }
    /// <summary>
    /// Количество работников, выполняющих определённой элементарную работу.
    /// </summary>
    /// <param name="work">Элементарная работа.</param>
    /// <returns>Количество работников.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public short GetWorkersCount(Work work)
    {
        ArgumentNullException.ThrowIfNull(work);
        var result = (short)0;
        foreach (var operationWork in _operationWorks)
            if (operationWork.Work == work && result < operationWork.WorkersCount)
                result = operationWork.WorkersCount;
        return result;
    }

    /// <summary>
    /// Общее количество работников.
    /// </summary>
    /// <returns>Количество работников.</returns>
    public short GetWorkersCount(bool showLinear = true)
    {
        var positionsWorkers = PositionsWorkersCount();
        var result = (short)0;
        foreach (var position in positionsWorkers.Keys)
            if (position.IsLinear == showLinear)
                result += positionsWorkers[position];
        return result;
    }

    /// <summary>
    /// Количество задействованных работников по должностям.
    /// </summary>
    /// <returns></returns>
    public IDictionary<Position, short> PositionsWorkersCount()
    {
        var result = new Dictionary<Position, short>();
        foreach (var operationWork in _operationWorks)
        {
            var position = operationWork.Position;
            var count = operationWork.WorkersCount;
            if (!result.TryGetValue(position, out short value))
                result.Add(position, count);
            else if (value < count)
                result[position] = count;
        }
        return result;
    }
    /// <summary>
    /// Количество задействованых работников по элементарным радотам.
    /// </summary>
    public IDictionary<Work, short> WorksWorkersCount()
    {
        var result = new Dictionary<Work, short>();
        foreach (var operationWork in _operationWorks)
        {
            var work = operationWork.Work;
            var count = operationWork.WorkersCount;
            if (!result.TryGetValue(work, out short value))
                result.Add(work, count);
            else if (value > count)
                result[work] = count;
        }
        return result;
    }
    /// <summary>
    /// Общее количество сотрудников для операции двора (ПРР) по всем должностям.
    /// </summary>
    public short TotalWorkersCount
    {
        get
        {
            var result = (short)0;
            var counts = new Dictionary<Position, short>();
            foreach (var operastionWork in _operationWorks)
            {
                var position = operastionWork.Position;
                var count = operastionWork.WorkersCount;
                if (!counts.TryAdd(position, count))
                    counts[position] += count;
            }
            foreach (var count in counts.Values)
                result += count;
            return result;
        }
    }
}
