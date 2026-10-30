using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PennyForYourStocks.Application.Abstractions;
using PennyForYourStocks.Infrastructure.Persistence;

namespace PennyForYourStocks.Infrastructure.Persistence;

public sealed class TrialDbContextFactory : IDesignTimeDbContextFactory<TrialDbContext>
{
    public TrialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrialDbContext>();
        optionsBuilder.UseSqlite("Data Source=triallog-design.db");
        return new TrialDbContext(optionsBuilder.Options, new DesignTimeUserContext());
    }

    private sealed class DesignTimeUserContext : ITrialUserContext
    {
        public string? UserId => null;
    }
}
