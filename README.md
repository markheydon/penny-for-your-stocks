# Penny For Your Stocks

A personal application to run a **written trial with a stop rule** for modest active share dealing: log weekly checks, record spread and dealing costs explicitly, review milestones at about 3, 12, and 18 months, and decide from your own record whether the attempt is clearly just losing money—not from stock tips or automated trading.

Built primarily because apparently simply asking shares to go up isn't a viable investment strategy.

The project delivers an Aspire-orchestrated Blazor web app with Identity passkeys, a clean architecture trial log, and SQLite persistence. It is experimental, makes no promise of returns, and treats withdrawing about £100–£200 a month as a hope that may never be met.

## Run with Aspire AppHost

From the repository root (prefer the Aspire CLI so migrations and health orchestration match CI/local dashboards):

```bash
aspire run
```

Or open the AppHost project in the IDE. The AppHost path is also in `aspire.config.json` at the repo root.

The AppHost models SQLite (`triallog`), runs EF migrations (`triallog-migrations`) via `Aspire.Hosting.EntityFrameworkCore`, then starts the Web project (`AddDotnetProject`). The dashboard shows migration completion before Web becomes healthy. Web uses Aspire-injected `ConnectionStrings:triallog` and scoped `TrialDbContext` in DI—migrations are not run from `Program.cs`.

When the Dev Container serves HTTPS and your **Windows** browser warns about the certificate, trust the public dev cert on the host:

```bash
aspire certs trust --non-interactive
./scripts/export-dev-cert-for-windows-host.sh
```

Then run the PowerShell `Import-Certificate` lines the script prints (into `Cert:\CurrentUser\Root`), fully quit the browser, and reopen the dashboard.

## Tailwind CSS

Styles compile on every **`dotnet build`** via the [Tailwind.MSBuild](https://www.nuget.org/packages/Tailwind.MSBuild) package (MSBuild tasks + standalone Tailwind CLI—**no Node/npm**). Input: `Styles/app.css` and `tailwind.config.js`. Output: `wwwroot/css/app.css`. Restart or rebuild the Web project after changing Razor classes or Tailwind config (Aspire extension rebuild is enough).

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
