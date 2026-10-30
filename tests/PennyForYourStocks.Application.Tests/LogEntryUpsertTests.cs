using NSubstitute;
using PennyForYourStocks.Application;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Application.Services;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Tests;

public class LogEntryUpsertTests
{
    [Fact]
    public async Task UpsertForCheckDate_UpdatesSameDayRow()
    {
        var read = Substitute.For<ITrialReadStore>();
        var write = Substitute.For<ITrialWriteStore>();
        var user = Substitute.For<ITrialUserContext>();
        user.UserId.Returns("user-1");

        var checkDate = new DateOnly(2026, 3, 1);
        var existing = new LogEntry
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            CheckDate = checkDate,
            Consideration = "Old",
            RowVersion = [1, 0, 0, 0],
        };

        read.GetLogEntryByCheckDateAsync(checkDate, Arg.Any<CancellationToken>()).Returns(existing, existing);
        read.ListCashObservationsAsync(Arg.Any<CancellationToken>()).Returns([]);
        read.ListLogEntriesAsync(null, null, Arg.Any<CancellationToken>()).Returns([existing]);

        LogEntry? saved = null;
        await write.UpsertLogEntryAsync(Arg.Do<LogEntry>(e => saved = e), Arg.Any<IReadOnlyList<LogEntryHolding>>(), Arg.Any<CancellationToken>());

        var service = new LogEntryService(read, write, user);
        await service.UpsertForCheckDateAsync(
            checkDate,
            new LogEntryUpsertRequest("Updated", null, false, null, null, null, null, null, [], existing.RowVersion),
            CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal("Updated", saved!.Consideration);
    }

    [Fact]
    public void PracticeModePolicy_WarnsWhenCostsMissingAndNotAcknowledged()
    {
        var entry = new LogEntry
        {
            Id = Guid.NewGuid(),
            UserId = "u",
            CheckDate = default,
            Consideration = "c",
            PracticeModeAcknowledged = false,
            Holdings = [new LogEntryHolding { Id = Guid.NewGuid(), IdentifierOrName = "X" }],
        };

        Assert.True(PracticeModePolicy.EntryRequiresPracticeWarning(entry));
    }
}
