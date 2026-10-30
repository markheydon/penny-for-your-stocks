var builder = DistributedApplication.CreateBuilder(args);

var trialDb = builder.AddConnectionString("triallog", "Data Source=triallog.db");

builder.AddProject<Projects.PennyForYourStocks_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(trialDb);

builder.Build().Run();
