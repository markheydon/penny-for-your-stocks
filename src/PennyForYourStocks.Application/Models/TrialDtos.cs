using PennyForYourStocks.Domain;

namespace PennyForYourStocks.Application.Models;

/// <summary>Trial configuration returned by read APIs.</summary>
public sealed record TrialConfigurationDto(
    DateOnly TrialStartDate,
    decimal OpeningRingFencedAmount,
    IReadOnlyList<TrialRegion> PermittedRegions,
    string? ExcludedThemesNotes,
    bool RealSharesOnly,
    bool NoLeverageOrDerivatives,
    int IntendedHoldDaysMin,
    int IntendedHoldDaysMax,
    string? NonTrialHoldingsNote,
    DateTimeOffset? ShortTermReviewCompletedAt);

/// <summary>Request to create or update trial configuration.</summary>
public sealed record TrialConfigurationUpsertRequest(
    DateOnly TrialStartDate,
    decimal OpeningRingFencedAmount,
    IReadOnlyList<TrialRegion> PermittedRegions,
    string? ExcludedThemesNotes,
    bool RealSharesOnly,
    bool NoLeverageOrDerivatives,
    int IntendedHoldDaysMin,
    int IntendedHoldDaysMax,
    string? NonTrialHoldingsNote);

public sealed record LogEntryHoldingDto(
    string IdentifierOrName,
    decimal? SpreadAndDealingCosts,
    string? Notes);

public sealed record LogEntrySummaryDto(
    DateOnly CheckDate,
    string Consideration,
    decimal TotalCosts,
    decimal? WithdrawalAttributed,
    bool PracticeModeWarning);

public sealed record LogEntryDetailDto(
    DateOnly CheckDate,
    string Consideration,
    string? ComplianceNotes,
    bool PracticeModeAcknowledged,
    string? PracticeModeDisclaimer,
    decimal? AggregatedSpreadDealingCosts,
    decimal? StampDuty,
    decimal? ForeignExchangeFees,
    decimal? WithdrawalAttributed,
    IReadOnlyList<LogEntryHoldingDto> Holdings,
    byte[] RowVersion,
    bool PracticeModeWarning);

public sealed record LogEntryUpsertRequest(
    string Consideration,
    string? ComplianceNotes,
    bool PracticeModeAcknowledged,
    string? PracticeModeDisclaimer,
    decimal? AggregatedSpreadDealingCosts,
    decimal? StampDuty,
    decimal? ForeignExchangeFees,
    decimal? WithdrawalAttributed,
    IReadOnlyList<LogEntryHoldingDto> Holdings,
    byte[]? RowVersion);

public sealed record PracticeModeWarningDto(bool ShouldWarn, string Message);

public sealed record PotSummaryDto(
    decimal OpeningAmount,
    decimal TopUpsTotal,
    decimal CostsTotal,
    decimal WithdrawalsTotal,
    decimal DerivedPot,
    string Disclaimer,
    PracticeModeComplianceDto PracticeModeCompliance);

public sealed record PracticeModeComplianceDto(bool Satisfied, string? Guidance);

public sealed record HopeSummaryDto(
    IReadOnlyList<HopeMonthlyBucketDto> MonthlyBuckets,
    decimal OverallAverageMonthlyWithdrawal,
    string HopeBandLabel,
    string ObservationNote,
    PracticeModeComplianceDto PracticeModeCompliance);

public sealed record HopeMonthlyBucketDto(string YearMonth, decimal TotalWithdrawn);

public sealed record MilestoneScheduleDto(
    DateOnly ShortTermDue,
    DateOnly TwelveMonthDue,
    DateOnly EighteenMonthDue,
    IReadOnlyList<MilestoneReviewStatusDto> Reviews);

public sealed record MilestoneReviewStatusDto(
    ReviewType ReviewType,
    bool Completed,
    DateOnly? DecisionDate);

public sealed record MilestoneReviewUpsertRequest(
    ReviewType ReviewType,
    DateOnly DecisionDate,
    string JudgementText,
    HopeStatus? HopeStatus,
    bool? FurtherCapitalInScope,
    byte[]? RowVersion);

public sealed record TopUpRequest(DateOnly EffectiveDate, decimal Amount, string? Notes);

public sealed record CashObservationRequest(DateOnly ObservedDate, decimal Amount, string? Notes);

public sealed record Milestone12SummaryDto(
    string? TwelveMonthJudgementText,
    decimal WithdrawalsTotal,
    PotSummaryDto Pot,
    HopeSummaryDto Hope);

public sealed record GuardrailsDto(
    string NoRecommendations,
    string PracticeModeWarning,
    string ProfitableStretchReminder,
    string CapitalBeforeThreeMonths);
