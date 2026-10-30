using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Domain.Entities;
using PennyForYourStocks.Infrastructure.Persistence;

namespace PennyForYourStocks.Infrastructure.Tests;

public class TrialDataIsolationTests
{
    [Fact]
    public async Task QueryFilter_ReturnsOnlyCurrentUserRows()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = new DbContextOptionsBuilder<TrialDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var contextA = new TrialDbContext(options, new FixedUserContext("user-a"));
        await contextA.Database.OpenConnectionAsync(cancellationToken);
        await contextA.Database.EnsureCreatedAsync(cancellationToken);

        contextA.TrialConfigurations.Add(new TrialConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = "user-a",
            TrialStartDate = new DateOnly(2026, 1, 1),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        contextA.TrialConfigurations.Add(new TrialConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = "user-b",
            TrialStartDate = new DateOnly(2026, 1, 1),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await contextA.SaveChangesAsync(cancellationToken);

        var visible = await contextA.TrialConfigurations.ToListAsync(cancellationToken);
        Assert.Single(visible);
        Assert.Equal("user-a", visible[0].UserId);
    }

    private sealed class FixedUserContext(string userId) : ITrialUserContext
    {
        public string? UserId { get; } = userId;
    }
}
