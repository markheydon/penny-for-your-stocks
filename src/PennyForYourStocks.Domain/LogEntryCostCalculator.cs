using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Domain;

/// <summary>Computes per-entry costs and trial pot from stored cash figures only.</summary>
public static class LogEntryCostCalculator
{
    public static bool UsesMixedSpreadDealingPattern(LogEntry entry)
    {
        var hasRollup = entry.AggregatedSpreadDealingCosts is not null;
        var hasLineCosts = entry.Holdings.Any(h => h.SpreadAndDealingCosts is not null);
        return hasRollup && hasLineCosts;
    }

    public static decimal GetEntryTotalCosts(LogEntry entry)
    {
        if (UsesMixedSpreadDealingPattern(entry))
        {
            throw new InvalidOperationException("Spread and dealing costs cannot use both rollup and per-holding lines.");
        }

        decimal spreadDealing;
        if (entry.AggregatedSpreadDealingCosts is { } rollup)
        {
            spreadDealing = rollup;
        }
        else
        {
            spreadDealing = entry.Holdings.Sum(h => h.SpreadAndDealingCosts ?? 0m);
        }

        return spreadDealing + (entry.StampDuty ?? 0m) + (entry.ForeignExchangeFees ?? 0m);
    }

    public static bool HasRecordedCostAdjustments(LogEntry entry)
    {
        if (entry.AggregatedSpreadDealingCosts is > 0m)
        {
            return true;
        }

        return entry.Holdings.Any(h => h.SpreadAndDealingCosts is > 0m)
            || entry.StampDuty is > 0m
            || entry.ForeignExchangeFees is > 0m;
    }
}
