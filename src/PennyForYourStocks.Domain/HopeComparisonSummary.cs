using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Domain;

/// <summary>Compares observed monthly withdrawals to the static £100–£200 hope band (observation labels only).</summary>
public static class HopeComparisonSummary
{
    public const decimal HopeBandLower = 100m;
    public const decimal HopeBandUpper = 200m;

    public sealed record MonthlyBucket(string YearMonth, decimal TotalWithdrawn);

    public sealed record Result(
        IReadOnlyList<MonthlyBucket> MonthlyBuckets,
        decimal OverallAverageMonthlyWithdrawal,
        string HopeBandLabel,
        string ObservationNote);

    public static Result Calculate(
        IReadOnlyList<LogEntry> logEntries,
        IReadOnlyList<CashObservation> cashObservations,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        IReadOnlyList<(DateOnly Date, decimal Amount)> withdrawals;
        if (cashObservations.Count > 0)
        {
            withdrawals = cashObservations
                .Select(c => (c.ObservedDate, c.Amount))
                .ToList();
        }
        else
        {
            withdrawals = logEntries
                .Where(e => e.WithdrawalAttributed is > 0m)
                .Select(e => (e.CheckDate, e.WithdrawalAttributed!.Value))
                .ToList();
        }

        if (periodEnd < periodStart)
        {
            periodEnd = periodStart;
        }

        var totalsByMonth = withdrawals
            .GroupBy(w => YearMonthKey(w.Date.Year, w.Date.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var buckets = new List<MonthlyBucket>();
        var cursor = new DateOnly(periodStart.Year, periodStart.Month, 1);
        var endMonth = new DateOnly(periodEnd.Year, periodEnd.Month, 1);
        var totalWithdrawn = 0m;
        var monthCount = 0;
        while (cursor <= endMonth)
        {
            var key = YearMonthKey(cursor.Year, cursor.Month);
            var monthTotal = totalsByMonth.GetValueOrDefault(key);
            buckets.Add(new MonthlyBucket(key, monthTotal));
            totalWithdrawn += monthTotal;
            monthCount++;
            cursor = cursor.AddMonths(1);
        }

        var overallAverage = monthCount == 0
            ? 0m
            : totalWithdrawn / monthCount;

        return new Result(
            buckets,
            overallAverage,
            $"Hope band (not a target): £{HopeBandLower:0.00}–£{HopeBandUpper:0.00} per month",
            "Observed cash withdrawn only; not mark-to-market income.");
    }

    private static string YearMonthKey(int year, int month) => $"{year:D4}-{month:D2}";
}
