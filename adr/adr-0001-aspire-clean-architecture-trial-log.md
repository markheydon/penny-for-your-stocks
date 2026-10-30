---
title: "ADR-0001: Aspire, clean architecture, and Identity passkeys for trial log"
status: "Accepted"
date: "2026-10-30"
authors: "Project owner"
tags: ["architecture", "aspire", "identity", "ef-core", "sqlite"]
supersedes: ""
superseded_by: ""
---

# ADR-0001: Aspire, clean architecture, and Identity passkeys for trial log

## Status

**Accepted**

## Context

The written trial log feature requires a cloud-backed, signed-in web application with per-user data isolation, derived cash-only pot maths, milestone reviews, and export—without share recommendations or broker integration. The owner requires .NET Aspire orchestration (no hand-rolled runtime connection strings), clean architecture for testable domain rules, SQLite initially with a PostgreSQL swap path, ASP.NET Core Identity with passkeys on .NET 10, and a Blazor Web App with Tailwind CSS.

AppHost-modelled resources must be consumed through Aspire-supplied client integrations and dependency injection, not by reading ad hoc connection strings in application startup code.

## Decision

Adopt a multi-project layout: **AppHost** (Aspire orchestration), **ServiceDefaults** (OTel, health, resilience), **Web** (Blazor + minimal APIs), **Domain**, **Application**, and **Infrastructure** (EF Core, Identity, repositories). Use Identity schema version 3 with passkeys; global EF query filters on `UserId`. Retire the placeholder single class library.

### SQLite via Aspire (Community Toolkit)

SQLite is **not** in the core Aspire SDK; this repository uses the [Aspire Community Toolkit](https://learn.microsoft.com/dotnet/aspire/community-toolkit/overview) packages documented on [aspire.dev](https://aspire.dev/integrations/databases/sqlite/sqlite-host/).

| Layer | Package | Registration |
|-------|---------|--------------|
| **AppHost** | `CommunityToolkit.Aspire.Hosting.Sqlite` | `builder.AddSqlite("triallog")` and `WithReference(trialDb)` on the Web project ([hosting integration](https://aspire.dev/integrations/databases/sqlite/sqlite-host/)) |
| **Web** | `CommunityToolkit.Aspire.Microsoft.EntityFrameworkCore.Sqlite` | `builder.AddSqliteDbContext<TrialDbContext>("triallog")` — connection name **must match** the AppHost resource name ([EF client integration](https://learn.microsoft.com/dotnet/aspire/community-toolkit/sqlite-entity-framework-integration)) |

When the AppHost references the SQLite resource, Aspire injects connection information into the Web process (for example `ConnectionStrings:triallog` and the `TRAILLOG_DATASOURCE` environment variable described in [Connect to SQLite](https://aspire.dev/integrations/databases/sqlite/sqlite-connect/)). The EF client integration reads that configuration, registers **`TrialDbContext` in DI**, and adds SQLite health checks and OpenTelemetry hooks. Application and Infrastructure code **inject `TrialDbContext`** (via EF stores); they do not construct `Microsoft.Data.Sqlite.SqliteConnection` or call `GetConnectionString` in `Program.cs`.

**Not used at runtime:** committed `ConnectionStrings:triallog` in `appsettings.json`, manual `AddDbContext` + `UseSqlite(connectionString)`, or AppHost `AddConnectionString` workarounds.

**Lower-level alternative (not chosen):** `CommunityToolkit.Aspire.Microsoft.Data.Sqlite` with `AddSqliteConnection("triallog")` and constructor injection of `SqliteConnection` per the connect guide. EF Core + Identity migrations require the **`AddSqliteDbContext`** path instead.

**Design-time only:** `TrialDbContextFactory` (`IDesignTimeDbContextFactory`) uses a local `triallog-design.db` path for `dotnet ef` migrations; it does not participate in Aspire orchestration.

First-party NuGet packages named `Aspire.Hosting.Sqlite` and `Aspire.Microsoft.EntityFrameworkCore.Sqlite` are not published; the Community Toolkit packages above are the supported integration surface on aspire.dev.

### PostgreSQL later

Swap AppHost hosting package and Web EF client integration (for example `Aspire.Hosting.PostgreSQL` + `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`) while keeping Domain and Application unchanged; continue `WithReference` + named `Add*DbContext` on Web.

## Consequences

### Positive

- **POS-001**: Domain pot and milestone logic unit-testable without EF or ASP.NET.
- **POS-002**: Aspire supplies the database file path, DI registration, health checks, and telemetry; local run via AppHost (`dotnet run` on AppHost or `aspire run`).
- **POS-003**: Provider swap (SQLite → PostgreSQL) isolated to Infrastructure and AppHost packages.
- **POS-004**: Runtime wiring matches aspire.dev SQLite hosting + EF client patterns; no parallel connection bootstrap in Web.

### Negative

- **NEG-001**: More projects and wiring than a monolithic Web app.
- **NEG-002**: Package surface (Aspire, Community Toolkit, EF, Identity) requires ongoing version alignment with AppHost SDK (currently 13.x).
- **NEG-003**: Passkey RP ID and `ServerDomain` must be configured explicitly for non-local hosts.
- **NEG-004**: Running **Web without AppHost** requires a local `ConnectionStrings:triallog` (for example user secrets) because no default is committed; normal development should use the AppHost.

## Alternatives Considered

### Monolithic Blazor Web project with DbContext in handlers

- **ALT-001**: **Description**: Single Web project containing UI, API, EF, and domain rules.
- **ALT-002**: **Rejection Reason**: Couples financial rules to persistence; harder PostgreSQL swap and unit testing.

### Manual connection strings without Aspire

- **ALT-001**: **Description**: `appsettings.json` only for database connectivity, or AppHost `AddConnectionString` with hand-rolled `AddDbContext`.
- **ALT-002**: **Rejection Reason**: Duplicates orchestration and conflicts with owner Aspire requirement and AppHost-modelled-resource policy.

### Microsoft Entra External ID for authentication

- **ALT-001**: **Description**: Hosted identity with OIDC.
- **ALT-002**: **Rejection Reason**: Operational overhead disproportionate for hobby single-user v1; Identity passkeys sufficient.

## References

- `specs/001-written-trial-log/plan.md`
- `specs/001-written-trial-log/research.md`
- [Set up SQLite in the AppHost](https://aspire.dev/integrations/databases/sqlite/sqlite-host/)
- [Connect to SQLite](https://aspire.dev/integrations/databases/sqlite/sqlite-connect/)
