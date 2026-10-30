var builder = DistributedApplication.CreateBuilder(args);

var trialDb = builder.AddSqlite("triallog");

#pragma warning disable ASPIREDOTNETPROJECT001 // Project v2 (AddDotnetProject) is the approved AppHost pattern for this repo.
#pragma warning disable ASPIREPROJECTS001 // AddEFMigrations on IDotnetProgramResource requires this paired suppression.
var web = builder.AddDotnetProject("web", "../PennyForYourStocks.Web/PennyForYourStocks.Web.csproj")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(trialDb);

var migrations = web
    .AddEFMigrations("triallog-migrations", "PennyForYourStocks.Infrastructure.Persistence.TrialDbContext")
    .WithMigrationsProject("../PennyForYourStocks.Infrastructure/PennyForYourStocks.Infrastructure.csproj")
    .RunDatabaseUpdateOnStart()
    .WaitFor(trialDb);

web.WaitForCompletion(migrations);
#pragma warning restore ASPIREPROJECTS001
#pragma warning restore ASPIREDOTNETPROJECT001

builder.Build().Run();
