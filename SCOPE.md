# Scope

**Project:** Penny For Your Stocks
**Last updated:** 2026-10-29

---

## In Scope — Current Direction

- A personal **trial log application** for a ring-fenced active share-dealing experiment (opening sum about £50 of money that can be lost unless and until a later short-term review says otherwise).
- **Check entries**: one primary entry per calendar check date; each captures consideration for the whole check, holdings touched or held, explicit spread and dealing costs, optional stamp duty or foreign-exchange fees, compliance notes against trial boundaries, and optional withdrawal amounts attributed to that check.
- **Trial configuration and boundaries** shown in the app: real shares only; permitted regions (UK, US, Europe); excluded controversial areas such as weapons; intended hold length of a few days to about a week; separation from money documented as outside the trial (for example existing stocks and shares ISA holdings).
- **Derived trial pot view** from opening amount, gated top-ups, summed recorded costs, and withdrawals—not mark-to-market or broker balances.
- **Milestone reviews**: short-term (about 3 months), stop-or-continue (about 12 months), and follow-up (about 18 months), plus optional early stop; plain-language judgements stored by the user, not decided by the app.
- **Hope comparison**: observed cash withdrawn versus the £100–£200 monthly hope band, labelled as hope versus observation.
- **Account model**: sign-in, per-user trial data, synchronisation across signed-in devices, export of the full log at any time (export never required to complete a review).
- **Guardrail messaging**: practice-mode limitations; reminders that a profitable stretch is not evidence of a durable edge; policy that adding trial capital before the short-term review is out of trial scope.

---

## Out of Scope

- Share recommendations, rankings, screeners, or automated trade signals.
- Trade execution, order placement, or brokerage account integration.
- Live share prices, market data feeds, or strategy back-testing engines.
- Tax, broker, or platform advice; choosing or comparing brokers; negotiating fees or limits.
- Leverage, contracts for difference, spread betting, borrowed money, or same-day dealing as a required routine.
- A mandatory daily market check (the commitment is at least weekly).
- Treating 12–18 months of one person’s trades as proof of a repeatable edge.
- Adding trial capital before the short-term review except as user-noted external intent flagged out of scope.
- Penny shares as a hard requirement (they may be a personal starting interest only).
- Multi-user accounts, sharing trial logs with others, or public publishing of trial data.
- A broader stock analysis application, Microsoft Foundry work, or automated trading—deferred until a later project decision explicitly revisits them.

---

## Revision History

| Date | Change | Reason |
|---|---|---|
| 2026-10-29 | Adapted project scope from the repository template | Establish the initial product boundaries |
| 2026-10-29 | Defined in-scope trial log product and explicit exclusions | Replace undecided analysis or trading direction with the bounded experiment |
| 2026-10-29 | Repository-wide template sweep (labels, issue templates, solution file) | Remove solo-dev-board residue and align with PennyForYourStocks.slnx |
