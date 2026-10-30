using Microsoft.Extensions.DependencyInjection;
using PennyForYourStocks.Application.Services;

namespace PennyForYourStocks.Application;

/// <summary>Registers application-layer trial services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds trial application services to the service collection.</summary>
    public static IServiceCollection AddTrialApplication(this IServiceCollection services)
    {
        services.AddScoped<TrialConfigurationService>();
        services.AddScoped<LogEntryService>();
        services.AddScoped<MilestoneReviewService>();
        services.AddScoped<TrialTopUpService>();
        services.AddScoped<CashObservationService>();
        services.AddScoped<TrialSummaryService>();
        services.AddScoped<TrialExportService>();
        return services;
    }
}
