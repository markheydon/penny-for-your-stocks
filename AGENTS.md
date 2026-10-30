# Agent Instructions

Repository-specific operating rules for AI coding agents.

## Core Context

Read before non-trivial changes:

- `GOALS.md`
- `SCOPE.md`
- `CONVENTIONS.md`
- `adr/` — accepted decisions

## Language

UK English in documentation, comments, and user-facing text.

## Tech Stack

- .NET 8.0 and .NET 10.0.
- xUnit v3 with Microsoft.Testing.Platform; NSubstitute when mocks are required; built-in asserts only.

## Skills

Project skills live in `.agents/skills/`.

| Skill | Use when |
|---|---|
| `code-review` | PR or branch diff review before merge (`/code-review` or [loop-code-review.md](.cursor/commands/loop-code-review.md)) |
| `create-architectural-decision-record` | Creating or major-updating an ADR |
| `documentation-writer` | Diátaxis-aligned documentation |
| `project-documentation` | Project-aware docs placement |

## Vendor-managed tool assets (confirm first)

Spec Kit and Aspire install skills, scripts, and config that **their installers overwrite on upgrade**. Manually editing those files is usually wasted work; a request to change them is often a mistake.

**Before editing** any of the paths below, **stop and ask** whether that is really what the user meant. Do not proceed until they confirm explicitly (for example, a one-off hotfix with acceptance that the change may be lost on the next tool upgrade).

| Source | Paths (any matching location, e.g. `.cursor/skills/`, `.agents/skills/`) |
|---|---|
| Spec Kit skills | `**/skills/speckit-*` |
| Aspire skills | `**/skills/aspire*`, plus Aspire-bundled `**/skills/dotnet-inspect` and `**/skills/playwright-cli` |
| Spec Kit installation | `.specify/` — scripts, templates, workflows, integrations, extensions, and other tool-managed files (not feature specs under `specs/`) |

Use the installed tools’ own upgrade or configuration flows to change behaviour instead of patching vendor copies in the repo.

## Cursor commands (Spec Kit and review)

Project-specific wrappers live in [`.cursor/commands/`](.cursor/commands/). Prefer invoking them over raw `/speckit-*` skills when you need **Penny For Your Stocks** branch, verification, PR, or loop behaviour in the same run. They reference Spec Kit skills under `.cursor/skills/` but do not replace them.

| Command | Use when |
|---|---|
| [do-analyse-after-tasks.md](.cursor/commands/do-analyse-after-tasks.md) | Optional: after `/speckit-tasks`, before code — artifact audit loop (no `src/`/`tests/` changes) |
| [do-implement.md](.cursor/commands/do-implement.md) | After `tasks.md` exists — implement the active feature under `specs/`, format/build/test, commit/push, open or update PR |
| [do-converge-after-implement.md](.cursor/commands/do-converge-after-implement.md) | Optional: after implement — converge loop (gap tasks → implement → verify) |
| [loop-code-review.md](.cursor/commands/loop-code-review.md) | Feature branch is implemented — convention-led review/fix loop before or during PR polish |

Typical Spec Kit flow for a feature:

```text
/speckit-constitution → /speckit-specify → /speckit-plan → /speckit-tasks
→ (optional) do-analyse-after-tasks
→ do-implement
→ (optional) do-converge-after-implement
→ (optional) loop-code-review
```

Several commands grant explicit permission to **commit** or **push** on the feature branch where noted; otherwise follow the user’s commit rules.

## Task Routing

- **Code** (`src/`, `tests/`): `CONVENTIONS.md`.
- **Documentation** (`**/*.md` except `adr/`): documentation skills.
- **ADRs** (`adr/*.md`): ADR skill only.

## Not Allowed Without Explicit Instruction

- Add or remove NuGet packages (except central versions in `Directory.Packages.props` when maintaining deps).
- Modify CI/CD behaviour.
- Commit secrets or API keys.

## Pull Request Workflow

Follow [plan/PULL_REQUEST_POLICY.md](plan/PULL_REQUEST_POLICY.md) and [`.github/PULL_REQUEST_TEMPLATE.md`](.github/PULL_REQUEST_TEMPLATE.md).

Before opening a PR:

```bash
dotnet format PennyForYourStocks.slnx --verify-no-changes
dotnet build PennyForYourStocks.slnx -c Release -warnaserror
dotnet test PennyForYourStocks.slnx -c Release --no-build
```

Title shape: `[Type] Imperative summary (#NNN)` with labels from [plan/LABEL_STRATEGY.md](plan/LABEL_STRATEGY.md).

All agent-authored pull requests require human review before merge.
