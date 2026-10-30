# Contributing

Read [GOALS.md](GOALS.md), [SCOPE.md](SCOPE.md), [CONVENTIONS.md](CONVENTIONS.md), and [VERSIONING.md](VERSIONING.md). AI coding agents should also read [AGENTS.md](AGENTS.md).

Run the app locally with `aspire run` (see [README.md](README.md) for HTTPS certificate notes in Dev Containers).

## Development

```bash
dotnet format PennyForYourStocks.slnx --verify-no-changes
dotnet build PennyForYourStocks.slnx -c Release -warnaserror
dotnet test PennyForYourStocks.slnx -c Release --no-build
```

Active feature work under `specs/` follows Spec Kit artifacts (`spec.md`, `plan.md`, `tasks.md`); see [AGENTS.md](AGENTS.md) for agent commands.

## Pull requests

Follow [plan/PULL_REQUEST_POLICY.md](plan/PULL_REQUEST_POLICY.md) and [plan/LABEL_STRATEGY.md](plan/LABEL_STRATEGY.md).

By participating, you agree to [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
