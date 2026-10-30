using NSubstitute;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Application.Services;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Tests;

public class HopeComparisonTests
{
    [Fact]
    public void HopeComparisonSummary_BuildsMonthlyBucketsFromCashObservations()
    {
        var entries = Array.Empty<LogEntry>();
        var cash = new List<CashObservation>
        {
            new() { Id = Guid.NewGuid(), UserId = "u", ObservedDate = new DateOnly(2026, 1, 15), Amount = 120m },
            new() { Id = Guid.NewGuid(), UserId = "u", ObservedDate = new DateOnly(2026, 1, 20), Amount = 30m },
        };

        var result = HopeComparisonSummary.Calculate(
            entries,
            cash,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        Assert.Single(result.MonthlyBuckets);
        Assert.Equal(150m, result.MonthlyBuckets[0].TotalWithdrawn);
        Assert.Equal(150m, result.OverallAverageMonthlyWithdrawal);
    }

    [Fact]
    public void HopeComparisonSummary_AveragesOverElapsedMonthsIncludingZeros()
    {
        var entries = Array.Empty<LogEntry>();
        var cash = new List<CashObservation>
        {
            new() { Id = Guid.NewGuid(), UserId = "u", ObservedDate = new DateOnly(2026, 4, 15), Amount = 150m },
        };

        var result = HopeComparisonSummary.Calculate(
            entries,
            cash,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 4, 30));

        Assert.Equal(4, result.MonthlyBuckets.Count);
        Assert.Equal(37.50m, result.OverallAverageMonthlyWithdrawal);
    }

    [Fact]
    public async Task UpsertReview_UsesSingleRowPerReviewType()
    {
        var read = Substitute.For<ITrialReadStore>();
        var write = Substitute.For<ITrialWriteStore>();
        var user = Substitute.For<ITrialUserContext>();
        user.UserId.Returns("user-1");

        read.GetMilestoneReviewAsync(ReviewType.ShortTerm3Month, Arg.Any<CancellationToken>())
            .Returns((MilestoneReview?)null);
        read.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(new TrialConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            TrialStartDate = new DateOnly(2026, 1, 1),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var service = new MilestoneReviewService(read, write, user);
        await service.UpsertReviewAsync(
            new Application.Models.MilestoneReviewUpsertRequest(
                ReviewType.ShortTerm3Month,
                new DateOnly(2026, 4, 1),
                "Judgement",
                HopeStatus.Open,
                false,
                null),
            CancellationToken.None);

        await write.Received(1).UpsertMilestoneReviewAsync(
            Arg.Is<MilestoneReview>(r => r.ReviewType == ReviewType.ShortTerm3Month),
            Arg.Any<CancellationToken>());
    }
}
