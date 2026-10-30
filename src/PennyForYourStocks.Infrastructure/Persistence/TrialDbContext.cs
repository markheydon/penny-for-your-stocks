using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Domain.Entities;
using PennyForYourStocks.Infrastructure.Identity;

namespace PennyForYourStocks.Infrastructure.Persistence;

public class TrialDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly string? _userId;

    protected override Version SchemaVersion => IdentitySchemaVersions.Version3;

    public TrialDbContext(DbContextOptions<TrialDbContext> options, ITrialUserContext userContext)
        : base(options)
    {
        _userId = userContext.UserId;
    }

    public DbSet<TrialConfiguration> TrialConfigurations => Set<TrialConfiguration>();

    public DbSet<LogEntry> LogEntries => Set<LogEntry>();

    public DbSet<LogEntryHolding> LogEntryHoldings => Set<LogEntryHolding>();

    public DbSet<TrialTopUp> TrialTopUps => Set<TrialTopUp>();

    public DbSet<CashObservation> CashObservations => Set<CashObservation>();

    public DbSet<MilestoneReview> MilestoneReviews => Set<MilestoneReview>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TrialConfiguration>(entity =>
        {
            entity.Property(e => e.OpeningRingFencedAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasQueryFilter(e => _userId != null && e.UserId == _userId);
        });

        builder.Entity<LogEntry>(entity =>
        {
            entity.Property(e => e.AggregatedSpreadDealingCosts).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StampDuty).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ForeignExchangeFees).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WithdrawalAttributed).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken()
                .IsRequired();
            entity.HasIndex(e => new { e.UserId, e.CheckDate }).IsUnique();
            entity.HasMany(e => e.Holdings).WithOne(h => h.LogEntry).HasForeignKey(h => h.LogEntryId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => _userId != null && e.UserId == _userId);
        });

        builder.Entity<LogEntryHolding>(entity =>
        {
            entity.Property(e => e.SpreadAndDealingCosts).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(h => _userId != null && h.LogEntry!.UserId == _userId);
        });

        builder.Entity<TrialTopUp>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(e => _userId != null && e.UserId == _userId);
        });

        builder.Entity<CashObservation>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(e => _userId != null && e.UserId == _userId);
        });

        builder.Entity<MilestoneReview>(entity =>
        {
            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken()
                .IsRequired();
            entity.HasIndex(e => new { e.UserId, e.ReviewType }).IsUnique();
            entity.HasQueryFilter(e => _userId != null && e.UserId == _userId);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySqliteRowVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySqliteRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplySqliteRowVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplySqliteRowVersions()
    {
        foreach (var entry in ChangeTracker.Entries<LogEntry>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Property(e => e.RowVersion).CurrentValue = CreateRowVersionToken();
            }
        }

        foreach (var entry in ChangeTracker.Entries<MilestoneReview>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Property(r => r.RowVersion).CurrentValue = CreateRowVersionToken();
            }
        }
    }

    private static byte[] CreateRowVersionToken() => Guid.NewGuid().ToByteArray();
}
