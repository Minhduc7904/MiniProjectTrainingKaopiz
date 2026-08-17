---
name: kaopiz-devkit
description: 'Orchestrate the DevKit workflow via CLI — resolve task (--task if passed → chat/context → DEVKIT_TASK_ID/docs → active.json), read per-session current_instruction (canonical, per-task mirror, or `run` stdout); slash `archive` means CLI + same-turn rollup Agent review. Slash `/kaopiz-devkit`: see `.agents/commands/kaopiz-devkit.md`.'
when_to_use: Use whenever the session uses `kaopiz-devkit` CLI state (`.vibe/`), slash commands that map to the CLI, or the user asks to start/step/approve/archive a DevKit workflow.
license: MIT
---

# Kaopiz DevKit — Agent orchestration (CLI)

When the team uses **`kaopiz-devkit`** to keep workflow **state** (`.vibe/`), the agent should **actively run terminal commands** at the **app repo root** (where `package.json` lives and `.agents/` is synced), instead of asking the user to type each command — unless the user says they will run commands themselves.

## Two layers: CLI + step content

- **`init` / `sync` / `doctor`:** usually run the CLI and summarize.
- **`start` / `step` / `run` / (after) `approve`:** run the CLI **and** when there is still work to do, resolve **which task** and **which instruction file** per workspace rule **`kaopiz-devkit-current-instruction`** ( **`--task`** if passed on that command → **chat/context** → **`DEVKIT_TASK_ID`** / documented scoped CLI → **`.vibe/active.json`** ), then **read** that file (canonical **`.vibe/sessions/<segment>/current_instruction.md`**, **`.vibe/current_instruction.<segment>.md`**, or the **`@` path** from **`kaopiz-devkit run`**) and the **session** **`state.json`** when you need `task_id` / `step_index` — **execute**; do not stop at stdout alone. **`start`** already wrote step 1 instruction; **`run`** (slash) means **do the current step** per the resolved file — CLI `run` prints the **`@` path** to use. **`approve`** only when the user requests a gate. Before **`approve`**, when the shell is **not** interactive (or whenever you prepare notes ahead of time), follow **`approve.step-notes`**: write **`summary.md`** / **`improvements.md`** in the **task session** folder under **`.vibe/sessions/<task>/`** (or use `--summary-file` / `--improvements-file`). The CLI ingests those into the per-step log under **`.vibe/logs/.../steps/`** and removes the staging copies after a successful approve. To switch which saved session is active without a new `start`, use **`kaopiz-devkit session list`** / **`kaopiz-devkit session activate <task-id>`**.
- **`archive` (slash in Cursor):** run **`kaopiz-devkit archive …`**, then in the **same response** complete the **rollup Agent review** (edit **`### Agent review`** in **`.logs/improvements/workflows/<workflow>.md`**) per **`approve.step-notes`** — *not* “CLI only and ask the user to fill review later.” See **`.agents/commands/kaopiz-devkit.md`** → **`### archive (slash)`**.
- Subcommand table: **`.agents/commands/kaopiz-devkit.md`** (slash command; same content synced from Hub).

## Typing in the Agent window (chat) — not the terminal

If the user **types one line** that looks like a shell command — e.g. `kaopiz-devkit init`, `kaopiz-devkit step`, `kaopiz-devkit run`, **`kaopiz-devkit archive`**, or a prefix like `/kaopiz-devkit sync` — that is a request to **execute the CLI** (and for `step` / `start` / **`run`** / … also **perform the instruction step** as above; for **`archive`** in chat, also **same-turn rollup Agent review** per **`.agents/commands/kaopiz-devkit.md`**), not only to explain. The Agent **must** run shell at the **project root** with the **same subcommand and arguments** (strip a leading `/` if present), then summarize results and continue step content when applicable.

**Map to real commands:** take everything after `kaopiz-devkit` (space + args), append to one of the forms in section 1 — prefer a script named **`kaopiz-devkit`** in `package.json` if present:

- `npm run kaopiz-devkit -- init` ↔ user typed `kaopiz-devkit init`
- `npm run kaopiz-devkit -- start TASK-1 --skill feature_dev` ↔ user typed that full line after `kaopiz-devkit`

If there is no `kaopiz-devkit` script but another script (e.g. `"devkit": "node …/cli.js"`): use that script name with `npm run <name> -- …`. Last resort: `npx kaopiz-devkit …`.

## 1. Basic commands (always from project root)

Prefer this order:

1. If `package.json` has a script pointing at the CLI (recommended name **`kaopiz-devkit`**, or alias like `"devkit"`):  
   `npm run kaopiz-devkit -- <subcommand> ...`  
   (`--` is required to pass args through to the CLI.)
2. If a binary is on PATH: `kaopiz-devkit ...`
3. Fallback: `npx kaopiz-devkit ...`

| Action | Command |
|--------|---------|
| First-time `.vibe/` setup | `… init` (optional `-y`) |
| Start task (+ step 1 + instruction) | `… start <TASK-ID> --skill <router>` |
| Continue / execute current step (Agent reads resolved `current_instruction`; CLI checks state + prints `@` path) | `… run` |
| Open next step (after `approve` when status is `running`) | `… step` |
| Approve a gated step | `… approve` |
| Redo current step | `… reject` |
| Sync Hub → `.agents/` (if needed) | `… sync` or `npx devkit-sync …` (per project docs) |
| Snapshot task + rollup (slash: + Agent review same turn) | `… archive` (optional `--force`, `--skip-session-logs`, `--skip-improvements-rollup`) |

**Approve notes:** Interactive `approve` can write **`summary.md`** (step outcome) and **`improvements.md`** (harness feedback). Use Hub skill **`approve.step-notes`** to draft them before pasting or using `--summary-file` / `--improvements-file`.

`TASK-ID` is a ticket/issue id (Jira, Linear, …). `router` is one of the `*` entries in the table below.

## 2. Pick the right router (`--skill`)

| Kind of work | `--skill` |
|----------------|-----------|
| Day-to-day feature | `feature_dev` |
| Bugfix | `bug_fix` |
| Refactor | `refactor` |
| Merge conflict | `resolve_conflict` |
| Write tests | `write_test` |
| Code review | `code_review` |
| Docs / OpenAPI | `write_docs` |
| Release prep | `release_prep` |
| Incident khẩn (unified) | `bug_fix` |
| Init greenfield — **one session, five steps** (architecture → … → docs) | `init_greenfield` |
| Full task + ingest before plan | `full_task_with_ingest` |
| Migration / investigate | `migration` / `investigate` |

Greenfield: **`start … --skill init_greenfield`** — một workflow năm bước; sau mỗi bước `approve` (CLI thường mở bước kế). `init.enabled_phases` bỏ hẳn các bước thuộc phase tắt — xem `WORKFLOWS-DEV.md`.

Workflow details + diagram: guideline `WORKFLOWS-DEV.md` in devkit-mcp; router JSON: `.agents/vibe-coding/devkit/routers/*.json` (`routes_to_workflow`).

## 3. Working with the resolved workflow (per-step skills)

- **Per-step behavior** comes from the **resolved** instruction file for that task (after `start` / `step`) and the **Hub skill** attached to that step (`phase.*`, `dev.tdd`, …) — see manifests under `.agents/vibe-coding/devkit/workflows/`.
- **Additionally**, when a **logical CLI step** is done (e.g. ingest finished, plan finished), the agent runs **`step`** so state matches the next step.
- If a step requires **user approval**, the agent **does not** self-`approve` — present results and **ask the user**; only run `approve` when the user agrees, then (if needed) `step` again.

## 4. Read current state

- Active orchestrator state lives under **`.vibe/active.json`** + **`.vibe/sessions/<task>/state.json`** (path also printed by CLI after `start`). Legacy **`.vibe/state.json`** at repo root is migrated automatically on first read. Use persisted state to see which workflow step is active and avoid drift from code changes.

## 5. Multi-IDE environments

Same pattern for **Cursor Agent**, **Claude Code**, **Antigravity**, or any agent with **terminal / run command**: this skill describes the **correct commands**; each product maps to its shell tool.

**Summary:** `start` with correct `--skill` → resolve task + instruction path → follow **`current_instruction`** content and the step’s resolved skill → when a CLI step completes, `step` / `approve` / `reject` as appropriate; after **`archive`** in chat, fill rollup **Agent review** in the same turn; read the session **`state.json`** for the **resolved** task when syncing context.
