---
name: phase.plan.write-plan
description: Use when requirements and workscope are captured (e.g. in .vibe/research) and you need a detailed, bite-sized implementation plan before writing code.
when_to_use: After ingest + scope lock (or explicit BLOCKED handoff per research file), or when the workflow loads spec without a separate ingest phase. Mandatory for multi-file or multi-slice implementation work.
license: MIT
---

# Plan — Writing plans (spec → implementation)

**Skill id:** `phase.plan.write-plan`

Write a comprehensive plan that a skilled developer with zero project context could follow.

## Where to write the plan file

**Canonical path:** **`.vibe/sessions/<sanitized-task-id>/PLAN.md`** (same folder segment as the active Vibe session; matches workflow **`expected_artifacts`**). Repo-root **`PLAN.md`** is legacy — `kaopiz-devkit verify`, **`approve`**, task-runner review, and post-approve translate resolve bare `PLAN.md` to the session file when it exists.

## After Ingest (`with-ingest` and similar workflows)

When the parent workflow **already ran** ingest (`default.with-ingest`, `phase.plan.post-ingest`, `phase.plan.detailed.post-ingest`, …):

1. **MUST** read the canonical research file **`.vibe/research/<task-id>.md`** first (`<task-id>` from **active session** state / legacy **`.vibe/state.json`** when present; normalize filename like ingest skills).
2. **Reflect** into `PLAN.md`: **In scope / Out of scope**, **Confirmed vs Assumptions**, material **Ambiguities** (and how the plan addresses them or leaves them explicit).
3. If research contains **`BLOCKED:`** or scope cannot be executed yet: **do not** draft a pretend full implementation plan. Either return to ingest / clarify, or write `PLAN.md` that states **blocked**, **what is missing**, and **resume conditions** only.

This handoff matches **`.claude/rules/artifact-spec-plan-log.md`** — one research file in, one plan file (**`PLAN.md`** under the session dir) out.

## Minimum sections in `PLAN.md`

Follow workspace rule **`artifact-spec-plan-log`** (minimum sections). At least:

| Section | Purpose |
| ------- | ------- |
| **Task / scope** | `task_id` or scope label |
| **Requirements traceability** | AC / requirement → plan step or section (proves research was consumed) |
| **Plan steps** | Checkboxes, repo-relative **files**, no empty TBD |
| **Verification** | **Run:** / **Expected:** per important step (see Task structure below) |
| **Won't do / Later** | Align with research out-of-scope |
| **validate_plan** | Fill after Review step MCP call — stub “pending” here if drafting only |
| **Risks & open questions** | Including unresolved assumptions |
| **Revision note** | When the plan changes later |

Optional: start from Hub **`PLAN.template.md`** (`kit/setup/content/templates/`).

## Task structure (steps in the plan)

### Default — code-changing slices

Each plan slice SHOULD include:

- **Files** — exact paths to create or modify.
- **TDD** — explicit Red / Green (or equivalent) with expected signal **when automated tests apply**.
- **Commit** — a concrete git command when the repo uses commit-per-slice.

### When not to force TDD on every line

| Situation | Use instead of Red–Green tests |
| --------- | ------------------------------ |
| Docs-only, comments-only | **Verification:** proofread + link check; **Run:** e.g. `npm run docs:build` if applicable |
| Config / infra YAML | **Verification:** validate config; **Run:** linter or `dry-run` command + **Expected:** |
| Manual / exploratory QA | **Verification:** scripted checklist + **Expected:** user-visible outcome |
| Spike behind a flag | Isolate spike paths; **Verification:** how the spike is discarded or promoted |

Still keep **Files**, **Run / Expected**, and **commit** (or explicit “no commit until slice N”) where it helps execution.

## Iron Law — No Code without a Plan

Do not change production source for the task until `PLAN.md` is approved per workflow. If you spike during planning, use a scratch file or branch — not the main plan.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I already know how to build it, the plan is just extra work." | Plans allow review and prevent debugging-by-coincidence. |
| "I'll just write 'Write tests' as a step." | Name the test case or the verification command. |
| "The project is changing too fast to plan." | Fast change needs a stable plan and explicit **Revision note** when you adjust. |

## Hub — `phase.review` / `validate_and_present`

Workflow **`phase.review`** step **`validate_and_present`** uses **`phase.review.validate-plan`** (not this skill): **`validate_plan`** + present **`PLAN.md`**, then **`phase.review.plan-approval`** for **approval** / **outcome**. **This skill** is for **drafting** `PLAN.md` during the Plan phase.

## See also

- `phase.review.validate-plan` — validate & present `PLAN.md` before the human approval gate.
- `phase.execute.subagent-two-layer-review` — execution review pattern.
- `phase.plan.risk-assess` — technical risks before locking the plan (detailed plan workflows).
- `phase.review.plan-approval` — human gate after PLAN validation.
- `phase.review.code-request` — fresh **code** review after implementation (not PLAN gate).
- `phase.ingest.read-sources` / `phase.ingest.lock-scope` — upstream research + scope.

---
**Summary:** Draft `PLAN.md` with research handoff when ingest ran; minimum sections per **artifact-spec-plan-log**; TDD when code, alternate verification when not.
