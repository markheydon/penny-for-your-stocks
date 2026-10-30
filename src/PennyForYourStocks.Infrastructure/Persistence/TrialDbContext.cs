using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Domain.Entities;
using PennyForYourStocks.Infrastructure.Identity;

namespace PennyForYourStocks.Infrastructure.Persistence;

public class TrialDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly string? _userId;

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
            entity.HasQueryFilter(e => _userId == null || e.UserId == _userId);
        });

        builder.Entity<LogEntry>(entity =>
        {
            entity.Property(e => e.AggregatedSpreadDealingCosts).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StampDuty).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ForeignExchangeFees).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WithdrawalAttributed).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => new { e.UserId, e.CheckDate }).IsUnique();
            entity.HasMany(e => e.Holdings).WithOne(h => h.LogEntry).HasForeignKey(h => h.LogEntryId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => _userId == null || e.UserId == _userId);
        });

        builder.Entity<LogEntryHolding>(entity =>
        {
            entity.Property(e => e.SpreadAndDealingCosts).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(h => _userId == null || h.LogEntry!.UserId == _userId);
        });

        builder.Entity<TrialTopUp>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(e => _userId == null || e.UserId == _userId);
        });

        builder.Entity<CashObservation>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(e => _userId == null || e.UserId == _userId);
        });

        builder.Entity<MilestoneReview>(entity =>
        {
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => new { e.UserId, e.ReviewType }).IsUnique();
            entity.HasQueryFilter(e => _userId == null || e.UserId == _userId);
        });
    }
}
