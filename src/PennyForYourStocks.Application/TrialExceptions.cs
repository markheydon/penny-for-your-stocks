namespace PennyForYourStocks.Application;

/// <summary>Validation or policy failure surfaced to API callers.</summary>
public sealed class TrialValidationException : Exception
{
    public TrialValidationException(string message)
        : base(message)
    {
    }

    public TrialValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}

/// <summary>Optimistic concurrency conflict.</summary>
public sealed class TrialConcurrencyException : Exception
{
    public TrialConcurrencyException()
        : base("The record was updated on another device. Reload and reconcile before saving.")
    {
    }
}

/// <summary>Capital gate or practice-mode summary compliance failure.</summary>
public sealed class TrialPolicyException : Exception
{
    public TrialPolicyException(string message, string? policyCode = null)
        : base(message)
    {
        PolicyCode = policyCode;
    }

    public string? PolicyCode { get; }
}
