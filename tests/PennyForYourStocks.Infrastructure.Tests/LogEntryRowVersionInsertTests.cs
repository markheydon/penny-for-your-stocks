using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Domain.Entities;
using PennyForYourStocks.Infrastructure.Persistence;

namespace PennyForYourStocks.Infrastructure.Tests;

public class LogEntryRowVersionInsertTests
{
    [Fact]
    public async Task SaveChanges_NewLogEntry_PersistsRowVersion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = new DbContextOptionsBuilder<TrialDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var db = new TrialDbContext(options, new FixedUserContext("user-a"));
        await db.Database.OpenConnectionAsync(cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);

        db.LogEntries.Add(new LogEntry
        {
            Id = Guid.NewGuid(),
            UserId = "user-a",
            CheckDate = new DateOnly(2026, 10, 30),
            Consideration = "Weekly check",
        });

        await db.SaveChangesAsync(cancellationToken);

        var saved = await db.LogEntries.AsNoTracking().SingleAsync(cancellationToken);
        Assert.NotNull(saved.RowVersion);
        Assert.NotEmpty(saved.RowVersion);
    }

    private sealed class FixedUserContext(string userId) : ITrialUserContext
    {
        public string? UserId { get; } = userId;
    }
}
