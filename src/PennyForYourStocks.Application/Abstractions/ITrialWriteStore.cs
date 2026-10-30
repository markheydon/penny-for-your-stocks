using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Abstractions;

/// <summary>Write access to persisted trial data for the current user.</summary>
public interface ITrialWriteStore
{
    Task SaveConfigurationAsync(TrialConfiguration configuration, CancellationToken cancellationToken = default);

    Task UpsertLogEntryAsync(LogEntry entry, IReadOnlyList<LogEntryHolding> holdings, CancellationToken cancellationToken = default);

    Task AddTopUpAsync(TrialTopUp topUp, CancellationToken cancellationToken = default);

    Task AddCashObservationAsync(CashObservation observation, CancellationToken cancellationToken = default);

    Task UpsertMilestoneReviewAsync(MilestoneReview review, CancellationToken cancellationToken = default);
}
