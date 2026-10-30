using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Services;

/// <summary>Log entry queries and upserts for weekly trial checks.</summary>
public sealed class LogEntryService(
    ITrialReadStore readStore,
    ITrialWriteStore writeStore,
    ITrialUserContext userContext)
{
    /// <summary>Lists log entries in chronological order with optional date filters.</summary>
    public async Task<IReadOnlyList<LogEntrySummaryDto>> ListAsync(
        DateOnly? from,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var entries = await readStore.ListLogEntriesAsync(from, toDate, cancellationToken);
        return entries
            .OrderBy(e => e.CheckDate)
            .Select(MapSummary)
            .ToList();
    }

    /// <summary>Returns a single log entry for the given check date.</summary>
    public async Task<LogEntryDetailDto?> GetAsync(DateOnly checkDate, CancellationToken cancellationToken = default)
    {
        var entry = await readStore.GetLogEntryByCheckDateAsync(checkDate, cancellationToken);
        return entry is null ? null : MapDetail(entry);
    }

    /// <summary>Creates or updates the primary log entry for a calendar check date.</summary>
    public async Task<LogEntryDetailDto> UpsertForCheckDateAsync(
        DateOnly checkDate,
        LogEntryUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var userId = RequireUserId();
        var cashObservations = await readStore.ListCashObservationsAsync(cancellationToken);
        var existingEntries = await readStore.ListLogEntriesAsync(null, null, cancellationToken);

        if (request.WithdrawalAttributed is > 0m)
        {
            if (TrialPotCalculator.UsesCashObservationWithdrawalPattern(cashObservations))
            {
                throw new TrialValidationException(
                    "Withdrawals are already recorded as cash observations. Clear that pattern before attributing withdrawals on log entries.");
            }
        }

        var existing = await readStore.GetLogEntryByCheckDateAsync(checkDate, cancellationToken);
        if (existing is not null && request.RowVersion is not null
            && !existing.RowVersion.SequenceEqual(request.RowVersion))
        {
            throw new TrialConcurrencyException();
        }

        var holdings = MapHoldings(request.Holdings);
        var draft = BuildDraftEntry(existing, userId, checkDate, request, holdings);
        if (LogEntryCostCalculator.UsesMixedSpreadDealingPattern(draft))
        {
            throw new TrialValidationException(
                "Use either aggregated spread and dealing costs or per-holding lines, not both.");
        }

        await writeStore.UpsertLogEntryAsync(draft, holdings, cancellationToken);
        var saved = await readStore.GetLogEntryByCheckDateAsync(checkDate, cancellationToken)
            ?? throw new InvalidOperationException("Upserted log entry could not be loaded.");
        return MapDetail(saved);
    }

    /// <summary>Practice-mode warning for official trial use without cost adjustments.</summary>
    public static PracticeModeWarningDto GetPracticeModeWarning(LogEntryUpsertRequest request)
    {
        var draft = new LogEntry
        {
            Id = Guid.Empty,
            UserId = string.Empty,
            CheckDate = default,
            Consideration = request.Consideration,
            PracticeModeAcknowledged = request.PracticeModeAcknowledged,
            PracticeModeDisclaimer = request.PracticeModeDisclaimer,
            AggregatedSpreadDealingCosts = request.AggregatedSpreadDealingCosts,
            StampDuty = request.StampDuty,
            ForeignExchangeFees = request.ForeignExchangeFees,
            Holdings = MapHoldings(request.Holdings).ToList(),
        };

        var shouldWarn = PracticeModePolicy.EntryRequiresPracticeWarning(draft);
        return new PracticeModeWarningDto(shouldWarn, TrialGuardrails.PracticeModeWarning);
    }

    private static LogEntry BuildDraftEntry(
        LogEntry? existing,
        string userId,
        DateOnly checkDate,
        LogEntryUpsertRequest request,
        IReadOnlyList<LogEntryHolding> holdings)
    {
        var entry = existing ?? new LogEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CheckDate = checkDate,
            Consideration = request.Consideration,
        };

        entry.Consideration = request.Consideration;
        entry.ComplianceNotes = request.ComplianceNotes;
        entry.PracticeModeAcknowledged = request.PracticeModeAcknowledged;
        entry.PracticeModeDisclaimer = request.PracticeModeDisclaimer;
        entry.AggregatedSpreadDealingCosts = request.AggregatedSpreadDealingCosts;
        entry.StampDuty = request.StampDuty;
        entry.ForeignExchangeFees = request.ForeignExchangeFees;
        entry.WithdrawalAttributed = request.WithdrawalAttributed;
        entry.Holdings = holdings.ToList();
        if (existing is not null && request.RowVersion is not null)
        {
            entry.RowVersion = request.RowVersion;
        }

        return entry;
    }

    private static void ValidateRequest(LogEntryUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Consideration))
        {
            throw new TrialValidationException("Consideration is required.");
        }

        if (request.Consideration.Length > TrialFieldLimits.Consideration)
        {
            throw new TrialValidationException($"Consideration cannot exceed {TrialFieldLimits.Consideration} characters.");
        }
    }

    private static List<LogEntryHolding> MapHoldings(IReadOnlyList<LogEntryHoldingDto> holdings) =>
        holdings.Select(h => new LogEntryHolding
        {
            Id = Guid.NewGuid(),
            IdentifierOrName = h.IdentifierOrName,
            SpreadAndDealingCosts = h.SpreadAndDealingCosts,
            Notes = h.Notes,
        }).ToList();

    private static LogEntrySummaryDto MapSummary(LogEntry entry) =>
        new(
            entry.CheckDate,
            entry.Consideration,
            LogEntryCostCalculator.GetEntryTotalCosts(entry),
            entry.WithdrawalAttributed,
            PracticeModePolicy.EntryRequiresPracticeWarning(entry));

    private static LogEntryDetailDto MapDetail(LogEntry entry) =>
        new(
            entry.CheckDate,
            entry.Consideration,
            entry.ComplianceNotes,
            entry.PracticeModeAcknowledged,
            entry.PracticeModeDisclaimer,
            entry.AggregatedSpreadDealingCosts,
            entry.StampDuty,
            entry.ForeignExchangeFees,
            entry.WithdrawalAttributed,
            entry.Holdings.Select(h => new LogEntryHoldingDto(h.IdentifierOrName, h.SpreadAndDealingCosts, h.Notes)).ToList(),
            entry.RowVersion,
            PracticeModePolicy.EntryRequiresPracticeWarning(entry));

    private string RequireUserId() =>
        userContext.UserId ?? throw new InvalidOperationException("User must be authenticated.");
}
