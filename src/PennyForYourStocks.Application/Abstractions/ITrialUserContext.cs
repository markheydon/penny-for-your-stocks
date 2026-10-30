namespace PennyForYourStocks.Application.Abstractions;

/// <summary>Provides the authenticated user's identifier for trial data access.</summary>
public interface ITrialUserContext
{
    /// <summary>Current user's Identity id, or null when unauthenticated.</summary>
    string? UserId { get; }
}
