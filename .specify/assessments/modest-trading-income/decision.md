# Decision: Modest income from active share trading

- **Slug**: modest-trading-income
- **Decided**: 2026-10-29
- **Verdict**: go
- **Artifacts reviewed**: intake.md | research.md | problem.md | concept.md

## Scorecard

| Criterion | Rating | Justification |
|-----------|--------|---------------|
| Problem validity | adequate | One UK retail investor cannot yet apply their own stop rule, because no trial has been logged. The income hope itself is already answered for an opening pot of about £50: £100–£200 a month is 2,400–4,800% a year, against a few pounds a year at a long-run nominal UK equity rate near 9%. What remains worth solving is that personal stop-or-continue record, not a path to the monthly withdrawal. |
| Evidence strength | adequate | The capital arithmetic is high confidence. Barber and Odean (frequent individual trading underperforms after costs) and the Taiwan day-trading persistence result are the strongest external performance evidence; they are medium confidence as applied here because the market, era, and holding period differ. Overall research confidence is medium, and full-page fetches were refused, but the decision-relevant facts are quantified. Background disagreements (French transaction tax, the April 2027 cash-ISA snippets, practice-mode fill quality) do not block the call. |
| Value vs. inaction | adequate | Doing nothing keeps the existing stocks and shares ISA and avoids turnover costs, and the project goals already treat a good sum as close to zero. It also leaves no personal pass/fail record, so a later top-up would still be a guess. A weekly log of about £50 that can be lost is a small cost for that record. |
| Feasibility / appetite | strong | The recommended option is one written trial: a notebook or spreadsheet, holds of a few days to about a week, a check at least weekly, and a plain-language stop at about 12 months with a review by about 18 months. Appetite is small and does not depend on a data feed, a broker adapter, or an application. |
| Strategic fit | adequate | Updated `GOALS.md` and `SCOPE.md` describe a ring-fenced trial log, real shares only within stated boundaries, income treated as a hope, and a clear failure as a valid outcome. This trial matches that. The constitution file is still an unfilled template, so it does not add a constraint. Building a separate analysis application and using Microsoft Foundry stay deferred until a later decision, which matches the project kill criteria on time and on raising stakes. |
| Risk posture | adequate | Cost drag, wide penny-share spreads, practice-mode results that omit spread and tax, an unread log, reading luck as skill, and adding capital before a short-term review are all named. The trial bounds them: write costs into each line, keep the opening sum near £50 until a short-term review, and do not treat 12–18 months of one person’s trades as proof of an edge. That last limit is accepted; a sample this small cannot separate luck from persistence. |

## Verdict & Rationale

**Go**, for a written trial with a stop rule (concept option B), and not for a trading application.

Problem validity and evidence are both adequate, and concept.md recommends a concrete option inside a small appetite. The income hope does not clear the bar on the opening pot, and published studies of frequent individual traders point to costs pulling results below the market. Specifying software now would spend hobby time before any personal record exists, and the evidence does not show that software improves the chance of the withdrawal. The idea is still worth specifying as the bounded trial: the one thing research cannot supply is the requester’s own 12-month judgement of whether the attempt is clearly just losing money.

Option A never creates that record. Option C would build a separate analysis application before the first logged month. Both stay out of this handoff.

## If needs-clarification

Not applicable. The verdict is go. Intake and research record no remaining questions for the requester.

## If go — Handoff to `/speckit-specify`

- **Problem**: A UK retail investor with basic experience has no personal record on which to decide, over 12–18 months, whether a small ring-fenced trial of active share dealing is clearly just losing money and should stop. Withdrawing about £100–£200 a month is a hope they accept may never be met; on about £50 it is far above long-run market rates.
- **Chosen approach**: A written trial with a stop rule. One ring-fenced log (notebook or spreadsheet), starting at about £50 of money that can be lost. Real shares only, in the UK, the United States, or Europe, excluding controversial areas such as weapons. No leverage or derivatives. Checks at least weekly, and daily only by choice. Intended holds of a few days to about a week. Each line records what was considered, what was held, and a plain allowance for spread and dealing costs. At about 12 months, stop if it is clearly just losing money; review again by about 18 months. Any cash withdrawn is written as observed cash and compared with the £100–£200 hope. Further money is considered only if the short term is not already a clear failure, and the amount is chosen then. The log does not suggest shares.
- **In scope / out of scope**: In scope is the trial record, the stop rule, and the comparison of any withdrawal with the hope. Out of scope: share recommendations or ranking screens; leverage, CFDs, spread betting, or borrowed money; same-day dealing or a required daily check; choosing a broker, platform fees, platform limits, or tax advice; treating the trial as proof of a repeatable edge; building an analysis or trading application, including Microsoft Foundry, until a later decision; adding capital before the short-term review; making penny shares a requirement.
- **Success metrics**: A recorded stop-or-continue judgement at about 12 months and a review by about 18 months. Any withdrawal compared with the £100–£200-a-month hope and recorded as observed cash. Extra capital, if any, added only after the short term is not already a clear failure. The requester can say from the log whether the hope is still open or has failed, without reading a short run as a durable edge.
- **Carried-forward open questions**: None that block specifying. Assumptions the specification should keep visible: a notebook or spreadsheet filled in on the weekly check is enough; “clearly just losing money” stays plain language, and the £100 figure is an illustration of that sentence, not a loss budget or a profit target; spread, and stamp duty or foreign-exchange fees where they apply, are written into each line, and a practice-mode result that omits them is not the trial result; the opening sum stays about £50 until a short-term review; the existing stocks and shares ISA continues for money outside the trial; a profitable stretch inside 12–18 months is not evidence of an edge that justifies larger stakes.
