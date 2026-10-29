> Coding and design conventions for this repository.

# Conventions

**Project:** Penny For Your Stocks
**Last updated:** 2026-10-29

When in doubt, follow this file. Significant architecture changes need an ADR.

---

## Project Structure

```
src/
└── PennyForYourStocks/

tests/
└── PennyForYourStocks.Tests/
```

Add folders and projects as the product grows. Update this section when the layout changes.

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
