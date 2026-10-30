using Microsoft.Extensions.DependencyInjection;
using PennyForYourStocks.Application;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Infrastructure.Identity;
using PennyForYourStocks.Infrastructure.Stores;

namespace PennyForYourStocks.Infrastructure;

/// <summary>Registers infrastructure persistence and trial stores.</summary>
public static class DependencyInjection
{
    /// <summary>Adds EF trial stores and user context.</summary>
    public static IServiceCollection AddTrialInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITrialUserContext, HttpTrialUserContext>();
        services.AddScoped<ITrialReadStore, EfTrialReadStore>();
        services.AddScoped<ITrialWriteStore, EfTrialWriteStore>();
        services.AddTrialApplication();
        return services;
    }
}
