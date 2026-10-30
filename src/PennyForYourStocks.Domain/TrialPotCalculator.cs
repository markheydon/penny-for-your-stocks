using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Domain;

/// <summary>Derives ring-fenced trial pot from opening amount, top-ups, withdrawals, and recorded costs.</summary>
public static class TrialPotCalculator
{
    public sealed record Input(
        decimal OpeningRingFencedAmount,
        IReadOnlyList<TrialTopUp> TopUps,
        IReadOnlyList<LogEntry> LogEntries,
        IReadOnlyList<CashObservation> CashObservations);

    public sealed record Result(
        decimal OpeningAmount,
        decimal TopUpsTotal,
        decimal CostsTotal,
        decimal WithdrawalsTotal,
        decimal DerivedPot);

    public static Result Calculate(Input input)
    {
        var topUpsTotal = input.TopUps.Sum(t => t.Amount);
        var costsTotal = input.LogEntries.Sum(LogEntryCostCalculator.GetEntryTotalCosts);
        var withdrawalsTotal = GetWithdrawalsTotal(input.LogEntries, input.CashObservations);
        var derivedPot = input.OpeningRingFencedAmount + topUpsTotal - withdrawalsTotal - costsTotal;
        return new Result(
            input.OpeningRingFencedAmount,
            topUpsTotal,
            costsTotal,
            withdrawalsTotal,
            derivedPot);
    }

    public static decimal GetWithdrawalsTotal(IReadOnlyList<LogEntry> logEntries, IReadOnlyList<CashObservation> cashObservations)
    {
        if (cashObservations.Count > 0)
        {
            return cashObservations.Sum(c => c.Amount);
        }

        return logEntries.Sum(e => e.WithdrawalAttributed ?? 0m);
    }

    public static bool UsesCashObservationWithdrawalPattern(IReadOnlyList<CashObservation> cashObservations) =>
        cashObservations.Count > 0;

    public static bool UsesEntryWithdrawalPattern(IReadOnlyList<LogEntry> logEntries) =>
        logEntries.Any(e => e.WithdrawalAttributed is not null and > 0m);
}
