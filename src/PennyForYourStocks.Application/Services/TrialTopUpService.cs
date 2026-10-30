using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Services;

/// <summary>Records post-gate trial top-ups.</summary>
public sealed class TrialTopUpService(
    ITrialReadStore readStore,
    ITrialWriteStore writeStore)
{
    /// <summary>Records a top-up when the capital gate allows.</summary>
    public async Task RecordTopUpAsync(TopUpRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new TrialValidationException("Top-up amount must be greater than zero.");
        }

        var configuration = await readStore.GetConfigurationAsync(cancellationToken);
        if (!CapitalGatePolicy.IsTopUpAllowed(configuration))
        {
            throw new TrialPolicyException(CapitalGatePolicy.BlockedMessage, "capitalGate");
        }

        var topUp = new TrialTopUp
        {
            Id = Guid.NewGuid(),
            UserId = configuration!.UserId,
            EffectiveDate = request.EffectiveDate,
            Amount = request.Amount,
            Notes = request.Notes,
        };

        await writeStore.AddTopUpAsync(topUp, cancellationToken);
    }
}

/// <summary>Records standalone cash withdrawal observations.</summary>
public sealed class CashObservationService(
    ITrialReadStore readStore,
    ITrialWriteStore writeStore,
    ITrialUserContext userContext)
{
    /// <summary>Adds a cash observation when withdrawal pattern rules allow.</summary>
    public async Task RecordAsync(CashObservationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new TrialValidationException("Cash observation amount must be greater than zero.");
        }

        var entries = await readStore.ListLogEntriesAsync(null, null, cancellationToken);
        if (TrialPotCalculator.UsesEntryWithdrawalPattern(entries))
        {
            throw new TrialValidationException(
                "Withdrawals are already attributed on log entries. Reconcile before recording cash observations.");
        }

        var observation = new CashObservation
        {
            Id = Guid.NewGuid(),
            UserId = RequireUserId(),
            ObservedDate = request.ObservedDate,
            Amount = request.Amount,
            Notes = request.Notes,
        };

        await writeStore.AddCashObservationAsync(observation, cancellationToken);
    }

    private string RequireUserId() =>
        userContext.UserId ?? throw new InvalidOperationException("User must be authenticated.");
}
