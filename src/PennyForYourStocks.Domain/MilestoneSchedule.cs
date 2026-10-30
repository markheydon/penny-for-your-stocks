namespace PennyForYourStocks.Domain;

/// <summary>Computed milestone due dates from the trial start date.</summary>
public static class MilestoneSchedule
{
    public sealed record DueDates(DateOnly ShortTerm3Month, DateOnly StopOrContinue12Month, DateOnly FollowUp18Month);

    public static DueDates GetDueDates(DateOnly trialStartDate) =>
        new(
            trialStartDate.AddMonths(3),
            trialStartDate.AddMonths(12),
            trialStartDate.AddMonths(18));
}
