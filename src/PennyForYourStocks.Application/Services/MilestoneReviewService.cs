using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain;
using PennyForYourStocks.Domain.Entities;

namespace PennyForYourStocks.Application.Services;

/// <summary>Milestone schedule and review upserts.</summary>
public sealed class MilestoneReviewService(
    ITrialReadStore readStore,
    ITrialWriteStore writeStore,
    ITrialUserContext userContext)
{
    /// <summary>Returns computed due dates and completion status.</summary>
    public async Task<MilestoneScheduleDto> GetScheduleAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await readStore.GetConfigurationAsync(cancellationToken)
            ?? throw new TrialValidationException("Trial configuration is required before viewing milestones.");
        var due = MilestoneSchedule.GetDueDates(configuration.TrialStartDate);
        var reviews = await readStore.ListMilestoneReviewsAsync(cancellationToken);
        var status = Enum.GetValues<ReviewType>()
            .Select(type =>
            {
                var review = reviews.FirstOrDefault(r => r.ReviewType == type);
                return new MilestoneReviewStatusDto(type, review is not null, review?.DecisionDate);
            })
            .ToList();

        return new MilestoneScheduleDto(due.ShortTerm3Month, due.StopOrContinue12Month, due.FollowUp18Month, status);
    }

    /// <summary>Upserts a milestone review for the given type.</summary>
    public async Task UpsertReviewAsync(MilestoneReviewUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.JudgementText))
        {
            throw new TrialValidationException("Judgement text is required.");
        }

        if (request.JudgementText.Length > TrialFieldLimits.JudgementText)
        {
            throw new TrialValidationException($"Judgement text cannot exceed {TrialFieldLimits.JudgementText} characters.");
        }

        var userId = RequireUserId();
        var existing = await readStore.GetMilestoneReviewAsync(request.ReviewType, cancellationToken);
        if (existing is not null && request.RowVersion is not null
            && !existing.RowVersion.SequenceEqual(request.RowVersion))
        {
            throw new TrialConcurrencyException();
        }

        var review = existing ?? new MilestoneReview
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReviewType = request.ReviewType,
            JudgementText = request.JudgementText,
        };

        review.DecisionDate = request.DecisionDate;
        review.JudgementText = request.JudgementText;
        review.HopeStatus = request.HopeStatus;
        review.FurtherCapitalInScope = request.FurtherCapitalInScope;

        await writeStore.UpsertMilestoneReviewAsync(review, cancellationToken);

        if (request.ReviewType == ReviewType.ShortTerm3Month)
        {
            var configuration = await readStore.GetConfigurationAsync(cancellationToken);
            if (configuration is not null && configuration.ShortTermReviewCompletedAt is null)
            {
                configuration.ShortTermReviewCompletedAt = DateTimeOffset.UtcNow;
                configuration.UpdatedAt = DateTimeOffset.UtcNow;
                await writeStore.SaveConfigurationAsync(configuration, cancellationToken);
            }
        }
    }

    private string RequireUserId() =>
        userContext.UserId ?? throw new InvalidOperationException("User must be authenticated.");
}
