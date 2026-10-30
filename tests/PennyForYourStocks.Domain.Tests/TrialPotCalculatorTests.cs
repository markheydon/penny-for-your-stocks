using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Domain.Tests;

public class TrialPotCalculatorTests
{
    [Fact]
    public void Calculate_WithdrawalsFromCashObservations_WhenObservationsExist()
    {
        var entries = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "u",
                CheckDate = new DateOnly(2026, 1, 1),
                Consideration = "Check",
                WithdrawalAttributed = 25m,
            },
        };
        var cash = new List<CashObservation>
        {
            new() { Id = Guid.NewGuid(), UserId = "u", ObservedDate = new DateOnly(2026, 2, 1), Amount = 40m },
        };

        var result = TrialPotCalculator.Calculate(new TrialPotCalculator.Input(50m, [], entries, cash));
        Assert.Equal(40m, result.WithdrawalsTotal);
        Assert.Equal(10m, result.DerivedPot);
    }

    [Fact]
    public void Calculate_WithdrawalsFromEntries_WhenNoCashObservations()
    {
        var entries = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = "u",
                CheckDate = new DateOnly(2026, 1, 1),
                Consideration = "Check",
                WithdrawalAttributed = 15m,
            },
        };

        var result = TrialPotCalculator.Calculate(new TrialPotCalculator.Input(50m, [], entries, []));
        Assert.Equal(15m, result.WithdrawalsTotal);
    }

    [Fact]
    public void GetEntryTotalCosts_RollupOrHoldingLines_AreMutuallyExclusive()
    {
        var entry = new LogEntry
        {
            Id = Guid.NewGuid(),
            UserId = "u",
            CheckDate = default,
            Consideration = "c",
            AggregatedSpreadDealingCosts = 5m,
            Holdings =
            [
                new LogEntryHolding { Id = Guid.NewGuid(), IdentifierOrName = "A", SpreadAndDealingCosts = 2m },
            ],
        };

        Assert.True(LogEntryCostCalculator.UsesMixedSpreadDealingPattern(entry));
        Assert.Throws<InvalidOperationException>(() => LogEntryCostCalculator.GetEntryTotalCosts(entry));
    }
}
