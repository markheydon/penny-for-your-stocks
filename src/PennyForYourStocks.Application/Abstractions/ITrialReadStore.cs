using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Abstractions;

/// <summary>Read access to persisted trial data for the current user.</summary>
public interface ITrialReadStore
{
    Task<TrialConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LogEntry>> ListLogEntriesAsync(DateOnly? from, DateOnly? toDate, CancellationToken cancellationToken = default);

    Task<LogEntry?> GetLogEntryByCheckDateAsync(DateOnly checkDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrialTopUp>> ListTopUpsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashObservation>> ListCashObservationsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MilestoneReview>> ListMilestoneReviewsAsync(CancellationToken cancellationToken = default);

    Task<MilestoneReview?> GetMilestoneReviewAsync(ReviewType reviewType, CancellationToken cancellationToken = default);
}
