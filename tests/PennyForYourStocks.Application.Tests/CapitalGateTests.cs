using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Tests;

public class CapitalGateTests
{
    [Fact]
    public void IsTopUpAllowed_BlockedBeforeShortTermReview()
    {
        var configuration = new TrialConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = "u",
            TrialStartDate = new DateOnly(2026, 1, 1),
            ShortTermReviewCompletedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        Assert.False(CapitalGatePolicy.IsTopUpAllowed(configuration));
    }

    [Fact]
    public void IsTopUpAllowed_AllowedAfterShortTermReview()
    {
        var configuration = new TrialConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = "u",
            TrialStartDate = new DateOnly(2026, 1, 1),
            ShortTermReviewCompletedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        Assert.True(CapitalGatePolicy.IsTopUpAllowed(configuration));
    }
}
