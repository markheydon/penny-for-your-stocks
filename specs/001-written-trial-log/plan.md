# Implementation Plan: Written trial log with stop rule

**Branch**: `001-written-trial-log` | **Date**: 2026-10-30 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-written-trial-log/spec.md` plus owner constraints: .NET Aspire orchestration, EF Core with clean architecture, SQLite initially (PostgreSQL-ready), repository testing standards.

## Summary

Deliver a signed-in, cloud-backed web application that records weekly trial checks, enforces trial boundary messaging, derives a cash-only trial pot, surfaces milestone reviews (3 / 12 / 18 months), and supports export—without share recommendations or broker integration. The system is modelled in a **.NET Aspire AppHost** (database and projects wired there); runtime code accesses infrastructure only via **DI and Aspire client integrations** (no hand-rolled connection strings or parallel resource bootstrapping). **Clean architecture** separates domain rules (pot maths, milestone eligibility, policy warnings) from EF Core persistence in Infrastructure, with **SQLite** as the first provider and a swap path to **PostgreSQL** via provider-specific Aspire packages and unchanged application abstractions. **Authentication** uses **ASP.NET Core Identity on .NET 10** with **passkeys** (WebAuthn) as the preferred sign-in path, with optional password fallback only if needed for account recovery flows. **UI** is a **Blazor Web App** styled with **Tailwind CSS**, **Lucide** icons, and **Chart.js** for hope/cash visualisations—no MudBlazor or Fluent UI component libraries.

## Technical Context

**Language/Version**: C# on **.NET 10 only** (`net10.0` for all projects; remove legacy `net8.0` from the placeholder library when restructuring).

**Primary Dependencies**: .NET Aspire (AppHost, ServiceDefaults), ASP.NET Core (Blazor Web App + minimal API or endpoint groups), Entity Framework Core, ASP.NET Core Identity, Aspire EF integrations (`Aspire.Microsoft.EntityFrameworkCore.Sqlite`; PostgreSQL package reserved for later), optional `Microsoft.Extensions.Http.Resilience` on typed HTTP clients if external HTTP is added later.

**Storage**: Relational database via EF Core—**SQLite** for local/hobby deployment initially; schema and repositories designed for **PostgreSQL** (Npgsql) swap without domain changes.

**Testing**: xUnit v3, NSubstitute, built-in asserts only; unit tests on Domain and Application; Infrastructure integration tests against SQLite in-memory or test containers as needed; **no AppHost/orchestration tests**; **no Playwright or browser E2E** in this feature (manual quickstart scenarios only).

**Target Platform**: Cross-device browser UI backed by a hosted API; local development via `aspire run` / AppHost.

**Project Type**: Multi-project web application (AppHost + UI + API layers + clean architecture libraries).

**Performance Goals**: Single-user / low concurrency; sub-second API responses for log CRUD and summary generation on typical weekly data volumes (hundreds of entries, not millions).

**Constraints**: No market data, broker APIs, or share recommendations; auth required; per-user data isolation; UK English UI copy; warnings as errors; nullable enabled; async + `CancellationToken` on public I/O APIs; configuration via `IOptions<T>` in hosted apps; outbound HTTP only via typed `AddHttpClient<T>()` if introduced.

**Scale/Scope**: One trial owner per account; ~52 log entries per year; three milestone review types plus optional early stop; export on demand.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Pre–Phase 0 | Post–Phase 1 |
|-----------|-------------|----------------|
| **I. Security-First** | Plan requires sign-in, per-user isolation, fail-closed auth on API, passkeys + HTTPS, secrets via Aspire/user secrets—not committed. | Identity schema v3 passkeys; `IdentityPasskeyOptions.ServerDomain` in production; authorised endpoints; export scoped to owner. **PASS** |
| **II. Validated Input at Boundaries** | API DTO validation + domain invariants for money fields, dates, enums. | Contracts document required fields and limits; data model lists validation rules. **PASS** |
| **III. Documented Public Surface** | Public domain/application APIs and HTTP endpoints get XML docs; user-facing copy UK English. | Contracts describe external HTTP surface. **PASS** |
| **IV. Test Discipline** | Unit tests specified for pot calculation, cost summation, milestone dates, capital gate, same-day entry merge, auth boundaries. | quickstart.md lists verification scenarios mapping to FRs. **PASS** |
| **V. Maintainable Code Quality** | Aspire + clean architecture + EF; ADR required before implementation (see below). | Layer boundaries in structure decision; no `.Result`/`.Wait()`. **PASS** (pending ADR commit before code) |

**Technology constraints**: Stays within SCOPE.md (no recommendations, no broker execution). Package additions will need human approval per AGENTS.md when implementation starts.

**ADR gate**: Adopting Aspire, multi-project clean architecture, and Identity is a significant stack choice. Record **`adr/0001-aspire-clean-architecture-trial-log.md`** (or next number) before merging implementation PRs—not part of this plan artifact set.

## Project Structure

### Documentation (this feature)

```text
specs/001-written-trial-log/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── http-api.md
│   ├── export-format.md
│   └── ui-stack.md
└── tasks.md             # Phase 2 (/speckit-tasks) — not created here
```

### Source Code (repository root)

```text
src/
├── PennyForYourStocks.AppHost/              # Aspire orchestration only
├── PennyForYourStocks.ServiceDefaults/      # OTel, health, resilience defaults
├── PennyForYourStocks.Web/                  # Blazor Web App (UI + all `/api/v1` minimal APIs)
├── PennyForYourStocks.Domain/               # Entities, value objects, domain services
├── PennyForYourStocks.Application/            # Use cases, interfaces, DTOs
└── PennyForYourStocks.Infrastructure/       # EF Core, Identity stores, repositories

tests/
├── PennyForYourStocks.Domain.Tests/
├── PennyForYourStocks.Application.Tests/
└── PennyForYourStocks.Infrastructure.Tests/
```

**Structure Decision**: Replace the placeholder `src/PennyForYourStocks` class library with the layout above. **AppHost** references **Web** only for HTTP/UI, adds **SQLite** (later **PostgreSQL**) via Aspire hosting packages, and passes connection references with `WithReference`—application projects call `AddSqliteDbContext<TContext>` / future `AddNpgsqlDbContext<TContext>` only. **Domain** has no EF or ASP.NET references. **Application** defines repository and clock abstractions; **Infrastructure** implements EF mappings and Identity. **Web** hosts Blazor and minimal APIs and consumes Application via DI registration extension methods.

### UI strategy

| Layer | Choice | Notes |
|-------|--------|--------|
| Framework | Blazor Web App (.NET 10) | Interactive Server (or Auto) for Identity cookies and passkey endpoints; see [research.md](./research.md) §5–§6 |
| Layout & styling | **Tailwind CSS** | Utility-first; build via Tailwind CLI or Vite/npm in `PennyForYourStocks.Web`; no MudBlazor / Fluent UI |
| Icons | **Lucide** | SVG icons (static assets or small Blazor wrapper); consistent stroke icons in nav and actions |
| Charts | **Chart.js** | Hope vs observed withdrawal trends and monthly buckets; thin JS interop module, no live market charts |
| Components | Native Razor + Tailwind | Forms, tables, and milestone flows composed in-house; guardrail copy as plain components |

Detail: [contracts/ui-stack.md](./contracts/ui-stack.md).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| 7+ projects (AppHost, ServiceDefaults, 3 layers, Web, tests) | Aspire + clean architecture + testability | Monolith in one Web project blurs domain rules and complicates PostgreSQL swap and unit testing of pot/milestone logic |
| Repository interfaces over direct DbContext in Application | EF provider swap and isolated domain tests | DbContext in handlers couples use cases to SQLite/PostgreSQL details |

## Phase 0 & 1 Outputs

- **research.md**: Resolved Aspire wiring, auth, UI, sync, export, and EF provider strategy.
- **data-model.md**: Persistence model aligned to spec entities and derived pot rules.
- **contracts/**: HTTP and export contracts for client and future agents.
- **quickstart.md**: Runnable validation scenarios (post-implementation).
