namespace PennyForYourStocks.Domain.Entities;

public class CashObservation
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public DateOnly ObservedDate { get; set; }

    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}
