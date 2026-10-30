using PennyForYourStocks.Domain;

namespace PennyForYourStocks.Domain.Tests;

public class MilestoneScheduleTests
{
    [Fact]
    public void GetDueDates_AddsCalendarMonthsFromStart()
    {
        var start = new DateOnly(2026, 1, 31);
        var due = MilestoneSchedule.GetDueDates(start);
        Assert.Equal(new DateOnly(2026, 4, 30), due.ShortTerm3Month);
        Assert.Equal(new DateOnly(2027, 1, 31), due.StopOrContinue12Month);
        Assert.Equal(new DateOnly(2027, 7, 31), due.FollowUp18Month);
    }
}
