using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Domain.Entities;
using PennyForYourStocks.Infrastructure.Persistence;
using PennyForYourStocks.Infrastructure.Stores;

namespace PennyForYourStocks.Infrastructure.Tests;

public class EfTrialWriteStoreTests
{
    [Fact]
    public async Task UpsertLogEntry_SecondContextSeesPriorSave()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        var options = new DbContextOptionsBuilder<TrialDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new TrialDbContext(options, new FixedUserContext("user-a")))
        {
            await setup.Database.EnsureCreatedAsync(cancellationToken);
        }

        var checkDate = new DateOnly(2026, 3, 1);
        await using (var contextA = new TrialDbContext(options, new FixedUserContext("user-a")))
        {
            var store = new EfTrialWriteStore(contextA);
            await store.UpsertLogEntryAsync(
                new LogEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = "user-a",
                    CheckDate = checkDate,
                    Consideration = "foo",
                },
                [],
                cancellationToken);
        }

        await using (var contextB = new TrialDbContext(options, new FixedUserContext("user-a")))
        {
            var store = new EfTrialWriteStore(contextB);
            var loaded = await contextB.LogEntries.AsNoTracking().SingleAsync(cancellationToken);
            await store.UpsertLogEntryAsync(
                new LogEntry
                {
                    Id = loaded.Id,
                    UserId = "user-a",
                    CheckDate = checkDate,
                    Consideration = "bar",
                    RowVersion = loaded.RowVersion,
                },
                [],
                cancellationToken);
        }

        await using var verify = new TrialDbContext(options, new FixedUserContext("user-a"));
        var final = await verify.LogEntries.AsNoTracking().SingleAsync(cancellationToken);
        Assert.Equal("bar", final.Consideration);
    }

    [Fact]
    public async Task UpsertLogEntry_HoldingsOnlyEdit_BumpsRowVersion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        var options = new DbContextOptionsBuilder<TrialDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new TrialDbContext(options, new FixedUserContext("user-a")))
        {
            await setup.Database.EnsureCreatedAsync(cancellationToken);
        }

        var checkDate = new DateOnly(2026, 3, 1);
        byte[] originalVersion;
        Guid entryId;

        await using (var context = new TrialDbContext(options, new FixedUserContext("user-a")))
        {
            var store = new EfTrialWriteStore(context);
            entryId = Guid.NewGuid();
            await store.UpsertLogEntryAsync(
                new LogEntry
                {
                    Id = entryId,
                    UserId = "user-a",
                    CheckDate = checkDate,
                    Consideration = "Weekly",
                },
                [],
                cancellationToken);
            originalVersion = (await context.LogEntries.AsNoTracking().SingleAsync(cancellationToken)).RowVersion;
        }

        await using (var context = new TrialDbContext(options, new FixedUserContext("user-a")))
        {
            var store = new EfTrialWriteStore(context);
            var loaded = await context.LogEntries.AsNoTracking().Include(e => e.Holdings).SingleAsync(cancellationToken);
            await store.UpsertLogEntryAsync(
                new LogEntry
                {
                    Id = entryId,
                    UserId = "user-a",
                    CheckDate = checkDate,
                    Consideration = "Weekly",
                    RowVersion = loaded.RowVersion,
                },
                [new LogEntryHolding { Id = Guid.NewGuid(), IdentifierOrName = "Example plc" }],
                cancellationToken);
        }

        await using var verify = new TrialDbContext(options, new FixedUserContext("user-a"));
        var final = await verify.LogEntries.AsNoTracking().SingleAsync(cancellationToken);
        Assert.False(final.RowVersion.SequenceEqual(originalVersion));
    }

    private sealed class FixedUserContext(string userId) : ITrialUserContext
    {
        public string? UserId { get; } = userId;
    }
}
