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

    public static Result Calculate(IReadOnlyList<LogEntry> logEntries, IReadOnlyList<CashObservation> cashObservations)
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

        var buckets = withdrawals
            .GroupBy(w => $"{w.Date.Year:D4}-{w.Date.Month:D2}")
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyBucket(g.Key, g.Sum(x => x.Amount)))
            .ToList();

        var overallAverage = buckets.Count == 0
            ? 0m
            : buckets.Average(b => b.TotalWithdrawn);

        return new Result(
            buckets,
            overallAverage,
            $"Hope band (not a target): £{HopeBandLower:0.00}–£{HopeBandUpper:0.00} per month",
            "Observed cash withdrawn only; not mark-to-market income.");
    }
}
