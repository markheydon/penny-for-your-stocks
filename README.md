# Penny For Your Stocks

A personal application to run a **written trial with a stop rule** for modest active share dealing: log weekly checks, record spread and dealing costs explicitly, review milestones at about 3, 12, and 18 months, and decide from your own record whether the attempt is clearly just losing money—not from stock tips or automated trading.

Built primarily because apparently simply asking shares to go up isn't a viable investment strategy.

The project is at an early stage. This repository currently contains the .NET solution and test scaffolding; the trial log application is not yet usable. It is experimental, makes no promise of returns, and treats withdrawing about £100–£200 a month as a hope that may never be met.

## Development

```bash
dotnet build PennyForYourStocks.slnx -c Release -warnaserror
dotnet test PennyForYourStocks.slnx -c Release --no-build
```

## Repository layout

| Path | Purpose |
|------|---------|
| `PennyForYourStocks.slnx` | .NET solution (application and tests) |
| `src/PennyForYourStocks/` | Application code |
| `tests/PennyForYourStocks.Tests/` | Automated tests |
| `GOALS.md`, `SCOPE.md`, `CONVENTIONS.md` | Product and coding context |
| `AGENTS.md`, `.cursor/rules/` | AI agent operating rules |
| `.agents/skills/` | Project-specific agent workflows |
| `plan/` | Release and PR policy |
| `adr/` | Architectural decision records |

## Licence

MIT — see [LICENSE](LICENSE).
