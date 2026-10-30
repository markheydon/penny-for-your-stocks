# Quickstart: Validating the written trial log

**Feature**: `001-written-trial-log`  
**Status**: Pre-implementation — run these scenarios after `/speckit-implement` completes.

References: [data-model.md](./data-model.md), [contracts/http-api.md](./contracts/http-api.md).

## Prerequisites

- .NET 10 SDK and Aspire workload/CLI as per repository README (to be updated during implementation).
- Repository built with Release warnings-as-error:

```bash
dotnet build PennyForYourStocks.slnx -c Release -warnaserror
```

- AppHost running (expected command after implementation):

```bash
dotnet run --project src/PennyForYourStocks.AppHost
```

Open the HTTPS URL shown in the Aspire dashboard for the Web project.

## Scenario 0 — Passkey sign-in (FR-013)

1. Register using **Create account with passkey** (or add a passkey after account creation).
2. Sign out and sign in with **Log in with passkey** on a second browser profile (same user).
3. **Expected**: Same trial data after sync; HTTPS required for WebAuthn.

## Scenario 1 — Onboard and first weekly entry (P1 / FR-001, FR-002)

1. Register or sign in (passkey preferred).
2. Set trial start date and opening amount (~£50) in configuration.
3. Create a log entry for today with consideration text, at least one holding, and spread/dealing costs.
4. **Expected**: Entry appears in chronological list; no share suggestions anywhere in UI (SC-004).

## Scenario 2 — Same calendar date reopens single entry (FR-001)

1. Save an entry for today's date.
2. Start a new check again on the same calendar date.
3. **Expected**: Same entry opens for edit; no duplicate primary row.

## Scenario 3 — Practice mode warning (FR-006 / SC-005)

1. Mark an entry as practice/simulated without cost adjustments.
2. Attempt to treat it as official without acknowledgement.
3. **Expected**: Warning; summary flows require adjusted figures or explicit disclaimer.

## Scenario 4 — Capital before 3-month review (FR-004)

1. With trial start date less than 3 months ago and no short-term review recorded, attempt to record a top-up.
2. **Expected**: Out-of-scope messaging; user may note external intent only per spec.

## Scenario 5 — Derived pot (FR-015)

1. Record several entries with costs and optional withdrawal attribution.
2. Open trial pot summary.
3. **Expected**: Pot equals opening + top-ups − withdrawals − summed costs; no share prices shown.

## Scenario 6 — Milestone reviews (FR-007, FR-008, FR-009)

1. Set trial start date in the past (test environment may allow editing for validation).
2. Complete short-term, 12-month, or simulated milestone flows.
3. Record plain-language judgements and hope status.
4. **Expected**: Hope comparison shows observation vs £100–£200 band labels, not targets.

## Scenario 7 — Export without review gate (FR-016)

1. Ensure practice-mode compliance (Scenario 3 satisfied or no practice-flagged entries without acknowledgement).
2. Export JSON before completing any milestone review.
3. **Expected**: Download succeeds with stored data; `derived` omitted if SC-005 violated (Scenario 3); milestone review can still be completed afterward.

## Scenario 8 — Second device sync (FR-013)

1. Sign in as the same user in a second browser profile.
2. **Expected**: Same configuration and entries visible; edit conflict shows reconciliation if concurrent saves (409 / UI message).

## Automated checks

```bash
dotnet test PennyForYourStocks.slnx -c Release --no-build
```

Domain/Application tests must cover pot calculation, milestone scheduling, and capital gate without AppHost.

## Out of scope for this quickstart

- Broker connectivity, live prices, share screeners.
- AppHost orchestration automated tests (explicitly excluded).
- Playwright or other browser E2E automation (explicitly excluded for this feature).
