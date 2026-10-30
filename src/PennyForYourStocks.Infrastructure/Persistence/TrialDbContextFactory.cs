using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Infrastructure.Persistence;

namespace PennyForYourStocks.Infrastructure.Persistence;

/// <summary>
/// Design-time and AppHost EF migration tooling. Uses Aspire-injected connection strings when present.
/// </summary>
public sealed class TrialDbContextFactory : IDesignTimeDbContextFactory<TrialDbContext>
{
    public TrialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrialDbContext>();
        optionsBuilder.UseApplicationServiceProvider(
            new ServiceCollection()
                .Configure<IdentityOptions>(options => options.Stores.SchemaVersion = IdentitySchemaVersions.Version3)
                .BuildServiceProvider());
        optionsBuilder.UseSqlite(ResolveConnectionString());
        return new TrialDbContext(optionsBuilder.Options, new DesignTimeUserContext());
    }

    internal static string ResolveConnectionString()
    {
        var aspireConnection = Environment.GetEnvironmentVariable("ConnectionStrings__triallog");
        if (!string.IsNullOrWhiteSpace(aspireConnection))
        {
            return aspireConnection;
        }

        var dataSource = Environment.GetEnvironmentVariable("TRIALLOG_DATASOURCE");
        if (!string.IsNullOrWhiteSpace(dataSource))
        {
            return dataSource.Contains('=', StringComparison.Ordinal)
                ? dataSource
                : $"Data Source={dataSource}";
        }

        return "Data Source=triallog-design.db";
    }

    private sealed class DesignTimeUserContext : ITrialUserContext
    {
        public string? UserId => null;
    }
}
