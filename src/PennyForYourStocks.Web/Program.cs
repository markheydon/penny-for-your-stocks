using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PennyForYourStocks.Infrastructure;
using PennyForYourStocks.Infrastructure.Identity;
using PennyForYourStocks.Infrastructure.Persistence;
using PennyForYourStocks.Web;
using PennyForYourStocks.Web.Api;
using PennyForYourStocks.Web.Components;
using PennyForYourStocks.Web.Components.Account;
using PennyForYourStocks.Web.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddTrialInfrastructure();
builder.Services.AddScoped<PennyForYourStocks.Application.Abstractions.ITrialUserContext, AuthenticatedTrialUserContext>();

builder.Services.AddDbContext<TrialDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("triallog")
        ?? throw new InvalidOperationException("Connection string 'triallog' not found. Run under the AppHost or set ConnectionStrings:triallog.");
    options.UseSqlite(connectionString);
    options.UseApplicationServiceProvider(serviceProvider);
});
builder.Services.AddExceptionHandler<DatabaseNotReadyExceptionHandler>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<TrialDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityPasskeyOptions>(options =>
{
    if (!builder.Environment.IsDevelopment())
    {
        var serverDomain = builder.Configuration["Passkeys:ServerDomain"];
        if (!string.IsNullOrWhiteSpace(serverDomain))
        {
            options.ServerDomain = serverDomain;
        }
    }
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseExceptionHandler();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaleIdentitySessionCleanup();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapTrialApi();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();
