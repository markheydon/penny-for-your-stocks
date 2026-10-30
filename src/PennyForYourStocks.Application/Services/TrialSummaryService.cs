using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Services;

/// <summary>Pot, hope, and composite summary read models.</summary>
public sealed class TrialSummaryService(ITrialReadStore readStore)
{
    private const string PotDisclaimer = "Cash and recorded costs only; not mark-to-market.";

    /// <summary>Returns derived pot figures when SC-005 is satisfied.</summary>
    public async Task<PotSummaryDto> GetPotSummaryAsync(CancellationToken cancellationToken = default)
    {
        var (configuration, entries, topUps, cashObservations) = await LoadAsync(cancellationToken);
        EnsureSummaryCompliance(entries);
        var pot = TrialPotCalculator.Calculate(new TrialPotCalculator.Input(
            configuration.OpeningRingFencedAmount,
            topUps,
            entries,
            cashObservations));

        return new PotSummaryDto(
            pot.OpeningAmount,
            pot.TopUpsTotal,
            pot.CostsTotal,
            pot.WithdrawalsTotal,
            pot.DerivedPot,
            PotDisclaimer,
            ComplianceSatisfied());
    }

    /// <summary>Returns hope comparison buckets when SC-005 is satisfied.</summary>
    public async Task<HopeSummaryDto> GetHopeSummaryAsync(CancellationToken cancellationToken = default)
    {
        var (configuration, entries, _, cashObservations) = await LoadAsync(cancellationToken);
        EnsureSummaryCompliance(entries);
        var periodEnd = ResolvePeriodEnd(entries, cashObservations);
        var hope = HopeComparisonSummary.Calculate(
            entries,
            cashObservations,
            configuration.TrialStartDate,
            periodEnd);
        return new HopeSummaryDto(
            hope.MonthlyBuckets.Select(b => new HopeMonthlyBucketDto(b.YearMonth, b.TotalWithdrawn)).ToList(),
            hope.OverallAverageMonthlyWithdrawal,
            hope.HopeBandLabel,
            hope.ObservationNote,
            ComplianceSatisfied());
    }

    /// <summary>Composite 12-month milestone summary (SC-002).</summary>
    public async Task<Milestone12SummaryDto> GetMilestone12SummaryAsync(CancellationToken cancellationToken = default)
    {
        var pot = await GetPotSummaryAsync(cancellationToken);
        var hope = await GetHopeSummaryAsync(cancellationToken);
        var review = await readStore.GetMilestoneReviewAsync(ReviewType.StopOrContinue12Month, cancellationToken);
        return new Milestone12SummaryDto(
            review?.JudgementText,
            pot.WithdrawalsTotal,
            pot,
            hope);
    }

    private async Task<(TrialConfiguration configuration, IReadOnlyList<LogEntry> entries, IReadOnlyList<TrialTopUp> topUps, IReadOnlyList<CashObservation> cash)> LoadAsync(
        CancellationToken cancellationToken)
    {
        var configuration = await readStore.GetConfigurationAsync(cancellationToken)
            ?? throw new TrialValidationException("Trial configuration is required.");
        var entries = await readStore.ListLogEntriesAsync(null, null, cancellationToken);
        var topUps = await readStore.ListTopUpsAsync(cancellationToken);
        var cash = await readStore.ListCashObservationsAsync(cancellationToken);
        return (configuration, entries, topUps, cash);
    }

    private static void EnsureSummaryCompliance(IReadOnlyList<LogEntry> entries)
    {
        if (!PracticeModePolicy.IsSummaryCompliant(entries))
        {
            throw new TrialPolicyException(PracticeModePolicy.SummaryComplianceGuidance, "practiceModeCompliance");
        }
    }

    private static PracticeModeComplianceDto ComplianceSatisfied() =>
        new(true, null);

    private static DateOnly ResolvePeriodEnd(
        IReadOnlyList<LogEntry> entries,
        IReadOnlyList<CashObservation> cashObservations)
    {
        var end = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var entry in entries)
        {
            if (entry.CheckDate > end)
            {
                end = entry.CheckDate;
            }
        }

        foreach (var observation in cashObservations)
        {
            if (observation.ObservedDate > end)
            {
                end = observation.ObservedDate;
            }
        }

        return end;
    }
}

/// <summary>Static guardrail copy for UI and API.</summary>
public static class GuardrailsService
{
    /// <summary>Returns read-only guardrail strings.</summary>
    public static GuardrailsDto GetGuardrails() =>
        new(
            TrialGuardrails.NoRecommendations,
            TrialGuardrails.PracticeModeWarning,
            TrialGuardrails.ProfitableStretchReminder,
            TrialGuardrails.CapitalBeforeThreeMonths);
}
