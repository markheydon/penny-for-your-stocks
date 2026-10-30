using Microsoft.AspNetCore.Mvc;
using PennyForYourStocks.Application;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Services;

namespace PennyForYourStocks.Web.Api;

internal static class TrialApiEndpoints
{
    public static RouteGroupBuilder MapTrialApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/trial")
            .RequireAuthorization();

        group.MapGet("/guardrails", () => Results.Ok(GuardrailsService.GetGuardrails()));

        group.MapGet("/configuration", async (TrialConfigurationService service, CancellationToken ct) =>
        {
            var dto = await service.GetAsync(ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        group.MapPut("/configuration", async (TrialConfigurationUpsertRequest body, TrialConfigurationService service, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await service.UpsertAsync(body, ct));
            }
            catch (TrialValidationException ex)
            {
                return ValidationProblem(ex);
            }
        });

        group.MapGet("/entries", async (DateOnly? from, DateOnly? to, LogEntryService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(from, to, ct)));

        group.MapGet("/entries/{checkDate}", async (DateOnly checkDate, LogEntryService service, CancellationToken ct) =>
        {
            var entry = await service.GetAsync(checkDate, ct);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        });

        group.MapPut("/entries/{checkDate}", async (DateOnly checkDate, LogEntryUpsertRequest body, LogEntryService service, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await service.UpsertForCheckDateAsync(checkDate, body, ct));
            }
            catch (TrialConcurrencyException)
            {
                return Results.Conflict(new ProblemDetails { Title = "Concurrency conflict", Status = StatusCodes.Status409Conflict });
            }
            catch (TrialValidationException ex)
            {
                return ValidationProblem(ex);
            }
        });

        group.MapGet("/summary/pot", async (TrialSummaryService service, CancellationToken ct) =>
            await SummaryResult(() => service.GetPotSummaryAsync(ct)));

        group.MapGet("/summary/hope", async (TrialSummaryService service, CancellationToken ct) =>
            await SummaryResult(() => service.GetHopeSummaryAsync(ct)));

        group.MapGet("/summary/milestone-12", async (TrialSummaryService service, CancellationToken ct) =>
            await SummaryResult(() => service.GetMilestone12SummaryAsync(ct)));

        group.MapGet("/milestones", async (MilestoneReviewService service, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await service.GetScheduleAsync(ct));
            }
            catch (TrialValidationException ex)
            {
                return ValidationProblem(ex);
            }
        });

        group.MapPost("/milestones/reviews", async (MilestoneReviewUpsertRequest body, MilestoneReviewService service, CancellationToken ct) =>
        {
            try
            {
                await service.UpsertReviewAsync(body, ct);
                return Results.NoContent();
            }
            catch (TrialConcurrencyException)
            {
                return Results.Conflict(new ProblemDetails { Title = "Concurrency conflict", Status = StatusCodes.Status409Conflict });
            }
            catch (TrialValidationException ex)
            {
                return ValidationProblem(ex);
            }
        });

        group.MapPost("/top-ups", async (TopUpRequest body, TrialTopUpService service, CancellationToken ct) =>
        {
            try
            {
                await service.RecordTopUpAsync(body, ct);
                return Results.NoContent();
            }
            catch (TrialPolicyException ex)
            {
                return Results.UnprocessableEntity(new ProblemDetails
                {
                    Title = ex.Message,
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Extensions = { ["policyCode"] = ex.PolicyCode },
                });
            }
            catch (TrialValidationException ex)
            {
                return ValidationProblem(ex);
            }
        });

        group.MapPost("/cash-observations", async (CashObservationRequest body, CashObservationService service, CancellationToken ct) =>
        {
            try
            {
                await service.RecordAsync(body, ct);
                return Results.NoContent();
            }
            catch (TrialValidationException ex)
            {
                return ValidationProblem(ex);
            }
        });

        group.MapGet("/export", async (string? format, TrialExportService exportService, CancellationToken ct) =>
        {
            var fmt = (format ?? "json").ToLowerInvariant();
            return fmt switch
            {
                "csv" => Results.Text(await exportService.ExportCsvAsync(ct), "text/csv"),
                "json" => Results.Text(await exportService.ExportJsonAsync(ct), "application/json"),
                _ => Results.BadRequest(new ProblemDetails { Title = "Supported formats: json, csv" }),
            };
        });

        return group;
    }

    private static async Task<IResult> SummaryResult<T>(Func<Task<T>> action)
    {
        try
        {
            return Results.Ok(await action());
        }
        catch (TrialPolicyException ex)
        {
            return Results.UnprocessableEntity(new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
                Extensions = { ["policyCode"] = ex.PolicyCode, ["practiceModeCompliance"] = ex.Message },
            });
        }
        catch (TrialValidationException ex)
        {
            return ValidationProblem(ex);
        }
    }

    private static IResult ValidationProblem(TrialValidationException ex) =>
        Results.ValidationProblem(
            ex.Errors ?? new Dictionary<string, string[]> { [""] = [ex.Message] },
            title: ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
}
