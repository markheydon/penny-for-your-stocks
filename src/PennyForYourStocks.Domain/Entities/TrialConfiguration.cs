namespace PennyForYourStocks.Domain.Entities;

public class TrialConfiguration
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public DateOnly TrialStartDate { get; set; }

    public decimal OpeningRingFencedAmount { get; set; } = 50.00m;

    /// <summary>Comma-separated <see cref="TrialRegion"/> values.</summary>
    public string PermittedRegions { get; set; } = "UK";

    public string? ExcludedThemesNotes { get; set; }

    public bool RealSharesOnly { get; set; } = true;

    public bool NoLeverageOrDerivatives { get; set; } = true;

    public int IntendedHoldDaysMin { get; set; } = 2;

    public int IntendedHoldDaysMax { get; set; } = 7;

    public string? NonTrialHoldingsNote { get; set; }

    public DateTimeOffset? ShortTermReviewCompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
