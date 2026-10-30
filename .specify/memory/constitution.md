<!--
Sync Impact Report
- Version change: (unratified template) → 1.0.0
- Modified principles: N/A (initial adoption from placeholder template)
- Added sections: Core Principles (5), Technology and Security Constraints,
  Development Workflow and Quality Gates, Governance
- Removed sections: Template placeholders and example comments
- Follow-up TODOs: None
-->

# Penny For Your Stocks Constitution

## Core Principles

### I. Security-First (NON-NEGOTIABLE)

Security considerations MUST be addressed before feature completeness. Authentication,
authorisation, and protection of signed-in trial data and personal financial notes MUST
follow least privilege and fail closed on errors. Secrets, API keys, and credentials MUST
NOT be committed; suspected vulnerabilities MUST be reported privately per `SECURITY.md`.

**Rationale:** The product stores private trial logs tied to signed-in accounts; a breach
would expose sensitive personal financial notes and undermine trust in the experiment record.

### II. Validated Input at Boundaries

All user-supplied input MUST be validated at system boundaries (HTTP APIs, forms, file
imports, and configuration overrides). Validation MUST enforce type, range, length, and
allowed-character rules before use in domain logic or persistence. Reject invalid input
with clear, non-leaking error responses; never trust client-side checks alone.

**Rationale:** Invalid or malicious input is the primary attack surface for data corruption,
injection, and logic abuse in a user-facing log application.

### III. Documented Public Surface

Every public API member (types, methods, properties, and endpoints exposed to other
assemblies or clients) MUST carry accurate XML documentation or equivalent structured
docs. Internal implementation MUST remain readable through clear naming and structure;
comments MUST explain non-obvious business rules (trial milestones, cost rules, stop
criteria) rather than restating code. User-facing copy MUST use UK English.

**Rationale:** The project is a long-lived hobby build; documentation keeps agents, future
contributors, and the owner aligned with trial boundaries without re-reading every file.

### IV. Test Discipline

Automated tests MUST protect behaviour that affects trial integrity: pot calculations, cost
summation, milestone timing, auth boundaries, and validation rules. Tests MUST use the
repository stack (xUnit v3, built-in asserts, NSubstitute when mocks are required) per
`CONVENTIONS.md`. New behaviour MUST ship with tests that would fail if the behaviour
regressed; flaky or order-dependent tests MUST be fixed or removed.

**Rationale:** Stop-or-continue decisions depend on a durable, correct log; untested
financial and milestone logic creates silent wrong conclusions.

### V. Maintainable Code Quality

Code MUST compile with warnings treated as errors, nullable reference types enabled, and
async I/O propagated with `CancellationToken` on public async APIs. Complexity MUST be
justified in review; prefer small, focused types and methods over speculative abstraction
(YAGNI). Significant architecture or stack choices MUST be recorded in `adr/` before
implementation diverges from prior decisions.

**Rationale:** Maintainability keeps scope bounded for a part-time hobby while still
meeting “properly built” expectations from `GOALS.md`.

## Technology and Security Constraints

- **Stack:** Only LTS versions of.NET, such as the current .NET 10 as defined in repository tooling; central package versions
  in `Directory.Packages.props` when dependencies change.
- **Scope:** Product features MUST stay within `SCOPE.md` and `GOALS.md` (no share
  recommendations, broker order execution, or live market data unless explicitly re-scoped).
- **Sensitive data:** Trial logs and account data MUST remain private to the signed-in user;
  logging and telemetry MUST NOT include secrets or full personal financial detail.
- **Dependencies:** Do not add or remove NuGet packages without explicit human approval
  except routine central version maintenance per `AGENTS.md`.

## Development Workflow and Quality Gates

Before merge, changes MUST pass:

```bash
dotnet format PennyForYourStocks.slnx --verify-no-changes
dotnet build PennyForYourStocks.slnx -c Release -warnaserror
dotnet test PennyForYourStocks.slnx -c Release --no-build
```

Pull requests MUST follow `plan/PULL_REQUEST_POLICY.md` and `.github/PULL_REQUEST_TEMPLATE.md`.
Human review is required for all agent-authored pull requests. Reviewers MUST verify
constitution principles relevant to the change (security, validation, tests, documentation).

Runtime coding detail beyond these gates lives in `CONVENTIONS.md` and `AGENTS.md`; where
those files conflict with this constitution, this constitution wins until amended.

## Governance

This constitution supersedes informal habit for governance questions (security, quality,
testing, and documentation expectations). Amendments MUST update `.specify/memory/constitution.md`,
bump `CONSTITUTION_VERSION` per semantic versioning, set `LAST_AMENDED_DATE`, and summarise
impact in the Sync Impact Report comment until the amendment is committed.

- **MAJOR:** Removal or incompatible redefinition of a core principle.
- **MINOR:** New principle or materially expanded section.
- **PATCH:** Clarifications and non-semantic wording.

Compliance SHOULD be checked during specification (`spec.md`), planning, implementation,
and pull request review. Deferred or intentional non-compliance MUST be documented with
rationale and a remediation plan.

**Version**: 1.0.0 | **Ratified**: 2026-10-29 | **Last Amended**: 2026-10-29
