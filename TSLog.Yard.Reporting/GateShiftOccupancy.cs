namespace TSLog.Yard.Reporting;

public static class GateShiftOccupancy
{
    // Минуты от начала смены. Обед и два десятиминутных перерыва
    // соответствуют расписанию из отчёта БК.
    private static readonly (int Start, int End)[] WorkingPeriods =
    [
        (0, 120),
        (130, 240),
        (300, 450),
        (460, 720)
    ];

    private static readonly int WorkingMinutes = WorkingPeriods.Sum(period => period.End - period.Start);

    public static double Calculate(IEnumerable<(double Start, double End)> factIntervals)
    {
        var intervals = factIntervals.OrderBy(interval => interval.Start).ToArray();
        double occupiedMinutes = 0;
        foreach (var period in WorkingPeriods)
        {
            double coveredUntil = period.Start;
            foreach (var interval in intervals)
            {
                if (interval.Start >= period.End)
                    break;

                var start = Math.Max(interval.Start, coveredUntil);
                var end = Math.Min(interval.End, period.End);
                if (end <= start)
                    continue;

                occupiedMinutes += end - start;
                coveredUntil = end;
            }
        }

        return occupiedMinutes * 100 / WorkingMinutes;
    }
}
