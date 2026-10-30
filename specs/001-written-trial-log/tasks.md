---

description: "Task list for feature implementation"
---

# Tasks: Written trial log with stop rule

**Input**: Design documents from `/specs/001-written-trial-log/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Unit and integration tests are included where plan.md, research.md, and constitution IV require protection of trial integrity (pot, milestones, capital gate, auth isolation). No Playwright or AppHost E2E.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/` (AppHost, ServiceDefaults, Web, Domain, Application, Infrastructure)
- **Tests**: `tests/PennyForYourStocks.Domain.Tests/`, `tests/PennyForYourStocks.Application.Tests/`, `tests/PennyForYourStocks.Infrastructure.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Replace placeholder library with Aspire + clean architecture layout; satisfy ADR gate before implementation PRs.

- [ ] T001 Record stack decision in `adr/0001-aspire-clean-architecture-trial-log.md` (Aspire, clean architecture, Identity passkeys, SQLite-first) per plan.md ADR gate
- [ ] T002 Create projects `src/PennyForYourStocks.AppHost/`, `src/PennyForYourStocks.ServiceDefaults/`, `src/PennyForYourStocks.Web/`, `src/PennyForYourStocks.Domain/`, `src/PennyForYourStocks.Application/`, `src/PennyForYourStocks.Infrastructure/` per plan.md structure
- [ ] T003 Create test projects `tests/PennyForYourStocks.Domain.Tests/`, `tests/PennyForYourStocks.Application.Tests/`, `tests/PennyForYourStocks.Infrastructure.Tests/` and wire xUnit v3 per CONVENTIONS.md
- [ ] T004 Update `PennyForYourStocks.slnx` to reference new projects and remove `src/PennyForYourStocks/` placeholder from the solution
- [ ] T005 [P] Set `TargetFramework` to `net10.0` only on all new projects; remove legacy `net8.0` from retired placeholder when removed
- [ ] T006 [P] Enable nullable reference types and treat warnings as errors on all new `.csproj` files per constitution V
- [ ] T007 Configure `src/PennyForYourStocks.AppHost/` with SQLite database resource, `WithReference` to Web, and `aspire run` entry point per research.md §1
- [ ] T008 [P] Add `src/PennyForYourStocks.ServiceDefaults/` and reference it from Web for OTel, health checks, and resilience defaults
- [ ] T009 [P] Add Tailwind CSS npm build pipeline in `src/PennyForYourStocks.Web/` outputting `wwwroot/css/app.css` per `contracts/ui-stack.md`

**Checkpoint**: Solution builds; AppHost starts (empty Web acceptable).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain rules, persistence, Identity, and API skeleton that all user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T010 Define domain entities and value objects (`TrialConfiguration`, `LogEntry`, `LogEntryHolding`, `TrialTopUp`, `CashObservation`, `MilestoneReview`) in `src/PennyForYourStocks.Domain/`
- [ ] T011 [P] Define enums `ReviewType` (`ShortTerm3Month`, `StopOrContinue12Month`, `FollowUp18Month`, `EarlyStop`) and `HopeStatus` (`Open`, `Failed`) in `src/PennyForYourStocks.Domain/`
- [ ] T012 Implement `TrialPotCalculator` in `src/PennyForYourStocks.Domain/` using `withdrawalsFromPot` branch in data-model.md (opening + top-ups − withdrawals − summed costs; no share prices)
- [ ] T013 Implement `MilestoneSchedule` (computed due dates: TrialStartDate + 3/12/18 months) in `src/PennyForYourStocks.Domain/`
- [ ] T014 Define `ITrialReadStore` / `ITrialWriteStore` (or unit-of-work) and request/response DTOs in `src/PennyForYourStocks.Application/`
- [ ] T015 Implement `TrialDbContext` with EF mappings, `decimal(18,2)` money columns, unique indexes on `(UserId, CheckDate)` for `LogEntry` and `(UserId, ReviewType)` for `MilestoneReview`, and `RowVersion` concurrency in `src/PennyForYourStocks.Infrastructure/`
- [ ] T016 Add initial EF Core migration in `src/PennyForYourStocks.Infrastructure/Migrations/`
- [ ] T017 Configure ASP.NET Core Identity schema version 3 with passkeys on `TrialDbContext`, global query filters on `UserId`, and per-user isolation (FR-014) in `src/PennyForYourStocks.Infrastructure/`
- [ ] T018 Register `AddSqliteDbContext<TrialDbContext>` via Aspire in `src/PennyForYourStocks.Web/Program.cs` (no hand-rolled connection strings) per research.md §1–§2
- [ ] T019 Implement EF repository classes for stores in `src/PennyForYourStocks.Infrastructure/` and `AddTrialInfrastructure` DI extension in `src/PennyForYourStocks.Infrastructure/DependencyInjection.cs`
- [ ] T020 Wire Identity passkey options including explicit `IdentityPasskeyOptions.ServerDomain` for non-local environments in `src/PennyForYourStocks.Web/Program.cs` per research.md §3
- [ ] T021 [P] Add authorised minimal API group at `/api/v1` with ProblemDetails errors in `src/PennyForYourStocks.Web/` (or dedicated Api project if split) per `contracts/http-api.md`
- [ ] T022 [P] Implement read-only guardrail copy provider for FR-003, FR-006, FR-010 static UK English strings in `src/PennyForYourStocks.Application/` and expose `GET /api/v1/trial/guardrails`
- [ ] T023 [P] Unit tests for `TrialPotCalculator` (withdrawal patterns, entry cost rollup vs per-line mutual exclusion) in `tests/PennyForYourStocks.Domain.Tests/TrialPotCalculatorTests.cs`
- [ ] T024 [P] Unit tests for `MilestoneSchedule` in `tests/PennyForYourStocks.Domain.Tests/MilestoneScheduleTests.cs`
- [ ] T025 Integration tests for EF `UserId` query filter isolation using SQLite in `tests/PennyForYourStocks.Infrastructure.Tests/TrialDataIsolationTests.cs`

**Checkpoint**: Signed-in user can hit health/guardrails; domain calculators tested; DB migrates under AppHost.

---

## Phase 3: User Story 1 — Record each trial check in the log (Priority: P1) 🎯 MVP

**Goal**: Create, view, edit, and list chronological log entries (consideration, holdings, costs) with same-day single entry behaviour and practice-mode warnings; no share recommendations.

**Independent Test**: Over several dated entries, confirm list order, field capture, same-day reopen, practice warning, and absence of recommendation copy (quickstart scenarios 1–3, SC-004).

### Implementation for User Story 1

- [ ] T026 [US1] Implement `UpsertLogEntryForCheckDate` use case enforcing one primary entry per calendar `CheckDate` per user (reopen upsert, not second row), per-entry cost rollup vs holding-line mutual exclusion, and withdrawal-pattern validation when setting `WithdrawalAttributed` in `src/PennyForYourStocks.Application/`
- [ ] T027 [US1] Implement list/get log entry queries with optional `from`/`to` DateOnly filters in `src/PennyForYourStocks.Application/`
- [ ] T028 [US1] Implement `GET /api/v1/trial/entries`, `GET /api/v1/trial/entries/{checkDate}`, `PUT /api/v1/trial/entries/{checkDate}` with validation (`consideration` required; string max lengths enforced at API per data-model.md) in `src/PennyForYourStocks.Web/`
- [ ] T029 [US1] Implement practice-mode policy check (FR-006): warn when official trial use lacks cost adjustments or explicit `practiceModeDisclaimer` in `src/PennyForYourStocks.Application/`
- [ ] T030 [P] [US1] Add Blazor weekly log editor page `src/PennyForYourStocks.Web/Components/Pages/Trial/LogEntryEditor.razor` with holdings sub-form (`IdentifierOrName`, per-line `SpreadAndDealingCosts`)
- [ ] T031 [P] [US1] Add Blazor chronological log list page `src/PennyForYourStocks.Web/Components/Pages/Trial/LogHistory.razor`
- [ ] T032 [US1] Route “new check” to existing entry when the chosen `CheckDate` already has a primary row (FR-001; same-day default) in `src/PennyForYourStocks.Web/Components/Pages/Trial/LogEntryEditor.razor`
- [ ] T033 [US1] Surface guardrail copy on log pages (no recommendations FR-003) using guardrails provider; audit visible strings in `src/PennyForYourStocks.Web/`
- [ ] T034 [US1] Minimal onboarding: create/update `TrialConfiguration` with `TrialStartDate` and `OpeningRingFencedAmount` default `50.00` GBP (`> 0`) so P1 logging is usable before full boundary UI in `src/PennyForYourStocks.Application/`
- [ ] T035 [P] [US1] Unit tests for same-day upsert and practice-mode predicate in `tests/PennyForYourStocks.Application.Tests/LogEntryUpsertTests.cs`

**Checkpoint**: User can onboard start date + £50, save weekly entries, reopen same-day entry, see warnings; no share suggestions.

---

## Phase 4: User Story 2 — Keep trial boundaries visible (Priority: P2)

**Goal**: Full trial configuration, compliance annotations, capital-before-3-month messaging, gated top-ups, and cash-only pot summary separate from non-trial notes.

**Independent Test**: Configure boundaries once; flag compliance on entries; block top-up before short-term review with policy message; pot view excludes ISA note from trial figures (quickstart scenarios 4–5, FR-004, FR-005, FR-015).

### Implementation for User Story 2

- [ ] T036 [US2] Implement get/update trial configuration use case with validation (`OpeningRingFencedAmount` > 0; permitted regions non-empty subset of UK/US/Europe) in `src/PennyForYourStocks.Application/`
- [ ] T037 [US2] Implement `GET /api/v1/trial/configuration` and `PUT /api/v1/trial/configuration` in `src/PennyForYourStocks.Web/` per `contracts/http-api.md`
- [ ] T038 [P] [US2] Add Blazor trial configuration page `src/PennyForYourStocks.Web/Components/Pages/Trial/Configuration.razor` (real shares only, no leverage, regions, exclusions, hold-day guidance, `NonTrialHoldingsNote`)
- [ ] T039 [US2] Extend log editor to capture `ComplianceNotes` boundary annotations in `src/PennyForYourStocks.Web/Components/Pages/Trial/LogEntryEditor.razor`
- [ ] T040 [US2] Implement capital gate policy: until `ShortTermReviewCompletedAt` is set, `POST /api/v1/trial/top-ups` returns **422** with out-of-scope messaging (FR-004) in `src/PennyForYourStocks.Application/` and Web endpoints
- [ ] T041 [US2] Implement post-gate `TrialTopUp` recording (`Amount` > 0, `EffectiveDate`) and `POST /api/v1/trial/top-ups` in `src/PennyForYourStocks.Infrastructure/` and Web
- [ ] T042 [US2] Implement `GET /api/v1/trial/summary/pot` using `TrialPotCalculator` with disclaimer “Cash and recorded costs only; not mark-to-market.” in `src/PennyForYourStocks.Web/`
- [ ] T043 [P] [US2] Add Blazor trial pot summary page separating pot figures from `NonTrialHoldingsNote` (FR-005) in `src/PennyForYourStocks.Web/Components/Pages/Trial/PotSummary.razor`
- [ ] T044 [P] [US2] Unit tests for capital gate before/after `ShortTermReviewCompletedAt` in `tests/PennyForYourStocks.Application.Tests/CapitalGateTests.cs`

**Checkpoint**: Boundaries visible; early top-up blocked with messaging; derived pot matches manual calculation.

---

## Phase 5: User Story 3 — Apply the stop rule and compare income hope (Priority: P3)

**Goal**: Milestone schedule and reviews (3 / 12 / 18 months + early stop), cash observations, hope comparison vs £100–£200 band, concurrency for multi-device sync.

**Independent Test**: With backdated start, complete milestone flows, record judgements and hope status, view hope summary and charts; export not required for review (quickstart scenarios 6–8, FR-007–FR-009, FR-011).

### Implementation for User Story 3

- [ ] T045 [US3] Implement `UpsertMilestoneReviewForType` use case (`ReviewType`, required `JudgementText`, optional `HopeStatus`, `FurtherCapitalInScope`) enforcing one row per `(UserId, ReviewType)` (reopen upsert, not duplicate active reviews) in `src/PennyForYourStocks.Application/`
- [ ] T046 [US3] Set `ShortTermReviewCompletedAt` on configuration when `ShortTerm3Month` review is saved in `src/PennyForYourStocks.Application/`
- [ ] T047 [US3] Implement `GET /api/v1/trial/milestones` and `POST /api/v1/trial/milestones/reviews` in `src/PennyForYourStocks.Web/` per `contracts/http-api.md`
- [ ] T048 [US3] Implement `CashObservation` create (`Amount` > 0), withdrawal-pattern validation (mutual exclusion with entry `WithdrawalAttributed`), and `POST /api/v1/trial/cash-observations` in `src/PennyForYourStocks.Infrastructure/` and Web
- [ ] T049 [US3] Implement `HopeComparisonSummary` domain logic (monthly buckets using same withdrawal source as pot; £100–£200 labels as observation only, not targets) in `src/PennyForYourStocks.Domain/` and `GET /api/v1/trial/summary/hope`
- [ ] T050 [P] [US3] Add Blazor milestone review flows in `src/PennyForYourStocks.Web/Components/Pages/Trial/MilestoneReviews.razor` (3-month, 12-month stop/continue, 18-month follow-up, early stop)
- [ ] T051 [P] [US3] Add Chart.js interop module `src/PennyForYourStocks.Web/wwwroot/js/hope-chart.js` and hope chart component with text fallback for accessibility per `contracts/ui-stack.md`
- [ ] T052 [US3] Display persistent profitable-period reminder (FR-010) on milestone and summary pages in `src/PennyForYourStocks.Web/`
- [ ] T053 [US3] Return **409** on stale `rowVersion` for log entry and milestone review upserts (sync conflict) per `contracts/http-api.md` in `src/PennyForYourStocks.Web/`
- [ ] T054 [P] [US3] Unit tests for hope comparison, milestone eligibility, and unique `ReviewType` upsert in `tests/PennyForYourStocks.Application.Tests/HopeComparisonTests.cs`
- [ ] T063 [US3] Implement `PracticeModeSummaryCompliance` (SC-005): reject **422** on `GET /trial/summary/pot`, `/trial/summary/hope`, and `/trial/summary/milestone-12` when practice-mode entries lack cost adjustment or explicit acknowledgement; omit export `derived` block when non-compliant (FR-016 raw backup always succeeds); unit tests in `tests/PennyForYourStocks.Application.Tests/PracticeModeSummaryComplianceTests.cs`
- [ ] T064 [P] [US3] Implement `GET /api/v1/trial/summary/milestone-12` composite (SC-002) and Blazor 12-month summary page in `src/PennyForYourStocks.Web/` per `contracts/http-api.md`

**Checkpoint**: Full stop-rule journey available; hope vs observation visible; conflicts surfaced on concurrent edit.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Export, account UX, documentation, and quality gates.

- [ ] T055 Implement JSON export bundle (`exportVersion` 1.0, configuration, entries, top-ups, cash observations, reviews, derived pot/hope) per `contracts/export-format.md` in `src/PennyForYourStocks.Application/`
- [ ] T056 Implement CSV export (log entry columns per export-format.md) and `GET /api/v1/trial/export?format=json|csv` (FR-016: never required for milestones) in `src/PennyForYourStocks.Web/`
- [ ] T057 [P] Add Blazor export page `src/PennyForYourStocks.Web/Components/Pages/Trial/Export.razor`
- [ ] T058 [P] Adapt Identity passkey/account pages to Tailwind + Lucide in `src/PennyForYourStocks.Web/Components/Account/`
- [ ] T059 Add XML documentation on public Application types and HTTP endpoint handlers per constitution III
- [ ] T060 Update repository README with AppHost run instructions and Tailwind build steps aligned to quickstart.md
- [ ] T061 Run `specs/001-written-trial-log/quickstart.md` scenarios manually and record any gaps
- [ ] T062 Verify `dotnet format PennyForYourStocks.slnx --verify-no-changes`, Release build with `-warnaserror`, and `dotnet test PennyForYourStocks.slnx -c Release --no-build`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately (human approval for new NuGet packages per AGENTS.md when adding Aspire/EF packages).
- **Foundational (Phase 2)**: Depends on Phase 1 — **blocks all user stories**.
- **User Story 1 (Phase 3)**: Depends on Phase 2 — MVP logging.
- **User Story 2 (Phase 4)**: Depends on Phase 2; extends US1 log UI and configuration — independently testable with configured trial.
- **User Story 3 (Phase 5)**: Depends on Phase 2; uses configuration, entries, top-ups, and cash data — independently testable with seeded data.
- **Polish (Phase 6)**: Depends on desired user stories being complete (export needs stored data from prior phases).

### User Story Dependencies

- **User Story 1 (P1)**: After Foundational — no dependency on US2/US3 (includes minimal onboarding for start date and opening amount).
- **User Story 2 (P2)**: After Foundational — builds on log pages from US1 but testable alone via API + configuration UI.
- **User Story 3 (P3)**: After Foundational — logically follows accumulated log/config data; early stop and reviews do not require US2 UI if configuration exists via API/tests.

### Within Each User Story

- Application use cases before Blazor pages (pages call same handlers as HTTP where possible per contracts note).
- Domain calculators tested before relying on summary endpoints.
- Story checkpoint before moving to next priority.

### Parallel Opportunities

- Phase 1: T005, T008, T009 in parallel after T002–T004.
- Phase 2: T011, T021, T022, T023, T024 in parallel after T010/T012–T015.
- User Story 1: T030, T031, T035 in parallel after T026–T028.
- User Story 2: T038, T043, T044 in parallel after T036–T037.
- User Story 3: T050, T051, T054 in parallel after T045–T049; T063 after T029/T035 (practice predicates); T064 after T049 (hope + pot summaries).
- Phase 6: T055–T056 after T063 (SC-005 derived omission on export; summary **422** only); T057, T058 in parallel.

---

## Parallel Example: User Story 1

```bash
# After T026–T028 complete, in parallel:
# T030 — LogEntryEditor.razor
# T031 — LogHistory.razor
# T035 — LogEntryUpsertTests.cs
```

---

## Parallel Example: User Story 3

```bash
# After hope/milestone APIs (T045–T049), in parallel:
# T050 — MilestoneReviews.razor
# T051 — hope-chart.js + chart component
# T054 — HopeComparisonTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (including ADR T001).
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. **STOP and VALIDATE**: quickstart scenarios 1–3 and automated tests for upsert/practice mode.
5. Demo weekly logging without milestones or export if time-boxed.

### Incremental Delivery

1. Setup + Foundational → secure shell with Identity passkeys and DB.
2. Add US1 → weekly log MVP.
3. Add US2 → boundaries and pot visibility.
4. Add US3 → stop rule and hope comparison.
5. Polish → export, account UX, README, full quickstart.

### Parallel Team Strategy

1. Team completes Setup + Foundational together (package approvals coordinated).
2. After Foundational:
   - Developer A: US1 log UI + entry API.
   - Developer B: US2 configuration + capital gate + pot.
   - Developer C: US3 milestones + hope charts.
3. Integrate on shared Application handlers and `TrialDbContext`.

---

## Notes

- Package additions (Aspire, EF, Identity, Chart.js npm, Tailwind) require explicit human approval per AGENTS.md before merge.
- Blazor may call Application in-process; keep behaviour aligned with `contracts/http-api.md` for future HTTP clients.
- Offline queue and Playwright E2E are out of scope for this feature (research.md §4, §9).
- Commit after each task or logical group; stop at any checkpoint to validate story independently.
