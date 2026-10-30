using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Application.Models;
using PennyForYourStocks.Application.Policies;
using PennyForYourStocks.Domain;

namespace PennyForYourStocks.Application.Services;

/// <summary>JSON and CSV export bundles (FR-016).</summary>
public sealed class TrialExportService(
    ITrialReadStore readStore,
    TrialConfigurationService configurationService,
    LogEntryService logEntryService,
    TrialSummaryService summaryService)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };

    /// <summary>Builds the canonical JSON export document.</summary>
    public async Task<string> ExportJsonAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await configurationService.GetAsync(cancellationToken);
        var entries = await logEntryService.ListAsync(null, null, cancellationToken);
        var fullEntries = new List<object>();
        foreach (var summary in entries)
        {
            var detail = await logEntryService.GetAsync(summary.CheckDate, cancellationToken);
            if (detail is not null)
            {
                fullEntries.Add(detail);
            }
        }

        var topUps = await readStore.ListTopUpsAsync(cancellationToken);
        var cash = await readStore.ListCashObservationsAsync(cancellationToken);
        var reviews = await readStore.ListMilestoneReviewsAsync(cancellationToken);

        object? derived = null;
        var allEntries = await readStore.ListLogEntriesAsync(null, null, cancellationToken);
        if (PracticeModePolicy.IsSummaryCompliant(allEntries))
        {
            try
            {
                var pot = await summaryService.GetPotSummaryAsync(cancellationToken);
                var hope = await summaryService.GetHopeSummaryAsync(cancellationToken);
                derived = new { pot, hopeComparison = hope };
            }
            catch (TrialValidationException)
            {
            }
            catch (TrialPolicyException)
            {
            }
        }

        var root = new
        {
            exportVersion = "1.0",
            exportedAt = DateTimeOffset.UtcNow,
            configuration,
            logEntries = fullEntries,
            topUps,
            cashObservations = cash,
            milestoneReviews = reviews,
            derived,
        };

        return JsonSerializer.Serialize(root, JsonOptions);
    }

    /// <summary>Builds CSV with one row per log entry.</summary>
    public async Task<string> ExportCsvAsync(CancellationToken cancellationToken = default)
    {
        var entries = await readStore.ListLogEntriesAsync(null, null, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("CheckDate,Consideration,HoldingsSummary,TotalCosts,WithdrawalAttributed,ComplianceNotes");
        foreach (var entry in entries.OrderBy(e => e.CheckDate))
        {
            var holdingsSummary = string.Join(';', entry.Holdings.Select(h => h.IdentifierOrName));
            var totalCosts = LogEntryCostCalculator.GetEntryTotalCosts(entry);
            sb.AppendLine(string.Join(',',
                Csv(entry.CheckDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Csv(entry.Consideration),
                Csv(holdingsSummary),
                totalCosts.ToString(CultureInfo.InvariantCulture),
                (entry.WithdrawalAttributed ?? 0m).ToString(CultureInfo.InvariantCulture),
                Csv(entry.ComplianceNotes ?? string.Empty)));
        }

        return sb.ToString();
    }

    private static string Csv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
