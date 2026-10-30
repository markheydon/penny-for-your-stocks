> Coding and design conventions for this repository.

# Conventions

**Project:** Penny For Your Stocks
**Last updated:** 2026-10-30

When in doubt, follow this file. Significant architecture changes need an ADR.

Product boundaries (share recommendations, broker integration, market data feeds, and related exclusions) are defined in `SCOPE.md`. User-facing copy and features must stay within those limits.

---

## Project Structure

Clean architecture with Aspire orchestration. Layering and persistence choices are summarised in [adr/adr-0001-aspire-clean-architecture-trial-log.md](adr/adr-0001-aspire-clean-architecture-trial-log.md).

```
src/
├── PennyForYourStocks.AppHost/          — Aspire AppHost (SQLite, migrations job, Web)
├── PennyForYourStocks.ServiceDefaults/  — OpenTelemetry, health checks, resilience
├── PennyForYourStocks.Web/              — Blazor UI, Identity passkeys, minimal APIs
├── PennyForYourStocks.Domain/           — Trial rules and calculators (no outer-layer refs)
├── PennyForYourStocks.Application/      — Use cases and export
└── PennyForYourStocks.Infrastructure/   — EF Core, Identity, repositories, migrations

tests/
├── PennyForYourStocks.Domain.Tests/
├── PennyForYourStocks.Application.Tests/
└── PennyForYourStocks.Infrastructure.Tests/
```

Run locally with `aspire run` (see [README.md](README.md)). Update this section when projects or boundaries change.

---

## C# patterns

- **Async/await** for I/O; propagate `CancellationToken` on public async APIs.
- **Nullable reference types** enabled (`Directory.Build.props`).
- **Treat warnings as errors** in all projects.
- **XML documentation** on public API members.
- **HTTP outbound calls** (if any): use `IHttpClientFactory` and typed clients.

---

## Testing

- **xUnit v3** with Microsoft.Testing.Platform.
- **NSubstitute** when mocks are required.
- **Built-in `Assert` only** — no FluentAssertions, Shouldly, Moq, NUnit, or MSTest.
- Test classes: `[ClassName]Tests`. Test methods: `Method_State_Expected`.

---

## Documentation

- UK English in docs and comments.
- User-facing docs in `docs/` when the README is not enough; see `.agents/skills/project-documentation/`.
