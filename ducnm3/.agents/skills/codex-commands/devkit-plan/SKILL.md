---
name: devkit-plan
description: "Plan phase: break work down into PLAN.md; validate plan before production code per workflow."
---

> DevKit agent command `devkit-plan`, published as a skill because Codex has no repo-level slash commands.

# devkit-plan

**Plan phase** — break work down, **`PLAN.md`**, (per policy) validate the plan before production code.

**In Cursor:** slash **`/devkit-plan`**.

---

## Agent — required

1. **Valid Vibe session:**
   - Resolve **which task** and **which instruction file** per rule **`kaopiz-devkit-current-instruction`** (`--task` if passed → chat/context → `DEVKIT_TASK_ID` / documented scoped CLI → **`.vibe/active.json`**); read **`.vibe/sessions/<segment>/current_instruction.md`** or **`.vibe/current_instruction.<segment>.md`**, or the **`@` path** from **`kaopiz-devkit run`**.
   - If the step is **Plan** (or a plan sub-workflow): follow the **Hub skill** (e.g. `phase.plan.write-plan`) and **Step notes** — create/update **`PLAN.md`** under **`.vibe/sessions/<task>/PLAN.md`** (per workflow `expected_artifacts`; legacy root `PLAN.md` still resolves); mark Won’t do / Later where applicable.
   - If you are on another step: remind the user to `approve` until Plan or `start` with the right router; you may draft `PLAN.md` if the user explicitly asks (spike).

2. **No session yet:**
   - Suggested routers: `feature_dev`, `full_task_with_ingest`, or another router matching the task type (bugfix, refactor, …) — see **`.agents/vibe-coding/devkit/routers/*.json`**.
   - Run `start` and follow the instruction when you reach the Plan step (you may need to approve past Ingest first if the workflow includes ingest).

3. **Workspace rule:** if there is a **Review plan** step after Plan, do not merge large production code before the user approves the plan (per `vibe-coding-workflow`).

---

## See also

- **`/kaopiz-devkit`**, **`/devkit-research`** (if research is needed first)
- Skills **`phase.plan.write-plan`**, **`kaopiz-devkit`** (CLI/orchestration)
- After **Review plan**: `phase.review.validate-plan` → `phase.review.plan-approval`; fill **`validate_plan`** in `PLAN.md` (see Hub **`PLAN.template.md`**, doc **`packages/devkit-hub/docs/technical/REVIEW-PLAN-PHASE-CHEATSHEET.md`**)

This file syncs to **`.agents/commands/`** and **`.agents/workflows/`** (Antigravity).
