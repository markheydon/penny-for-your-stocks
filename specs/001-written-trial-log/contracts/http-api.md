# HTTP API contract (logical)

**Feature**: `001-written-trial-log`  
**Version**: 0.1 (planning)  
**Base path**: `/api/v1`  
**Auth**: Cookie (Blazor) or bearer token if added later; all endpoints require authenticated user unless noted.

Errors use **ProblemDetails** (RFC 7807); validation failures return **400** with field errors. Unauthorized **401**. Cross-user access **404** (do not leak existence).

## Trial configuration

### GET `/trial/configuration`

Returns the current user's trial configuration or **404** if not yet onboarded.

### PUT `/trial/configuration`

Create or update configuration. Body:

```json
{
  "trialStartDate": "2026-01-01",
  "openingRingFencedAmount": 50.00,
  "permittedRegions": ["UK", "US", "Europe"],
  "excludedThemesNotes": "No weapons-related holdings",
  "realSharesOnly": true,
  "noLeverageOrDerivatives": true,
  "intendedHoldDaysMin": 2,
  "intendedHoldDaysMax": 7,
  "nonTrialHoldingsNote": "ISA holdings documented separately"
}
```

Validation: amounts > 0; dates valid; regions non-empty subset of allowed set.

## Log entries

### GET `/trial/entries`

Query: `from`, `to` (optional DateOnly). Returns chronological list (summary fields).

### GET `/trial/entries/{checkDate}`

`checkDate` format `YYYY-MM-DD`. Returns full entry or **404**.

### PUT `/trial/entries/{checkDate}`

Upsert **primary entry for calendar date** (FR-001). Body:

```json
{
  "consideration": "Reviewed existing positions; no new trades.",
  "complianceNotes": "All holdings UK equities",
  "practiceModeAcknowledged": false,
  "practiceModeDisclaimer": null,
  "aggregatedSpreadDealingCosts": null,
  "stampDuty": null,
  "foreignExchangeFees": null,
  "withdrawalAttributed": null,
  "holdings": [
    {
      "identifierOrName": "Example plc",
      "spreadAndDealingCosts": 3.50,
      "notes": null
    }
  ],
  "rowVersion": "base64..."
}
```

**409** if `rowVersion` stale (sync conflict).

**400** if spread/dealing costs use both rollup and per-holding lines, or if `withdrawalAttributed` conflicts with existing `CashObservation` rows (withdrawal pattern mix).

### DELETE `/trial/entries/{checkDate}`

Optional; if supported, soft-delete or hard-delete with audit—implementation choice documented in ADR/tasks.

## Trial pot and hope summary

### GET `/trial/summary/pot`

**422** when SC-005 is not satisfied: any log entry flagged as practice/simulated official use without cost adjustments and without `practiceModeAcknowledged` (or equivalent domain rule from FR-006). ProblemDetails include `practiceModeCompliance` guidance; UI must not present figures as official trial summaries until resolved.

Response when compliant:

```json
{
  "openingAmount": 50.00,
  "topUpsTotal": 0.00,
  "costsTotal": 12.30,
  "withdrawalsTotal": 0.00,
  "derivedPot": 37.70,
  "disclaimer": "Cash and recorded costs only; not mark-to-market.",
  "practiceModeCompliance": { "satisfied": true }
}
```

### GET `/trial/summary/hope`

Same SC-005 gate as pot summary (**422** when non-compliant).

Response includes monthly buckets and comparison labels to £100–£200 hope band (observation only).

## Milestones

### GET `/trial/milestones`

Returns scheduled dates (3 / 12 / 18 months) and completion status from stored reviews.

### POST `/trial/milestones/reviews`

Body:

```json
{
  "reviewType": "ShortTerm3Month",
  "decisionDate": "2026-04-01",
  "judgementText": "Still learning; not adding capital yet.",
  "hopeStatus": "Open",
  "furtherCapitalInScope": false
}
```

Allowed `reviewType`: `ShortTerm3Month`, `StopOrContinue12Month`, `FollowUp18Month`, `EarlyStop`.

Upsert **one stored review per `reviewType` per user** (reopen for edit; no duplicate active rows). Optional `rowVersion` on body; **409** if stale (multi-device sync, same as log entries).

### GET `/trial/summary/milestone-12` (SC-002)

Composite read model: 12-month stop-or-continue `judgementText` (if recorded), total observed withdrawals, derived pot block (pot summary fields), and hope comparison summary—single payload so the user need not assemble spreadsheets. Subject to the same SC-005 gate (**422** when non-compliant).

## Top-ups (post gate)

### POST `/trial/top-ups`

Rejected with **422** and policy message if short-term review not complete. Body: `effectiveDate`, `amount`, `notes`.

## Cash observations

### POST `/trial/cash-observations`

Body: `observedDate`, `amount`, `notes`.

**400** if trial already records withdrawals via log entry `WithdrawalAttributed` (pattern mix).

## Export

### GET `/trial/export`

Query: `format=json|csv` (default `json`). Returns full bundle per [export-format.md](./export-format.md). **Never** required for milestone completion.

Stored configuration, entries, top-ups, cash observations, and reviews are **always** included (FR-016). When SC-005 is not satisfied, **omit** the `derived` block entirely—do **not** reject the entire export with **422**.

## Policy endpoints (read-only copy)

### GET `/trial/guardrails`

Returns static UK English strings: no recommendations, practice-mode warning, profitable stretch reminder, capital-before-3-months message (FR-003, FR-006, FR-010).

---

## Implementation note

Blazor MVP may invoke Application services **in-process** without HTTP; keep handler logic identical to these routes so future HTTP clients share one behaviour.

Passkey ceremonies use Identity endpoints (not listed here); see [ui-stack.md](./ui-stack.md) and Microsoft Learn passkey docs for Blazor Web Apps.
