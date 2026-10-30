namespace PennyForYourStocks.Application;

/// <summary>Field length limits enforced at the API and application layers.</summary>
public static class TrialFieldLimits
{
    public const int Consideration = 8000;
    public const int ComplianceNotes = 4000;
    public const int PracticeModeDisclaimer = 2000;
    public const int IdentifierOrName = 500;
    public const int HoldingNotes = 2000;
    public const int JudgementText = 8000;
    public const int ExcludedThemesNotes = 4000;
    public const int NonTrialHoldingsNote = 4000;
    public const int TopUpNotes = 2000;
    public const int CashObservationNotes = 2000;
}
