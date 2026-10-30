# Goals

**Project:** Penny For Your Stocks
**Owner:** Mark Heydon
**Last updated:** 2026-10-29

---

## Why This Exists

Simply hoping that shares will go up is not a viable investment strategy. The personal question behind this repository is whether active share dealing, with a small ring-fenced sum that can be lost, could ever support withdrawing about £100–£200 a month—and whether that hope is worth testing at all.

It is a hobby and learning project. The curiosity is whether modest supplementary income is achievable from low-outlay share dealing. It is not a belief that it will make anyone rich. Like the National Lottery Generator, it is semi lighthearted but properly built, and it is not a novelty app. The realistic chance of anyone making a good sum from it is close to zero, and the project says so plainly. On an opening pot of about £50, ordinary long-run market returns are a few pounds a year, not hundreds a month; finding that out is part of the point.

The immediate product is an application that helps run a **written trial with a stop rule**: a durable log of weekly checks, explicit costs, milestone reviews, and a plain-language decision to stop or continue—not stock tips or automated dealing.

---

## Goals

- Build an application that records each trial check (what was considered, what was held, spread and dealing costs, and optional stamp duty or foreign-exchange fees) so stop-or-continue can be judged from the log, not memory.
- Reflect agreed trial boundaries in the app: real shares only; no leverage, contracts for difference, spread betting, or borrowed money; UK, United States, and European markets with controversial sectors such as weapons excluded; intended holds of a few days to about a week; checks at least weekly (daily only when chosen).
- Track a running trial pot from the opening ring-fenced amount (about £50 by default), user-recorded top-ups after the short-term review, withdrawals, and summed costs—without live share prices, market data feeds, or broker balances.
- Surface milestone reviews at about 3 months (short-term review before any extra trial capital is even considered), about 12 months (stop if clearly just losing money), and about 18 months (follow-up).
- Compare any cash withdrawn with the £100–£200 monthly hope as **observed cash only**, never as a budget, profit target, or reason to raise stakes after a good month.
- Require sign-in and synchronise the log across devices; keep trial data private to the signed-in account.
- Warn when broker practice-mode results are treated as the official trial outcome without cost adjustments aligned to real spread and tax.
- **Do not** recommend, rank, screen, or signal individual shares; **do not** execute trades or connect to brokerage accounts for orders.

---

## Success Looks Like

- The user can complete a typical weekly log entry in a few minutes after initial familiarisation.
- At about 12 months, a single in-app summary supports a stop-or-continue judgement, total observed withdrawals, the derived trial pot view (cash and costs only), and hope-versus-observation comparison without relying on external spreadsheets.
- A clear “the income hope has failed” or “still open but not proven” outcome is reachable from the stored record. Treating a short profitable run as a durable edge is explicitly discouraged in the product copy.
- Building a separate stock analysis application, Microsoft Foundry experiments, or automated trading remains **out of scope** until a later explicit decision; this repository’s current focus is the trial log only.

---

## Kill Criteria

- The project becomes too complicated or takes up too much time. It is a hobby and learning exercise, and there is no time to develop it full time.
- The trial is clearly just losing money at the about-12-month review, or the user records an early stop with that reasoning.
- Pursuing it would mean risking money that is needed for essentials, adding trial capital before the short-term review without treating that as out of scope, or raising stakes to chase the monthly hope.
- Scope creep into market data feeds, screeners, broker adapters, or share recommendations before the trial record has done its job.

---

## What This Is NOT For

(See also: SCOPE.md)

- Promising returns, financial advice, or a guaranteed or likely income source.
- Choosing a broker, setting platform fees or limits, or replacing tax or platform guidance.
- Leveraged products or speculation with money that cannot be lost.
- Proving a repeatable trading edge from 12–18 months of one person’s trades alone.

---

## Revision History

| Date | Change | Reason |
|---|---|---|
| 2026-10-29 | Adapted project goals from the repository template | Establish the initial product direction |
| 2026-10-29 | Added motivation, learning goals, tone, paper-first approach and kill criteria | Capture the owner's intent and brainstorm outcomes |
| 2026-10-29 | Refocused on a written trial log application with stop rule and deferred analysis or trading tooling | Align repository goals with the bounded personal experiment |
| 2026-10-29 | Repository-wide template sweep (solution name, labels, issue templates) | Remove solo-dev-board residue and fix PennyForYourStocks.slnx alignment |
