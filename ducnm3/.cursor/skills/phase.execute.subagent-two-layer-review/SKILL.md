---
name: phase.execute.subagent-two-layer-review
description: Use when a task slice is ready for validation against specifications and code quality standards.
when_to_use: Activate after completing a planned task slice or checkbox group. Mandatory for high-risk domains and multi-file changes.
license: MIT
---

# Execute — Two-layer review

**Skill id:** `phase.execute.subagent-two-layer-review`

Verify that the work is both "right" (spec compliance) and "correctly shaped" (code quality).

## Required session artifact (workflow)

Hub step **`review_slice`** expects **`.vibe/sessions/<task-id>/REVIEW-SLICES.md`**. Before `approve`, append:

- Slice / commit range reviewed
- Layer 1: AC/plan compliance (pass / issues)
- Layer 2: quality & risk (pass / issues)
- Skip reason only for trivial single-line edits (note “trivial — layers skipped”)

## The Protocol

1.  **Layer 1 (Spec Compliance)**: Does the change match the `PLAN.md` and AC? No scope drift allowed.
2.  **Layer 2 (Code Quality)**: Are tests passing? Is the style consistent? Are there security risks?

### Layer 2 — stack-aware checks (use with `dev.stack.*`)

After generic quality, add **one pass** aligned with the surface you touched (from `PLAN.md` / open files):

| Surface | Extra Layer 2 questions |
|---------|-------------------------|
| **Web (`dev.stack.frontend-web`)** | Responsive breakpoints, motion performance, a11y (focus, labels), asset loading |
| **RN / Flutter / iOS / Android** | Platform conventions, safe areas, navigation back stack, permission strings |
| **Full-stack / API** | Boundary validation, error shape, auth on new routes, CORS if new origin — see `dev.stack.fullstack` |
| **Any** | `verify_compliance` / project linter output if referenced in PLAN |

Do not expand Layer 2 into a new feature; if you find missing requirements, record them and fix in a **new** PLAN slice / TDD cycle.

## Iron Law — Sequential Verification

Layer 1 MUST pass before Layer 2 begins. Never polish code that is functionally incorrect or out of scope.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "Subagents are overkill for this small edit." | Fresh eyes (even AI ones) catch assumptions the implementer misses. |
| "I'll just do a mental review instead of a formal layer pass." | Mental reviews are prone to "confirmation bias." Formal layers enforce rigor. |

## See also

- `phase.review.code-request` — used for fresh-context review of the entire task.
- `phase.verify.compliance` — automated standards check after human/agent review.

---
**Summary:** Layer 1 = Right Thing (Spec); Layer 2 = Right Shape (Quality).
