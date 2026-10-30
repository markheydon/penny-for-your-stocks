# UI stack contract

**Feature**: `001-written-trial-log`  
**Version**: 0.1 (planning)

Owner policy: **Tailwind CSS**, **Lucide** icons, **Chart.js** for charts. Do **not** adopt MudBlazor, Fluent UI, or other full component libraries unless explicitly re-scoped.

## Blazor hosting

- **Project**: `PennyForYourStocks.Web` (Aspire-managed).
- **Render mode**: Interactive Server preferred for MVP (Identity cookies, passkey ceremony, lower WASM payload). Revisit Auto/WASM only if latency or hosting constraints appear.
- **Pages**: Trial dashboard, weekly log editor (one entry per calendar date), configuration, milestone reviews, export, account/passkey management (Identity template pages adapted to Tailwind).

## Tailwind CSS

- Tailwind v4 or v3 per toolchain chosen at implement time; single source stylesheet input built into `wwwroot/css/app.css`.
- Design tokens: neutral greys, accessible contrast, UK English copy; no share-ticker or “trading platform” visual language.
- Responsive layout for phone + desktop (weekly check on mobile).

## Lucide icons

- Use Lucide icon SVGs for navigation (log, settings, export, reviews), warnings (practice mode, capital gate), and passkey prompts.
- Prefer inline SVG or a minimal icon component; avoid mixing other icon sets.

## Chart.js

- **In scope**: Bar or line charts for **observed cash withdrawn** over time and comparison band to £100–£200 hope (labelled observation vs hope, not targets).
- **Out of scope**: Price charts, candlesticks, or any market data visualisation.
- Load Chart.js once; small `IJSRuntime` module for init/update/dispose on parameter change.

## Accessibility

- Passkey flows must remain keyboard- and screen-reader friendly (Identity template baseline + Tailwind focus states).
- Chart summaries duplicate key numbers in text for SC-002/SC-003 reviewers who do not rely on graphics.

## Asset pipeline

- **Tailwind**: `Tailwind.MSBuild` on `PennyForYourStocks.Web` compiles `Styles/app.css` → `wwwroot/css/app.css` on `dotnet build` using the standalone Tailwind CLI (no Node/npm). CI relies on the same build step.
