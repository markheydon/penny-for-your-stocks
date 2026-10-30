---
title: "ADR-0001: Aspire, clean architecture, and Identity passkeys for trial log"
status: "Accepted"
date: "2026-10-30"
authors: "Project owner"
tags: ["architecture", "aspire", "identity", "ef-core"]
supersedes: ""
superseded_by: ""
---

# ADR-0001: Aspire, clean architecture, and Identity passkeys for trial log

## Status

**Accepted**

## Context

The written trial log feature requires a cloud-backed, signed-in web application with per-user data isolation, derived cash-only pot maths, milestone reviews, and export—without share recommendations or broker integration. The owner requires .NET Aspire orchestration (no hand-rolled connection strings), clean architecture for testable domain rules, SQLite initially with a PostgreSQL swap path, ASP.NET Core Identity with passkeys on .NET 10, and a Blazor Web App with Tailwind CSS.

## Decision

Adopt a multi-project layout: **AppHost** (Aspire orchestration), **ServiceDefaults** (OTel, health, resilience), **Web** (Blazor + minimal APIs), **Domain**, **Application**, and **Infrastructure** (EF Core, Identity, repositories). Wire SQLite via `AddSqliteDbContext<TrialDbContext>` and AppHost `WithReference`. Use Identity schema version 3 with passkeys; global EF query filters on `UserId`. Retire the placeholder single class library.

## Consequences

### Positive

- **POS-001**: Domain pot and milestone logic unit-testable without EF or ASP.NET.
- **POS-002**: Aspire supplies connection configuration, health checks, and local run via `aspire run`.
- **POS-003**: Provider swap (SQLite → PostgreSQL) isolated to Infrastructure and AppHost packages.

### Negative

- **NEG-001**: More projects and wiring than a monolithic Web app.
- **NEG-002**: Package surface (Aspire, EF, Identity) requires ongoing version alignment.
- **NEG-003**: Passkey RP ID and `ServerDomain` must be configured explicitly for non-local hosts.

## Alternatives Considered

### Monolithic Blazor Web project with DbContext in handlers

- **ALT-001**: **Description**: Single Web project containing UI, API, EF, and domain rules.
- **ALT-002**: **Rejection Reason**: Couples financial rules to persistence; harder PostgreSQL swap and unit testing.

### Manual connection strings without Aspire

- **ALT-001**: **Description**: `appsettings.json` only for database connectivity.
- **ALT-002**: **Rejection Reason**: Duplicates orchestration and conflicts with owner Aspire requirement.

### Microsoft Entra External ID for authentication

- **ALT-001**: **Description**: Hosted identity with OIDC.
- **ALT-002**: **Rejection Reason**: Operational overhead disproportionate for hobby single-user v1; Identity passkeys sufficient.

## References

- `specs/001-written-trial-log/plan.md`
- `specs/001-written-trial-log/research.md`
