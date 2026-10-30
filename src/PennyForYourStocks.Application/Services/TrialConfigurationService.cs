using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Services;

/// <summary>Coordinates trial configuration reads and updates.</summary>
public sealed class TrialConfigurationService(
    ITrialReadStore readStore,
    ITrialWriteStore writeStore,
    ITrialUserContext userContext)
{
    /// <summary>Returns the current user's configuration, if onboarded.</summary>
    public Task<TrialConfiguration?> GetEntityAsync(CancellationToken cancellationToken = default) =>
        readStore.GetConfigurationAsync(cancellationToken);

    /// <summary>Maps persisted configuration to a DTO.</summary>
    public async Task<TrialConfigurationDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var entity = await readStore.GetConfigurationAsync(cancellationToken);
        return entity is null ? null : Map(entity);
    }

    /// <summary>Creates or updates trial configuration for the current user.</summary>
    public async Task<TrialConfigurationDto> UpsertAsync(
        TrialConfigurationUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var userId = RequireUserId();
        var existing = await readStore.GetConfigurationAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = existing ?? new TrialConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = now,
        };

        entity.TrialStartDate = request.TrialStartDate;
        entity.OpeningRingFencedAmount = request.OpeningRingFencedAmount;
        entity.PermittedRegions = TrialRegionParser.SerializePermittedRegions(request.PermittedRegions);
        entity.ExcludedThemesNotes = request.ExcludedThemesNotes;
        entity.RealSharesOnly = request.RealSharesOnly;
        entity.NoLeverageOrDerivatives = request.NoLeverageOrDerivatives;
        entity.IntendedHoldDaysMin = request.IntendedHoldDaysMin;
        entity.IntendedHoldDaysMax = request.IntendedHoldDaysMax;
        entity.NonTrialHoldingsNote = request.NonTrialHoldingsNote;
        entity.UpdatedAt = now;

        await writeStore.SaveConfigurationAsync(entity, cancellationToken);
        return Map(entity);
    }

    /// <summary>Minimal onboarding defaults for MVP logging.</summary>
    public async Task<TrialConfigurationDto> EnsureMinimalOnboardingAsync(
        DateOnly? trialStartDate,
        CancellationToken cancellationToken = default)
    {
        var existing = await readStore.GetConfigurationAsync(cancellationToken);
        if (existing is not null)
        {
            return Map(existing);
        }

        return await UpsertAsync(
            new TrialConfigurationUpsertRequest(
                trialStartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                50.00m,
                [TrialRegion.UK],
                null,
                true,
                true,
                2,
                7,
                null),
            cancellationToken);
    }

    private static void Validate(TrialConfigurationUpsertRequest request)
    {
        if (request.OpeningRingFencedAmount <= 0)
        {
            throw new TrialValidationException("Opening ring-fenced amount must be greater than zero.");
        }

        if (!TrialRegionParser.IsValidSubset(request.PermittedRegions))
        {
            throw new TrialValidationException("Permitted regions must be a non-empty subset of UK, US, and Europe.");
        }

        if (request.IntendedHoldDaysMin > request.IntendedHoldDaysMax)
        {
            throw new TrialValidationException("Intended hold day minimum cannot exceed maximum.");
        }
    }

    private static TrialConfigurationDto Map(TrialConfiguration entity) =>
        new(
            entity.TrialStartDate,
            entity.OpeningRingFencedAmount,
            TrialRegionParser.ParsePermittedRegions(entity.PermittedRegions),
            entity.ExcludedThemesNotes,
            entity.RealSharesOnly,
            entity.NoLeverageOrDerivatives,
            entity.IntendedHoldDaysMin,
            entity.IntendedHoldDaysMax,
            entity.NonTrialHoldingsNote,
            entity.ShortTermReviewCompletedAt);

    private string RequireUserId() =>
        userContext.UserId ?? throw new InvalidOperationException("User must be authenticated.");
}
