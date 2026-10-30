namespace PennyForYourStocks.Domain.Entities;

public class TrialTopUp
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}
