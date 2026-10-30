namespace PennyForYourStocks.Domain.Entities;

public class MilestoneReview
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public ReviewType ReviewType { get; set; }

    public DateOnly DecisionDate { get; set; }

    public required string JudgementText { get; set; }

    public HopeStatus? HopeStatus { get; set; }

    public bool? FurtherCapitalInScope { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
