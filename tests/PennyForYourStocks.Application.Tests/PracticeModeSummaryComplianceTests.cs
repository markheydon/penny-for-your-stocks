using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Tests;

public class PracticeModeSummaryComplianceTests
{
    [Fact]
    public void IsSummaryCompliant_FalseWhenCostsMissingWithoutAcknowledgement()
    {
        var entries = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "u",
                CheckDate = new DateOnly(2026, 1, 1),
                Consideration = "Weekly check",
                PracticeModeAcknowledged = false,
                Holdings = [new LogEntryHolding { Id = Guid.NewGuid(), IdentifierOrName = "Example plc" }],
            },
        };

        Assert.False(PracticeModePolicy.IsSummaryCompliant(entries));
    }

    [Fact]
    public void IsSummaryCompliant_TrueWhenPracticeModeAcknowledged()
    {
        var entries = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "u",
                CheckDate = new DateOnly(2026, 1, 1),
                Consideration = "Weekly check",
                PracticeModeAcknowledged = true,
            },
        };

        Assert.True(PracticeModePolicy.IsSummaryCompliant(entries));
    }

    [Fact]
    public void IsSummaryCompliant_TrueForNoTradeEntryWithoutHoldings()
    {
        var entries = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "u",
                CheckDate = new DateOnly(2026, 1, 1),
                Consideration = "No trades this week",
                PracticeModeAcknowledged = false,
                Holdings = [],
            },
        };

        Assert.True(PracticeModePolicy.IsSummaryCompliant(entries));
    }
}
