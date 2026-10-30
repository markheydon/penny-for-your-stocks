using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Application;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;
using PennyForYourStocks.Infrastructure.Persistence;

namespace PennyForYourStocks.Infrastructure.Stores;

internal sealed class EfTrialReadStore(TrialDbContext db) : ITrialReadStore
{
    public Task<TrialConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default) =>
        db.TrialConfigurations.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<LogEntry>> ListLogEntriesAsync(DateOnly? from, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var query = db.LogEntries.AsNoTracking().Include(e => e.Holdings).AsQueryable();
        if (from is not null)
        {
            query = query.Where(e => e.CheckDate >= from);
        }

        if (toDate is not null)
        {
            query = query.Where(e => e.CheckDate <= toDate);
        }

        return await query.OrderBy(e => e.CheckDate).ToListAsync(cancellationToken);
    }

    public Task<LogEntry?> GetLogEntryByCheckDateAsync(DateOnly checkDate, CancellationToken cancellationToken = default) =>
        db.LogEntries.AsNoTracking().Include(e => e.Holdings)
            .FirstOrDefaultAsync(e => e.CheckDate == checkDate, cancellationToken);

    public async Task<IReadOnlyList<TrialTopUp>> ListTopUpsAsync(CancellationToken cancellationToken = default) =>
        await db.TrialTopUps.AsNoTracking().OrderBy(t => t.EffectiveDate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CashObservation>> ListCashObservationsAsync(CancellationToken cancellationToken = default) =>
        await db.CashObservations.AsNoTracking().OrderBy(c => c.ObservedDate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MilestoneReview>> ListMilestoneReviewsAsync(CancellationToken cancellationToken = default) =>
        await db.MilestoneReviews.AsNoTracking().ToListAsync(cancellationToken);

    public Task<MilestoneReview?> GetMilestoneReviewAsync(ReviewType reviewType, CancellationToken cancellationToken = default) =>
        db.MilestoneReviews.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewType == reviewType, cancellationToken);
}

internal sealed class EfTrialWriteStore(TrialDbContext db) : ITrialWriteStore
{
    public async Task SaveConfigurationAsync(TrialConfiguration configuration, CancellationToken cancellationToken = default)
    {
        db.ChangeTracker.Clear();
        var tracked = await db.TrialConfigurations
            .FirstOrDefaultAsync(c => c.UserId == configuration.UserId, cancellationToken);
        if (tracked is null)
        {
            db.TrialConfigurations.Add(configuration);
        }
        else
        {
            db.Entry(tracked).CurrentValues.SetValues(configuration);
            tracked.ShortTermReviewCompletedAt = configuration.ShortTermReviewCompletedAt;
            tracked.UpdatedAt = configuration.UpdatedAt;
        }

        await SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertLogEntryAsync(LogEntry entry, IReadOnlyList<LogEntryHolding> holdings, CancellationToken cancellationToken = default)
    {
        db.ChangeTracker.Clear();
        var tracked = await db.LogEntries.AsNoTracking().Include(e => e.Holdings)
            .FirstOrDefaultAsync(e => e.CheckDate == entry.CheckDate, cancellationToken);

        if (tracked is null)
        {
            foreach (var holding in holdings)
            {
                holding.LogEntryId = entry.Id;
            }

            entry.Holdings = holdings.ToList();
            db.LogEntries.Add(entry);
        }
        else
        {
            db.LogEntries.Attach(tracked);
            if (entry.RowVersion is { Length: > 0 })
            {
                db.Entry(tracked).Property(e => e.RowVersion).OriginalValue = entry.RowVersion;
            }

            tracked.Consideration = entry.Consideration;
            tracked.ComplianceNotes = entry.ComplianceNotes;
            tracked.PracticeModeAcknowledged = entry.PracticeModeAcknowledged;
            tracked.PracticeModeDisclaimer = entry.PracticeModeDisclaimer;
            tracked.AggregatedSpreadDealingCosts = entry.AggregatedSpreadDealingCosts;
            tracked.StampDuty = entry.StampDuty;
            tracked.ForeignExchangeFees = entry.ForeignExchangeFees;
            tracked.WithdrawalAttributed = entry.WithdrawalAttributed;

            db.LogEntryHoldings.RemoveRange(tracked.Holdings);
            foreach (var holding in holdings)
            {
                holding.Id = Guid.NewGuid();
                holding.LogEntryId = tracked.Id;
                db.LogEntryHoldings.Add(holding);
            }

            db.Entry(tracked).Property(e => e.RowVersion).IsModified = true;
        }

        await SaveChangesAsync(cancellationToken);
    }

    public async Task AddTopUpAsync(TrialTopUp topUp, CancellationToken cancellationToken = default)
    {
        db.ChangeTracker.Clear();
        db.TrialTopUps.Add(topUp);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task AddCashObservationAsync(CashObservation observation, CancellationToken cancellationToken = default)
    {
        db.ChangeTracker.Clear();
        db.CashObservations.Add(observation);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertMilestoneReviewAsync(MilestoneReview review, CancellationToken cancellationToken = default)
    {
        db.ChangeTracker.Clear();
        var tracked = await db.MilestoneReviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReviewType == review.ReviewType, cancellationToken);
        if (tracked is null)
        {
            db.MilestoneReviews.Add(review);
        }
        else
        {
            db.MilestoneReviews.Attach(tracked);
            if (review.RowVersion is { Length: > 0 })
            {
                db.Entry(tracked).Property(r => r.RowVersion).OriginalValue = review.RowVersion;
            }

            tracked.DecisionDate = review.DecisionDate;
            tracked.JudgementText = review.JudgementText;
            tracked.HopeStatus = review.HopeStatus;
            tracked.FurtherCapitalInScope = review.FurtherCapitalInScope;
            db.Entry(tracked).Property(r => r.RowVersion).IsModified = true;
        }

        await SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            throw new TrialConcurrencyException();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            db.ChangeTracker.Clear();
            throw new TrialConcurrencyException();
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true;
}
