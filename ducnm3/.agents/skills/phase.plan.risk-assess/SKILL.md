---
name: phase.plan.risk-assess
description: Use when drafting or refining an implementation plan to proactively identify dependencies, breaking changes, and performance impacts.
when_to_use: Activate during the Planning phase for any task touching multiple modules, data schemas, or performance-critical logic. Skip for trivial docs-only or CSS-only changes.
license: MIT
---

# Plan — Technical risk assessment

**Skill id:** `phase.plan.risk-assess`

The Agent acts as a Senior developer to "review" their own plan and find potential weaknesses.

## Risk Checklist

1.  **Breaking Changes**: Will this modification break other modules? Use `git grep` to find where this module is being used.
2.  **Performance**: Is this query too slow? Will this logic cause an infinite loop?
3.  **Refactor**: Is it necessary to refactor existing code before adding new logic (Vibe #6)? 
4.  **Reusability**: Does a similar module already exist in the project? Call `fetch_knowhow` before "reinventing the wheel."

## Required session artifact (workflow)

When the Hub step **`map_domains`** lists **`expected_artifacts`**, create or update **`.vibe/sessions/<task-id>/RISK.md`** (same segment as `PLAN.md`) **before** `kaopiz-devkit approve`. Minimum content:

- Domains touched (auth, billing, …)
- Top risks and mitigations (or “none material” for trivial work)
- Anything that must be merged into `PLAN.md` in the next step

## Actions

If high risk is detected, add **explicit checkbox step(s)** in `PLAN.md` (micro-task style — see **`phase.plan.write-plan`**), e.g. **Step 0:** refactor module ABC before feature work; include **files touched** and **Run / Expected** where it helps execution.

Keep prerequisites **concrete** — avoid placeholders like “refactor if needed” without a named step or exit criterion.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'll just fix risks as I encounter them." | Fixing risks mid-execution causes plan updates and cycle loss. Detect them early. |
| "This project is small, no real risks." | Every multi-file change has a regression risk. Grep for usages to be sure. |

## See also

- `phase.plan.write-plan` — the destination for identified risk-mitigation tasks.
- `phase.execute.fetch-knowhow` — domain rules used during execution.

---
**Summary:** Paranoid planning prevents poor performance.
