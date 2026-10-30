# Penny For Your Stocks

A personal application to run a **written trial with a stop rule** for modest active share dealing: log weekly checks, record spread and dealing costs explicitly, review milestones at about 3, 12, and 18 months, and decide from your own record whether the attempt is clearly just losing money—not from stock tips or automated trading.

Built primarily because apparently simply asking shares to go up isn't a viable investment strategy.

The project delivers an Aspire-orchestrated Blazor web app with Identity passkeys, a clean architecture trial log, and SQLite persistence. It is experimental, makes no promise of returns, and treats withdrawing about £100–£200 a month as a hope that may never be met.

## Run with Aspire AppHost

From the repository root:

```bash
dotnet run --project src/PennyForYourStocks.AppHost/PennyForYourStocks.AppHost.csproj
```

The AppHost models a SQLite resource named `triallog` and passes it to the Web project with `WithReference`. The Web app registers `TrialDbContext` through the Aspire Community Toolkit EF integration (`AddSqliteDbContext<TrialDbContext>("triallog")`); repositories and Identity resolve the context from DI. Aspire injects the database path at run time (see [Connect to SQLite](https://aspire.dev/integrations/databases/sqlite/sqlite-connect/)). HTTP health is exposed at `/health`.

To run the web project without the AppHost (unusual), set `ConnectionStrings:triallog` locally (for example via user secrets). There is no committed default connection string.

## Tailwind CSS

Styles are built into `src/PennyForYourStocks.Web/wwwroot/css/app.css`.

```bash
cd src/PennyForYourStocks.Web
npm ci
npm run build:css
```

Source input: `Styles/app.css`. Re-run `npm run build:css` after changing Razor markup classes or Tailwind config.

## Development checks

```bash
dotnet format PennyForYourStocks.slnx --verify-no-changes
dotnet build PennyForYourStocks.slnx -c Release -warnaserror
dotnet test PennyForYourStocks.slnx -c Release --no-build
```

## Repository layout

| Path | Purpose |
|------|---------|
| `PennyForYourStocks.slnx` | .NET solution (AppHost, web, domain, application, infrastructure, tests) |
| `src/PennyForYourStocks.AppHost/` | Aspire orchestration |
| `src/PennyForYourStocks.Web/` | Blazor UI, Identity passkeys, minimal API |
| `src/PennyForYourStocks.Domain/` | Trial rules and calculators |
| `src/PennyForYourStocks.Application/` | Use cases and export |
| `src/PennyForYourStocks.Infrastructure/` | EF Core `TrialDbContext`, Identity, migrations |
| `tests/PennyForYourStocks.*.Tests/` | Unit and integration tests |
| `specs/001-written-trial-log/` | Feature specification and tasks |
| `GOALS.md`, `SCOPE.md`, `CONVENTIONS.md` | Product and coding context |
| `adr/` | Architectural decision records |

## Licence

MIT — see [LICENSE](LICENSE).
