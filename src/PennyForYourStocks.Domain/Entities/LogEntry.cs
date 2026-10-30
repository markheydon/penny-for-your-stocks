namespace PennyForYourStocks.Domain.Entities;

public class LogEntry
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public DateOnly CheckDate { get; set; }

    public required string Consideration { get; set; }

    public string? ComplianceNotes { get; set; }

    public bool PracticeModeAcknowledged { get; set; }

    public string? PracticeModeDisclaimer { get; set; }

    public decimal? AggregatedSpreadDealingCosts { get; set; }

    public decimal? StampDuty { get; set; }

    public decimal? ForeignExchangeFees { get; set; }

    public decimal? WithdrawalAttributed { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public ICollection<LogEntryHolding> Holdings { get; set; } = [];
}
