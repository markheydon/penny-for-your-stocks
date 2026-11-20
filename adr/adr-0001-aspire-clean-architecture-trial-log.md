---
title: "ADR-0001: Aspire orchestration and clean architecture"
status: "Accepted"
date: "2026-10-30"
authors: "Project owner"
tags: ["architecture", "aspire", "clean-architecture"]
supersedes: ""
superseded_by: ""
---

# ADR-0001: Aspire orchestration and clean architecture

## Status

**Accepted**

## Context

The trial log needs testable domain rules, a signed-in web application, and infrastructure that is easy to run and observe locally. The owner requires **.NET Aspire** for orchestration: resources are declared in the AppHost and consumed by application code through Aspire-supplied configuration and dependency injection, not ad hoc connection strings or parallel bootstrap in `Program.cs`.

A **clean architecture** layout keeps domain rules independent of EF Core, ASP.NET, and UI so pot maths, milestones, and policy logic stay unit-testable and persistence can change without rewriting the core model.

## Decision

1. **Aspire AppHost** is the sole place that models runnable resources (database, web project, migration jobs, references, and wait/order). Application and test projects do not start or configure infrastructure themselves.
2. **Multi-project clean architecture**: AppHost, ServiceDefaults, Web, Domain, Application, and Infrastructure—with dependency direction inward (Domain has no infrastructure or web references).
3. **ServiceDefaults** on the Web project provides shared OpenTelemetry, health checks, and HTTP resilience defaults consistent with Aspire guidance.

## Consequences

### Positive

- **POS-001**: Domain and application logic can be tested without EF or ASP.NET.
- **POS-002**: Local and CI behaviour stays aligned when the AppHost declares how services start and in what order.
- **POS-003**: Swapping persistence or hosting integrations is bounded to AppHost and Infrastructure rather than scattered across the solution.

### Negative

- **NEG-001**: More projects and wiring than a single Blazor monolith.
- **NEG-002**: Developers need the Aspire CLI (and bundle) for AppHost builds and the intended local run path (`aspire run` / AppHost), not Web alone.
- **NEG-003**: Architectural boundaries must be enforced in review; nothing in the toolchain prevents layer violations without discipline.

## Alternatives Considered

### Monolithic Blazor Web project

- **ALT-001**: **Description**: One project for UI, APIs, EF, and domain rules.
- **ALT-002**: **Rejection Reason**: Couples financial rules to persistence and complicates isolated testing and provider changes.

### Manual connection strings without Aspire

- **ALT-001**: **Description**: Committed or hand-wired `ConnectionStrings` and `AddDbContext` only in Web, with no AppHost resource model.
- **ALT-002**: **Rejection Reason**: Duplicates orchestration and conflicts with the Aspire-first requirement.

## References

- [CONVENTIONS.md](../CONVENTIONS.md) — project layout and coding norms
