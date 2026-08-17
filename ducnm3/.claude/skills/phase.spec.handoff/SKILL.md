---
name: phase.spec.handoff
description: Use when the spec is clear enough to hand off to planning — record marker and user approval; no PLAN.md in this step.
when_to_use: After clarify_until_clear; when AC and scope are agreed. Run verify_commands if the workflow attaches them.
license: MIT
---

# Spec — Ready for PLAN (handoff)

**Skill id:** `phase.spec.handoff`

This step is **not** another full ingest/read-sources pass. It **closes** the spec-discovery line: confirm the research file is complete, add the handoff phrase, and get explicit approval before starting `default`, `default.with-ingest`, `default.compact`, `default.compact.with-ingest`, or `dev.feature`.

## What you do

1. Confirm **`.vibe/research/<task-id>.md`** reflects **Confirmed vs Assumptions**, **In scope / Out of scope**, and resolved **Ambiguities** (or a clear list of open items the team accepts).
2. When ready, add the exact phrase **`Spec ready for PLAN`** (e.g. under **Decision log** in the research file).
3. Request **user approval** on this step if the workflow sets `requires_user_approval` — via the editor's ask-question tool (`AskUserQuestion` on opencode/Claude Code · `AskQuestion` on Cursor), not plain chat text.
4. If the workflow lists **verify_commands**, run **`kaopiz-devkit verify`** so the scripted check passes (e.g. phrase present in research file).

## What you do not do

- Do **not** recreate PLAN.md here.
- Do **not** re-run the full **read-sources** playbook; that was the previous step.

## Next

Start planning with the same `task_id`, e.g. `kaopiz-devkit start <task-id> --skill feature_dev` or your team’s router to `default`, `with-ingest`, or the **compact** variants (`task_compact`, `task_compact_with_ingest`).
