# Feature Specification: Written trial log with stop rule

**Feature Branch**: `001-written-trial-log`

**Created**: 2026-10-29

**Status**: Draft

**Input**: User description: "Use the handoff summary in .specify/assessments/modest-trading-income/decision.md to specify the approved written trial with a stop rule. The written trial will be via an app to record the details."

## Clarifications

### Session 2026-10-29

- Q: Where should the trial log primarily live so you can use it each week and still rely on it at the 12-month review? → A: Cloud account with sign-in and sync across devices.
- Q: For each weekly check, should you create one log entry for the whole check or a separate entry for each buy, sell, or hold change? → A: One entry per check (one dated record per check, with multiple holdings and costs inside).
- Q: When should the app treat the “short-term review” as due, before you may even consider adding trial capital beyond about £50? → A: 3 months after trial start.
- Q: Should the app keep a running trial pot figure for you, or only store what you write in each check without automatic totals? → A: App tracks opening £50 plus recorded top-ups (after 3-month gate), withdrawals, and summed costs—no share prices.
- Q: Must you export a full backup of the trial log before the app lets you finish a milestone review (3, 12, or 18 months)? → A: Export available anytime; never required to complete a review.
- Q: When you open the app for a second check on the same calendar date, what should it do by default before you choose anything else? → A: Reopen the existing same-day entry so you add to one primary record.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record each trial check in the log (Priority: P1)

As a UK retail investor running a small ring-fenced trial, I open the app at least once a week (or daily if I choose) and add a log entry that captures what I considered, what I held, and an explicit allowance for spread and dealing costs, so that later I can judge stop-or-continue from my own record rather than memory.

**Why this priority**: Without a durable, line-by-line record the trial cannot produce the personal pass/fail evidence the assessment approved; everything else depends on consistent logging.

**Independent Test**: Can be fully tested by creating entries over several simulated weeks, exporting or reviewing the history, and confirming each entry shows consideration, holdings, and cost allowance without any share suggestions from the app.

**Acceptance Scenarios**:

1. **Given** the trial has started with an opening ring-fenced sum of about £50, **When** I complete a weekly check and save a new entry, **Then** the entry is stored with date, what was considered, what was held, and stated spread and dealing costs (and stamp duty or foreign-exchange fees where I record them).
2. **Given** I used a broker practice mode, **When** I try to mark that session as the official trial result without cost adjustments, **Then** the app warns me that practice results that omit spread and tax are not the trial result and I must record adjusted figures or note the limitation in plain language.
3. **Given** I am logging activity, **When** I use the app, **Then** it does not recommend, rank, or suggest specific shares or screens.

---

### User Story 2 - Keep trial boundaries visible (Priority: P2)

As the investor, I want the app to reflect the trial rules I agreed (real shares only, no leverage or derivatives, allowed markets, excluded controversial sectors, intended hold length of a few days to about a week) so I do not accidentally treat out-of-scope activity as part of the trial.

**Why this priority**: The written trial is bounded; mixing ISA long-term holdings, leveraged products, or off-scope instruments would invalidate the stop-or-continue judgement.

**Independent Test**: Can be tested by configuring trial parameters once and verifying that log entries can be flagged or annotated when activity falls outside stated boundaries, without blocking personal note-taking.

**Acceptance Scenarios**:

1. **Given** trial settings state real shares only in the UK, United States, or Europe with no weapons-related holdings, **When** I record an entry, **Then** I can note holdings and mark whether they comply with those boundaries.
2. **Given** the opening sum is about £50 and must stay there until a short-term review at 3 months, **When** I attempt to record additional trial capital before that review completes, **Then** the app prompts me that adding capital early is out of trial scope and asks me to confirm I am only noting intent or external action, not approving it as part of the trial plan.
3. **Given** money outside the trial remains in my existing stocks and shares ISA, **When** I view trial summary, **Then** trial pot figures are separate from any note that ISA money is out of scope for this log.

---

### User Story 3 - Apply the stop rule and compare income hope (Priority: P3)

As the investor, when I reach about 12 months of the trial (and again by about 18 months), I use the app to record a plain-language stop-or-continue judgement, compare any cash withdrawn against my £100–£200-a-month hope as observed cash only, and decide whether further capital is even worth considering.

**Why this priority**: The stop rule and hope comparison are the success outcomes of the trial; logging alone is insufficient without structured review milestones.

**Independent Test**: Can be tested by setting trial start date, advancing to review milestones (or simulating dates), recording judgements, and verifying summaries show hope versus observed withdrawals without claiming a durable edge.

**Acceptance Scenarios**:

1. **Given** about 12 months have elapsed since trial start, **When** I open the review flow, **Then** I can record whether active dealing is clearly just losing money and whether I stop or continue, in my own words, with the app surfacing that “clearly just losing money” is plain language (the £100 figure in project materials illustrates that sentence, not a loss budget or profit target).
2. **Given** 3 months have elapsed since trial start, **When** I complete the short-term review, **Then** I can record whether the trial is already a clear failure and whether adding capital remains out of scope, without the app deciding for me.
3. **Given** I have withdrawn cash during the trial, **When** I complete a milestone review, **Then** the app shows observed monthly cash withdrawn alongside the £100–£200 hope and whether I still consider the hope open or failed, without treating a profitable stretch as proof of a repeatable edge.
4. **Given** the short term is already a clear failure, **When** I record a milestone review, **Then** the app reflects that adding further trial capital is out of scope unless I explicitly document a later personal decision outside the original trial bounds.

---

### Edge Cases

- What happens when a weekly check is missed? The app should allow a late entry with the actual check date and show gaps in the check history without auto-judging failure.
- How are zero-activity weeks handled? User can log one check entry with “no trade” consideration notes and costs of zero or platform fees only.
- What if the user checks twice on the same day? The app keeps one primary entry per calendar check date; starting another check on that date reopens that entry for editing and additions rather than creating a second entry or separate action-level rows.
- What if the user records a highly profitable month? The app summarises observed cash but displays a fixed reminder that 12–18 months of one person’s trades cannot prove persistence or justify larger stakes on their own.
- What if spread or tax was estimated? Entry remains valid if the user documents that figures are estimates and revises later if needed.
- What if the user wants to stop before 12 months? The app allows an early stop judgement with reason, without replacing the scheduled 18-month optional review unless the user records one.
- What if the user logs in from a second device after an offline session on the first? Sync resolves to one chronological log without duplicate milestone reviews; conflicts surface for the user to reconcile before summaries are finalised. **v1** assumes an online authoritative store (no offline queue); deferred offline behaviour is documented in [research.md](./research.md) §4.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST let the user create, view, edit, and chronologically list trial log entries, each tied to a check date, with exactly one primary entry per calendar check date; when the user starts another check on a date that already has an entry, the app MUST reopen that entry for editing and additions instead of creating a second entry.
- **FR-002**: Each log entry MUST capture at minimum: what was considered, what was held during that check (one or more holdings, identifiers or names as the user enters them), and explicit allowances for spread and dealing costs (per holding or rolled up for the check, as the user records); optional fields for stamp duty, foreign-exchange fees, and free-text notes.
- **FR-003**: The app MUST NOT provide share recommendations, rankings, screening, or automated trade signals.
- **FR-004**: The app MUST let the user set and display trial boundaries: opening ring-fenced sum (default expectation about £50), real shares only, no leverage or derivatives, permitted regions (UK, US, Europe), and excluded controversial areas such as weapons, plus intended hold duration guidance (a few days to about a week). Until the short-term review at 3 months is recorded, the app MUST treat adding trial capital as out of scope except as a user-noted external intent.
- **FR-005**: The app MUST distinguish trial pot activity from money documented as held outside the trial (for example existing ISA investments) via labelling or separate notes, without integrating to a broker.
- **FR-006**: The app MUST warn when the user marks practice-mode or simulated fills as official trial results without cost adjustments aligned to real spread and tax.
- **FR-007**: The app MUST track trial start date and surface review milestones at 3 months (short-term review, including whether adding capital may be considered), about 12 months (stop-or-continue judgement), and about 18 months (follow-up review).
- **FR-008**: The app MUST let the user record stop-or-continue and early-stop judgements in plain language and store the decision date.
- **FR-009**: The app MUST let the user record cash withdrawals observed during the trial and summarise average or monthly observed cash against the stated £100–£200 monthly hope, labelled as hope versus observation, not as targets or budgets.
- **FR-015**: The app MUST maintain a running trial pot view derived only from the opening ring-fenced amount, user-recorded top-ups after the 3-month short-term review gate, user-recorded withdrawals, and the sum of spread, dealing, and optional tax or FX costs recorded across check entries—without live share prices, market data feeds, or broker balances.
- **FR-010**: The app MUST display a persistent reminder that a profitable period within 12–18 months is not evidence of a durable edge and must not be used to justify increased stakes without a separate future decision.
- **FR-011**: The app MUST persist all trial data so the user can answer, from the log alone, whether the income hope remains open or has failed at milestone reviews.
- **FR-012**: The app MUST NOT execute trades, connect to brokerage accounts for order placement, or replace tax, broker, or platform advice.
- **FR-013**: The app MUST require the user to sign in to a personal account and MUST synchronise trial data across the user’s signed-in devices so weekly checks and milestone reviews see the same log.
- **FR-014**: The app MUST restrict each account to a single trial owner; trial data MUST NOT be visible to other users.
- **FR-016**: The app MUST let the user export the full trial log and reviews at any time; export MUST NOT be a prerequisite for completing any milestone review.

### Key Entities *(include if feature involves data)*

- **Trial configuration**: Start date, opening ring-fenced amount, boundary rules (instruments, regions, exclusions), check cadence expectation (at least weekly), documented separation from non-trial holdings, and recorded top-ups after the 3-month gate.
- **Log entry**: Check date, consideration narrative for the whole check, one or more holdings touched or held during that check, cost allowances (spread, dealing, optional tax or FX, per holding or aggregated), compliance notes with trial boundaries, optional link to practice-mode disclaimer, and optional user-recorded withdrawal amounts attributed to that check.
- **Trial pot snapshot (derived)**: Running figure from opening amount, gated top-ups, summed costs, and withdrawals—explicitly not mark-to-market.
- **Cash observation**: Date or period, amount withdrawn, user notes; aggregated views for comparison to the monthly hope band.
- **Milestone review**: Scheduled type (3-month short-term review, 12-month stop/continue, 18-month review, or early stop), decision date, plain-language judgement, hope status (open or failed) where applicable, whether further capital is considered in scope, and outcome of the short-term capital gate when relevant.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The user can complete a weekly log entry (consideration, holdings, costs) in under 10 minutes after the first use, for at least 95% of checks in a 3-month observation of personal use.
- **SC-002**: At the 12-month milestone, the user can produce from the app a single summary that includes stop-or-continue judgement text, total observed withdrawals, derived trial pot view (cash and costs only, not share prices), and hope comparison without using external spreadsheets.
- **SC-003**: By the 18-month milestone (if the trial continues), the user can produce an updated review record that states whether the hope is still open or has failed, referencing only app-stored entries and reviews.
- **SC-004**: Zero in-app prompts or flows offer specific shares to buy or sell; independent review of all user-visible copy finds no recommendation language.
- **SC-005**: When practice-mode results are flagged, 100% of official trial summaries either use cost-adjusted figures or include an explicit user acknowledgement that figures omit real spread or tax.

## Assumptions

- A single primary user (the project owner) uses the app; multi-user accounts and sharing are out of scope unless added in a later feature.
- The app replaces notebook or spreadsheet for the trial record but does not replace broker platforms, tax filing, or professional advice.
- The opening ring-fenced sum remains about £50 until the short-term review at 3 months is recorded; the app enforces this as policy messaging, not as a bank lock.
- “Clearly just losing money” remains subjective plain language; the app structures judgement capture but does not algorithmically decide stop or continue.
- The £100–£200 monthly figure is an hope band for comparison only, not a loss limit, profit guarantee, or stake-sizing rule.
- Building the broader stock analysis application, Microsoft Foundry experiments, and automated trading remain out of scope until a later project decision, per assessment handoff.
- Same-day dealing as a requirement, penny shares as a requirement, broker selection, platform fee negotiation, and leverage products remain out of scope.
- Network connectivity is available for routine weekly use; the authoritative store is the user’s cloud-backed account, with offline entry and later sync acceptable if planning supports it.
- Trial data is private to the signed-in account; export for personal backup is available on demand and is never required to complete a review; public sharing is not.
