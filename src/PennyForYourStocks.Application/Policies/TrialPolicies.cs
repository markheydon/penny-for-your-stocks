using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Policies;

/// <summary>Static UK English guardrail copy (FR-003, FR-006, FR-010).</summary>
public static class TrialGuardrails
{
    public const string NoRecommendations =
        "This app records your own trial decisions. It does not recommend shares to buy or sell.";

    public const string PracticeModeWarning =
        "If you are using the log for official trial figures, record spread, dealing, stamp duty, and FX costs—or acknowledge practice mode limitations explicitly.";

    public const string ProfitableStretchReminder =
        "A profitable stretch is a reminder to review your stop rule, not a signal to add capital automatically.";

    public const string CapitalBeforeThreeMonths =
        "Adding ring-fenced capital before your short-term (about three-month) review is out of scope for this trial design.";
}

/// <summary>Practice-mode warnings and SC-005 summary compliance.</summary>
public static class PracticeModePolicy
{
    public static bool EntryRequiresPracticeWarning(LogEntry entry)
    {
        if (entry.PracticeModeAcknowledged)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(entry.PracticeModeDisclaimer))
        {
            return !LogEntryCostCalculator.HasRecordedCostAdjustments(entry);
        }

        return !LogEntryCostCalculator.HasRecordedCostAdjustments(entry)
            && entry.Holdings.Count > 0;
    }

    public static bool IsSummaryCompliant(IReadOnlyList<LogEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.PracticeModeAcknowledged)
            {
                continue;
            }

            var simulatedUse = !string.IsNullOrWhiteSpace(entry.PracticeModeDisclaimer);
            if (simulatedUse && !LogEntryCostCalculator.HasRecordedCostAdjustments(entry))
            {
                return false;
            }

            if (!simulatedUse && !LogEntryCostCalculator.HasRecordedCostAdjustments(entry))
            {
                return false;
            }
        }

        return true;
    }

    public static string SummaryComplianceGuidance { get; } =
        "Adjust recorded costs on log entries or acknowledge practice mode before viewing official trial summaries.";
}

/// <summary>Blocks top-ups until the short-term milestone review is recorded.</summary>
public static class CapitalGatePolicy
{
    public static bool IsTopUpAllowed(TrialConfiguration? configuration) =>
        configuration?.ShortTermReviewCompletedAt is not null;

    public static string BlockedMessage { get; } = TrialGuardrails.CapitalBeforeThreeMonths;
}
