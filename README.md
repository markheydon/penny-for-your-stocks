# Penny For Your Stocks

An experimental stock analysis and trading application that attempts to identify modest trading opportunities using market data, a bit of logic and an appropriate amount of scepticism.

Built primarily because apparently simply asking shares to go up isn't a viable investment strategy.

The project is at an early stage. This repository currently contains the .NET solution and test scaffolding; it is not yet a usable trading system. It is experimental and makes no promise of returns.

## Development

```bash
dotnet build Repo.slnx -c Release -warnaserror
dotnet test Repo.slnx -c Release --no-build
```

## Repository layout

| Path | Purpose |
|------|---------|
| `src/PennyForYourStocks/` | Application code |
| `tests/PennyForYourStocks.Tests/` | Automated tests |
| `GOALS.md`, `SCOPE.md`, `CONVENTIONS.md` | Product and coding context |
| `AGENTS.md`, `.cursor/rules/` | AI agent operating rules |
| `.agents/skills/` | Project-specific agent workflows |
| `plan/` | Release and PR policy |
| `adr/` | Architectural decision records |

## Licence

MIT — see [LICENSE](LICENSE).
