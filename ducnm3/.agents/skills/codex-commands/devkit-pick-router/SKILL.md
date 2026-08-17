---
name: devkit-pick-router
description: "Pick a * router and emit kaopiz-devkit start <task-id> --skill <router>; CLI requires --skill."
---

> DevKit agent command `devkit-pick-router`, published as a skill because Codex has no repo-level slash commands.

# devkit-pick-router

Pick a **`*` (router)** that fits the user’s **current** request, then output a full **`kaopiz-devkit start <task-id> --skill <router>`** command. The CLI **requires** `--skill`; there is **no** silent fallback from profile. **`devkit.workflow-profile.json`** → **`daily_workflow`** is only a **hint** (team default router, ingest, compliance, …).

**Skill:** (Agent playbook — no dedicated skill id)

## Required flow (YOU — AI)

1. **Read profile** (if present): `devkit.workflow-profile.json` and optionally `.vibe/workflow-profile.local.json` — note `daily_workflow.preferred_router_skill_id`, `require_ingest_first`, `prefer_full_gates`, `interview.answers`.
2. **Rank routers (recommended):** call MCP **`suggest_common_task_routers`** with `query` = task title/description (and optional `project_root` = repo absolute path to receive merged `daily_workflow` in the JSON). Use `results[]` scores as a **hint**, not a hard rule.
3. **List routers** from the Hub after sync: **`setup/routers/*.json`** in the kit (or `.agents/vibe-coding/devkit/routers/` after materialize) — each file has `id`, `description`, `routes_to_workflow` — **only pick ids that exist** on disk.
4. **Map user intent** → **one** `*`:
   - Typical feature / dev work → often `feature_dev`.
   - Needs ingest + full Vibe gates (plan/review/…) → often `full_task_with_ingest`.
   - Same full gates but **fewer flat steps** (~5–6 + optional deliver) for small clear tickets → `task_compact`; with a **one-step ingest** first → `task_compact_with_ingest`.
   - Bug / hotfix / refactor / review / docs / spec / investigate / migration / conflict / release — match JSON filename and description.
5. **Present** to the user: suggested router + **short rationale** + profile hint (if it differs, explain).
6. If the user’s chosen `--skill` **differs** from `daily_workflow.preferred_router_skill_id` or from the **top** MCP suggestion: **ask in chat** whether they still want to run `start` with that router — do **not** rely on TTY; wait for confirmation, then run the command.
7. **Present the handoff (chat-first, then shell):** In Cursor (mode A), show the user the slash form they can paste or run next — same args as CLI:  
   `/kaopiz-devkit start <TASK-ID> --skill <*>`  
   Optional: `--quiet-router-hints` when stderr router warnings would clutter scripts/CI.  
   **Shell equivalent** (what you actually execute in terminal / automation — repo root with `package.json`):  
   `npm run kaopiz-devkit -- start <TASK-ID> --skill <*>`  
   The `npm run …` lines are not “instead of” slash chat; they are the **exact** command the Agent runs under the hood.

## Don’t / limits

- **Do not** assume `start` reads the profile so `--skill` can be omitted.
- **Do not** scaffold an app in this slash.

## See also

- `kaopiz-devkit.md` — full CLI.
- `devkit-workflow-profile.md` — profile & interview.
