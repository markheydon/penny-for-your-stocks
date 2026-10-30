# Export format contract

**Feature**: `001-written-trial-log`  
**Version**: 0.1 (planning)

Export is available at any time (FR-016). Formats: **JSON** (canonical) and **CSV** (log entries tabular only).

## JSON export (`application/json`)

Filename suggestion: `trial-log-export-{yyyy-MM-dd}.json`

Root object:

```json
{
  "exportVersion": "1.0",
  "exportedAt": "2026-10-30T12:00:00Z",
  "configuration": { },
  "logEntries": [ ],
  "topUps": [ ],
  "cashObservations": [ ],
  "milestoneReviews": [ ],
  "derived": {
    "pot": { },
    "hopeComparison": { }
  }
}
```

- **`configuration`**: Same fields as GET `/trial/configuration`.
- **`logEntries`**: Full entry payloads including `holdings` arrays and `rowVersion` omitted or included for backup only (implementation documents choice).
- **`derived`**: Snapshot at export time when SC-005 is satisfied; **omitted** when practice-mode compliance fails (raw stored data still exported per FR-016). When present, reproducible from stored data via domain calculator.

## CSV export (`text/csv`)

One row per log entry with columns:

| Column | Description |
|--------|-------------|
| CheckDate | ISO date |
| Consideration | Escaped text |
| HoldingsSummary | Semicolon-separated names |
| TotalCosts | Decimal |
| WithdrawalAttributed | Decimal |
| ComplianceNotes | Text |

Configuration, reviews, and top-ups remain JSON-only in v1 unless a second CSV sheet is added later.

## Privacy

Export contains personal financial notes; treat as sensitive. No server-side sharing endpoints in v1.
