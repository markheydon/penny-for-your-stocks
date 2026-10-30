# Research: Written trial log (001)

**Feature**: `001-written-trial-log`  
**Date**: 2026-10-30 (revised: passkeys, UI stack, .NET 10 only, no E2E)

## 1. .NET Aspire resource modelling

**Decision**: Use an **AppHost** project as the sole place that declares the database and web project resources; use **ServiceDefaults** on the web/API project; consume the database via **`Aspire.Microsoft.EntityFrameworkCore.Sqlite`** (`AddSqliteDbContext<TrialDbContext>`) with an AppHost `AddSqlite` / database resource and `WithReference`.

**Rationale**: Owner requirement: no roll-your-own wiring—connection strings and health checks come from Aspire integration. Matches Microsoft guidance for EF + Aspire.

**Alternatives considered**:

- Manual `ConnectionStrings` in `appsettings.json` only—rejected (bypasses Aspire, duplicates orchestration).
- PostgreSQL container from day one—valid for production parity but heavier for hobby local run; deferred to swap phase.

## 2. Database provider and clean architecture

**Decision**: **SQLite** for initial deployment and development; **EF Core** with provider-specific configuration isolated in **Infrastructure** (`TrialDbContext`, migrations). Application depends on **`ITrialReadStore` / `ITrialWriteStore`** (or unit-of-work) interfaces, not `DbContext`. Switching to PostgreSQL later: add **`Aspire.Hosting.PostgreSQL`**, **`Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`**, new AppHost resource reference, and Infrastructure registration branch—domain and application unchanged.

**Rationale**: Spec allows either; SQLite minimises ops for a single-user hobby app while EF + interfaces preserve swap cost.

**Alternatives considered**:

- Dapper only—rejected (harder migration story, more manual SQL across providers).
- Two separate schemas per provider—rejected (one model, provider-specific types only where needed, e.g. decimal precision).

## 3. Authentication and cloud account (passkeys)

**Decision**: **ASP.NET Core Identity on .NET 10** with **first-class passkey (WebAuthn) support**, EF stores in the same **`TrialDbContext`**, **Identity schema version 3**, cookie sign-in for Blazor, **`IdentityPasskeyOptions.ServerDomain`** set explicitly in non-local environments, HTTPS required. Primary UX: **passwordless account creation and sign-in via passkey**; optional password + recovery only if needed for edge cases (document in implement tasks). All trial rows keyed by **`UserId`**; global query filters enforce FR-014 isolation.

**Rationale**: FR-013 requires cloud sign-in and sync; passkeys are phishing-resistant and avoid password friction. **.NET 10 ships passkeys in Identity** (merged ASP.NET Core PR, documented on Learn for Blazor Web App template)—aligned with targeting net10.0 only. Blazor template includes passkey registration, management UI, and endpoints (`PasskeyCreationOptions` / `PasskeyRequestOptions` pattern).

**Passkeys vs Microsoft Entra / External ID**:

| Aspect | Identity + passkeys (.NET 10) | Entra ID / External ID |
|--------|--------------------------------|-------------------------|
| Passkeys | Built into Identity for Blazor Web App | Entra supports FIDO2 for workforce/consumer tenants but adds tenant, app registration, and hosted UI complexity |
| Cost / ops | Single app + SQLite/Postgres | Azure subscription, redirect URIs, secret rotation |
| Fit for hobby single-user v1 | Strong | Overkill unless social/OIDC logins required day one |

**Decision reaffirmed**: Stay on **ASP.NET Core Identity with passkeys**; defer **Microsoft Entra External ID** until a requirement for social login, B2B, or centralised tenant policy appears. Can add **OpenID Connect** handler alongside passkeys later without changing domain layer.

**Implementation notes**:

- Configure `options.Stores.SchemaVersion = IdentitySchemaVersions.Version3` and passkey options per [Learn: passkeys in Blazor](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/passkeys/blazor?view=aspnetcore-10.0).
- Validate **AllowedHosts** / forwarded headers when inferring RP ID; use explicit **ServerDomain** in production.
- Limitations accepted: Identity passkeys are **not** a general FIDO2 library; no default attestation validation; passkeys are **primary** factor (not 2FA)—acceptable for this app.

**Alternatives considered**:

- Password-only Identity—rejected (owner preference; worse UX and security posture).
- Auth0 / Passkeys via third party—rejected (extra vendor for single-user hobby).
- Client-only localStorage log—rejected (fails sync requirement).

## 4. “Sync across devices”

**Decision**: **Online authoritative server** model for v1: each device uses the same signed-in session/API; data lives in the database. **Optimistic concurrency** (`RowVersion` / xmin on PostgreSQL later) on trial aggregate roots so conflicting edits surface for user reconciliation (spec edge case). **Offline queue** deferred to a follow-up task if needed (spec allows offline if planning supports it—document as non-MVP).

**Rationale**: Simplest correct sync for one user; avoids CRDT complexity.

**Alternatives considered**:

- PWA offline-first with background sync—more engineering; track as enhancement.
- Per-device copies merged by last-write-wins—rejected (silent loss on milestone reviews).

## 5. UI stack (Blazor + Tailwind + Lucide + Chart.js)

**Decision**: **Blazor Web App** on .NET 10 (Interactive Server for MVP) as the Aspire web resource, with:

- **Tailwind CSS** for all layout and styling (no MudBlazor / Fluent UI).
- **Lucide** for icons (SVG).
- **Chart.js** (via minimal JS interop) for hope-vs-observed cash charts only—no market price charts.

**Rationale**: Cross-device browser, single stack, Identity/passkey template alignment, owner UI policy. Tailwind keeps components custom and lightweight for a small set of screens (log, config, reviews, export, account).

**Alternatives considered**:

- MudBlazor / Fluent UI—rejected by owner policy.
- SPA (React) + API—more repos and duplication; Blazor keeps Application services in-process.
- MAUI—rejected (browser + sync via web).

## 6. API shape

**Decision**: **Minimal APIs or endpoint groups** in the Web project (or dedicated Api project if Blazor separation needed), JSON over HTTPS, documented in `contracts/http-api.md`. Blazor pages call Application services in-process first; extract HTTP for export/automation if needed.

**Rationale**: In-process Application calls reduce latency for MVP; contracts still define the logical surface for tests and export clients.

**Alternatives considered**:

- gRPC—rejected (no spec need).

## 7. Derived trial pot and milestones

**Decision**: Implement **domain service** `TrialPotCalculator` and **`MilestoneSchedule`** (pure functions over configuration + entries + reviews + top-ups/withdrawals); persist snapshots only if needed for performance—default **compute on read** for auditability.

**Rationale**: Constitution requires tested financial logic; computed view matches FR-015 (no mark-to-market).

**Alternatives considered**:

- Cached pot column updated on every write—acceptable optimisation later with tests proving equivalence.

## 8. Export

**Decision**: **JSON** document export (full stored trial bundle: configuration, entries, reviews; **`derived` block only when SC-005 satisfied**) plus optional **CSV** for entries; available anytime (FR-016).

**Rationale**: Machine-readable backup and human spreadsheet compatibility.

**Alternatives considered**:

- PDF only—rejected for automation and diffing.

## 9. Testing approach

**Decision**: Unit tests on Domain/Application (pot, milestones, capital gate, practice-mode warning predicates, one-entry-per-calendar-date rule). Infrastructure tests: EF mappings, Identity user isolation, and query filters with SQLite. **No AppHost tests**. **No Playwright or browser E2E** in this feature—validation via [quickstart.md](./quickstart.md) manual scenarios and automated unit/integration tests only.

**Rationale**: Matches owner testing standards, constitution IV, and explicit out-of-scope for E2E automation at this stage.

**Alternatives considered**:

- Playwright C# E2E—deferred / out of scope (owner).
- AppHost integration tests—explicitly excluded by owner.

## 10. HTTP clients and resilience

**Decision**: No external HTTP in MVP. If added (e.g. email provider), use **typed clients** and **`AddHttpClient<T>`** with resilience configured on the handler pipeline in the host.

**Rationale**: Owner pattern requirements; YAGNI for MVP.
