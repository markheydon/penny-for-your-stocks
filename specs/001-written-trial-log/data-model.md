# Data model: Written trial log

**Feature**: `001-written-trial-log`  
**Date**: 2026-10-30

Logical model for EF Core persistence. Monetary amounts use **`decimal(18,2)`** (SQLite/PostgreSQL compatible). All trial-owned entities include **`UserId`** (string, Identity user id) and optional **`RowVersion`** for concurrency.

## Entity relationship overview

```text
User (Identity) 1 ── 1 TrialConfiguration
                 1 ── * LogEntry ── * LogEntryHolding
                 1 ── * CashObservation
                 1 ── * MilestoneReview
                 1 ── * TrialTopUp (after 3-month gate)
```

One **active trial configuration** per user (v1); early stop does not delete history.

---

## TrialConfiguration

| Field | Type | Rules |
|-------|------|--------|
| Id | Guid | PK |
| UserId | string | FK, unique |
| TrialStartDate | DateOnly | Required; drives milestones |
| OpeningRingFencedAmount | decimal | Default 50.00 GBP; > 0 |
| PermittedRegions | flags/string set | UK, US, Europe |
| ExcludedThemesNotes | string? | e.g. weapons exclusion narrative |
| RealSharesOnly | bool | Default true |
| NoLeverageOrDerivatives | bool | Default true |
| IntendedHoldDaysMin / Max | int | Guidance band (e.g. 2–7) |
| NonTrialHoldingsNote | string? | ISA separation note |
| ShortTermReviewCompletedAt | DateTimeOffset? | Set when 3-month review saved |
| CreatedAt / UpdatedAt | DateTimeOffset | Audit |

**Policy (application layer)**: Until `ShortTermReviewCompletedAt` is set, recording **TrialTopUp** triggers out-of-scope messaging (FR-004).

---

## LogEntry

One primary row per **calendar check date** per user (unique index on `UserId`, `CheckDate`).

| Field | Type | Rules |
|-------|------|--------|
| Id | Guid | PK |
| UserId | string | FK |
| CheckDate | DateOnly | Required |
| Consideration | string | Required; max length enforced at API |
| ComplianceNotes | string? | Boundary compliance |
| PracticeModeAcknowledged | bool | If true, user accepts figures omit real spread/tax (SC-005) |
| PracticeModeDisclaimer | string? | Plain-language limitation; non-empty implies practice/simulated use for SC-005 unless costs are fully adjusted |
| AggregatedSpreadDealingCosts | decimal? | Optional rollup for the whole check |
| StampDuty | decimal? | Optional |
| ForeignExchangeFees | decimal? | Optional |
| WithdrawalAttributed | decimal? | Optional cash out on this check |
| RowVersion | rowversion | Concurrency |

**Cost aggregation (per entry)**: Spread and dealing for the check use **either** `AggregatedSpreadDealingCosts` **or** the sum of `LogEntryHolding.SpreadAndDealingCosts`, never both. `StampDuty` and `ForeignExchangeFees` add on top. Application rejects mixed rollup + per-line costs (400).

**Children**: `LogEntryHolding` (0..* when rollup used; 1..* when per-line only).

---

## LogEntryHolding

| Field | Type | Rules |
|-------|------|--------|
| Id | Guid | PK |
| LogEntryId | Guid | FK |
| IdentifierOrName | string | User-entered; not validated as ticker feed |
| SpreadAndDealingCosts | decimal? | Per-line costs |
| Notes | string? | |

---

## TrialTopUp

| Field | Type | Rules |
|-------|------|--------|
| Id | Guid | PK |
| UserId | string | FK |
| EffectiveDate | DateOnly | After short-term review gate |
| Amount | decimal | > 0 |
| Notes | string? | User documents external action |

---

## CashObservation

Standalone withdrawal record (also summarised from entries if attributed).

| Field | Type | Rules |
|-------|------|--------|
| Id | Guid | PK |
| UserId | string | FK |
| ObservedDate | DateOnly | |
| Amount | decimal | > 0 |
| Notes | string? | |

---

## MilestoneReview

| Field | Type | Rules |
|-------|------|--------|
| Id | Guid | PK |
| UserId | string | FK |
| ReviewType | enum | ShortTerm3Month, StopOrContinue12Month, FollowUp18Month, EarlyStop |
| DecisionDate | DateOnly | |
| JudgementText | string | Required plain language |
| HopeStatus | enum? | Open, Failed (where applicable) |
| FurtherCapitalInScope | bool? | Short-term gate outcome |
| RowVersion | rowversion | Optimistic concurrency for multi-device sync |

**Uniqueness**: At most **one** `MilestoneReview` row per `(UserId, ReviewType)` (same pattern as one primary log entry per calendar date). Saving again for the same `ReviewType` **updates** that row (reopen for edit), never a second active review of the same type. Sync conflicts use `RowVersion` (**409**); the user reconciles before treating summaries as final (spec edge case).

**Milestone due dates** (computed, not stored): `TrialStartDate + 3/12/18 months` (calendar-aware in domain).

---

## Derived: TrialPotSnapshot (not persisted by default)

Computed from:

```
Pot = OpeningRingFencedAmount
    + Sum(TrialTopUp.Amount)
    - withdrawalsFromPot
    - Sum(entryTotalCosts for each LogEntry)
```

Where `entryTotalCosts` = spread/dealing (rollup **or** sum of holding lines) + `StampDuty` + `ForeignExchangeFees` per data-model cost aggregation rules.

Where:

```
withdrawalsFromPot = Sum(CashObservation.Amount)
  if any cash observation exists for the user
  else Sum(WithdrawalAttributed on LogEntry rows)
```

**Withdrawal semantics**: `CashObservation` is the preferred record for cash leaving the trial and feeds hope comparison (FR-009). `WithdrawalAttributed` on a log entry is an alternate pattern when the user documents withdrawal within a check week only—**never subtract both** for the same trial. Application MUST reject (400) a new `CashObservation` or non-null `WithdrawalAttributed` that would mix patterns once the other pattern already has data, unless the user clears/reconciles first (document in ADR if needed).

No share prices or broker balances (FR-015).

---

## Derived: HopeComparisonSummary (not persisted)

Rolling average/monthly buckets of **observed withdrawals** vs static hope band **£100–£200** (labels only, not targets).

Uses the same withdrawal source as `withdrawalsFromPot`: monthly buckets from `CashObservation.ObservedDate` when any cash observations exist; otherwise buckets from `LogEntry.CheckDate` using `WithdrawalAttributed` amounts.

---

## Validation summary (boundaries)

| Rule | Layer |
|------|--------|
| Unique (UserId, CheckDate) on LogEntry | DB index + domain |
| Unique (UserId, ReviewType) on MilestoneReview | DB index + domain upsert |
| Single withdrawal pattern: cash observations **or** entry `WithdrawalAttributed`, not both | Application + tests (see T023, T048) |
| Per-entry spread/dealing: rollup **or** holding lines, not both | Application + tests (see T023, T026) |
| No share recommendation fields | N/A—product copy only |
| Authorisation: UserId matches current user | API + EF query filter |
| String max lengths | API + EF configuration |
| Invalid dates / negative money rejected | API + domain |

---

## Identity tables

Standard ASP.NET Core Identity schema in the same `TrialDbContext` (or shared context) — users, roles, tokens as per template. **Schema version 3** stores passkey credentials (WebAuthn) alongside user records; migrations must use Identity’s v3 passkey tables when enabling passkeys.
