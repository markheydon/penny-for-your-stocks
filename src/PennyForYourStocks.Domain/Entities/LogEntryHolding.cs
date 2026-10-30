namespace PennyForYourStocks.Domain.Entities;

public class LogEntryHolding
{
    public Guid Id { get; set; }

    public Guid LogEntryId { get; set; }

    public LogEntry? LogEntry { get; set; }

    public required string IdentifierOrName { get; set; }

    public decimal? SpreadAndDealingCosts { get; set; }

    public string? Notes { get; set; }
}
