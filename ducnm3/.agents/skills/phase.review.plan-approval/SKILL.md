---
name: phase.review.plan-approval
description: "PLAN approval gate; human sign-off on PLAN.md after validate_plan — keywords: approve plan, review plan gate, plan OK before execute, cancelled blocked."
when_to_use: Hub step approval_and_outcome in phase.review, or the combined validate_approve step in phase.review.compact, after PLAN exists and (when applicable) validate_plan has been run.
license: MIT
---

# Review — PLAN approval & outcome

**Skill id:** `phase.review.plan-approval`

**Plan file on disk:** the same canonical path as drafting — **`.vibe/sessions/<task-id>/PLAN.md`** (legacy root **`PLAN.md`** still supported by tooling). Use this for the **human decision** on that plan, not for reviewing source code diffs. For fresh **code** review after implementation, use **`phase.review.code-request`**.

## When this applies

- **`phase.review`**, step **`approval_and_outcome`** — **after** **`validate_and_present`** (**`phase.review.validate-plan`**) has run **`validate_plan`** and walked traceability.
- **`phase.review.compact`**, step **`validate_approve`** — single flat step that bundles **`validate_plan`** + approval; apply the same criteria **before** you approve.

## What to confirm (with the user)

| Criterion | Question to satisfy |
| --------- | ------------------- |
| **Đủ chi tiết** | Steps cite **repo-relative files**; important work has **Run / Expected** (or explicit non-TDD verification); no empty “TBD” where execution would guess. |
| **Khả thi** | Dependencies, ordering, and **Risks & open questions** in `PLAN.md` match reality; assumptions from **`.vibe/research/<task-id>.md`** are reflected or explicitly carried. |
| **Đúng phạm vi** | **Won’t do / Later** and traceability cover AC / scope; no silent scope creep. |
| **Tín hiệu MCP** | **`validate_plan`** result (ok / warnings / errors) is addressed before approval — fix `PLAN.md` or record accepted risk. |

## How to ask (modal — required)

Present the decision via the editor's **ask-question tool** (`AskUserQuestion` on opencode/Claude, `AskQuestion` on Cursor) — see rule **`kaopiz-devkit-current-instruction`** (interaction principle). **Do not** ask the approval as plain chat text. Ask `Approve PLAN for <task>?` with options:
- **Approve** — plan is sufficient; proceed to Execute.
- **Revise plan** — list what to change (free-text via "type your own answer").
- **Cancel / Block** — do not enter Execute.

## Outcomes

- **Approve** → next phase may enter **Execute** (per workflow).
- **Revise plan** → edit `PLAN.md`, then **repeat** validation (do not skip **`validate_plan`** when the workflow splits steps).
- **Cancelled** / **Blocked** → do **not** enter Execute; ticket/task state per team policy.

## What you do **not** do here

- Do **not** run git-SHA / subagent **code-review** flows — that is **`phase.review.code-request`** after implementation.
- Do **not** replace **`phase.review.validate-plan`** when that step exists — it owns MCP validation and first presentation.

## Composite workflows and `skill_id`

Parent steps (e.g. **`review_plan`** on **`default.with-ingest`**) may list **`skill_id`: `phase.review.plan-approval`** as the **representative** skill for the whole Review phase; the atomic manifest still runs **`validate-plan`** first, then **this skill**.

## See also

- `phase.review.validate-plan` — `validate_plan` + present `PLAN.md` before this gate.
- `phase.review.code-request` — independent **code** review (Execute / PR).
- `phase.plan.write-plan` — drafting `PLAN.md`.

---
**Summary:** Approve or block **PLAN** after validation — not code review.
