var builder = DistributedApplication.CreateBuilder(args);

var trialDb = builder.AddSqlite("triallog");

builder.AddProject<Projects.PennyForYourStocks_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(trialDb);

builder.Build().Run();
