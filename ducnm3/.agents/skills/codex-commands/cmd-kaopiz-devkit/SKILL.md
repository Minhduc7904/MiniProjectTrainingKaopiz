---
name: cmd-kaopiz-devkit
description: "Orchestrate DevKit CLI (init, start, step, run, approve, archive, …); resolve task (--task if passed → chat/context → DEVKIT_TASK_ID / docs → active.json), then execute per canonical/mirror instructi…"
---

> DevKit agent command `kaopiz-devkit`, published as a skill because Codex has no repo-level slash commands.

# kaopiz-devkit

CLI that orchestrates the DevKit workflow (`init`, `start`, `step`, `run`, `approve`, `reject`, `archive`, `sync`, `doctor`, …). **`start`** opens the first step automatically (writes **`.vibe/sessions/<segment>/current_instruction.md`** and **`.vibe/current_instruction.<segment>.md`** — same family as `step`). **`run`:** for the slash command `/kaopiz-devkit run`, the product meaning is **ask the Agent to execute** per the **resolved** instruction file (see rule **`kaopiz-devkit-current-instruction`**: **`--task`** if passed → chat/context → **`DEVKIT_TASK_ID`** / documented scoped CLI → **`active.json`**, then canonical / mirror / path printed by **`run`**) — **not** “only run the CLI to print one line of guidance”. The **`kaopiz-devkit run`** binary **checks** state/instruction files and **prints** **`Thực thi bước hiện tại theo: @<repo-relative-path>`** — use that **`@` path** as the file to read. **`archive`:** see subsection **### archive (slash)** below — slash `/kaopiz-devkit archive` means **CLI + same-turn Agent rollup review** (edit **`### Agent review`** in the workflow rollup file), not “run CLI only”. State lives under `.vibe/`; sync skills/rules from the Hub with `sync`.

**In chat:** type `/kaopiz-devkit` then subcommands and args — e.g. `/kaopiz-devkit init`, `/kaopiz-devkit step`, `/kaopiz-devkit start TASK-1 --skill feature_dev`.

**Authoring skills (prompt for `SKILL.md`):** use the dedicated slash **`/kaopiz-devkit-skill-authoring`** (recommended), **or** in the **same message** type **`/kaopiz-devkit skill-authoring "description …"`** — the Agent follows playbook **`kaopiz-devkit-skill-authoring.md`** (runs CLI `skill-authoring prompt`, **does not** go through `start`/`step`/`run`).

---

## Instructions for the Agent — resolve task, then the instruction file

This **slash command** content is for the AI Agent, not only humans. When the subcommand is **`start`**, **`step`**, **`run`**, or **`approve`** (and the workflow is **not** `completed` in the **resolved** session state under **`.vibe/sessions/.../state.json`** — or for **`run`** when valid instruction remains), the Agent **must not** stop only because shell ran or because it saw **one line** on stdout.

**Before reading markdown:** resolve **which task** per rule **`kaopiz-devkit-current-instruction`** ( **`--task`** if passed on the CLI → **chat/context** → **`DEVKIT_TASK_ID`** / documented scoped CLI → **`.vibe/active.json`** ). Then open **`.vibe/sessions/<session_segment>/current_instruction.md`** or **`.vibe/current_instruction.<session_segment>.md`**, or use the path after **`@`** from **`kaopiz-devkit run`** stdout.

**In the same response** (after the CLI when there is a command — for **`step`**, even if the CLI refuses because of **`awaiting_approval`**), the Agent must:

1. **Put the resolved instruction file in the execution context** ( **`@`** that path in Cursor, or read it via tools with the repo-relative path).

2. **Print one reminder line** in the response (so the user sees the Agent is on the step), e.g. repeat the **`run`** line or: `Executing the current step per: @.vibe/sessions/<segment>/current_instruction.md` (with the real segment).

3. **Then** do the work (edit files, run project commands, …) per **Hub skill (resolved)** and **Step notes** in that file.

If the subcommand is only **`init` / `sync` / `doctor`** and **no** workflow session with actionable steps is opened, you may **skip** the instruction step (unless the user wants to compare state).

For **`archive`**, you **do not** use the step instruction file for the review pass — follow subsection **`### archive (slash)`** below and Hub skill **`approve.step-notes`** (*After `kaopiz-devkit archive`*).

---

## Agent: slash command = CLI + step content (not “run command only”)

**Only running `npm run kaopiz-devkit -- …` and stopping** is **not enough** for commands tied to a **workflow session** (`start`, `step`, **`run`**, context after **`approve`**, or **`archive`** when a rollup is written). The user expects the Agent to **actually perform** per DevKit, not only print terminal logs.

**Two layers required:**

1. **Terminal (repo root, with `package.json`):** run the subcommand per the table below (prefer `npm run kaopiz-devkit -- …`, `--` is required; fallback `npx kaopiz-devkit …`).
2. **Step content:** when it still concerns an actionable step, the Agent **reads** the **resolved** instruction file (canonical / per-task mirror / **`run`** stdout) and the **session** state file under **`.vibe/sessions/.../state.json`** when needed, and **executes** what it describes — **even when** CLI `step` **cannot** run (see below). Content: resolved Hub skill, step notes, router global rules, MCP hints — via read/edit tools, project commands, etc. **Summarize for the user** what was done or **what is blocked** (permissions, need to pick a stack, …).

Cursor rule **`kaopiz-devkit-current-instruction`** (after `devkit-sync`) reinforces prioritizing the instruction file when state exists.

### `step`: CLI vs “do the step” (Agent)

- **CLI `kaopiz-devkit step`** only runs when the **resolved session** state’s **`status`** is **not** **`awaiting_approval`** (state file: **`.vibe/sessions/<task>/state.json`**; task from **`active.json`** or **`--task` / `DEVKIT_TASK_ID`**). Its job is to **open the step**: re-render instruction files (canonical + mirrors) and set `awaiting_approval`.
- When the user types **`/kaopiz-devkit step`** and the CLI errors like *“Approve or reject the current step first”*: it means the **current step is already open**, instruction **already** on disk for that session. This is **not** a reason for the Agent to **skip** execution — the Agent **still** reads the **resolved** instruction path (e.g. **`run`** or canonical) and **works** from it; only note briefly: the CLI did not need to rewrite the file; step content still comes from the current files.

### `run`: slash command vs CLI

- **`/kaopiz-devkit run`** = user asks the Agent to **do the current step** per the instruction file whose **`@` path** appears on stdout (or the resolved canonical/mirror), **not** only “print a hint and stop”.
- **CLI `kaopiz-devkit run`** checks state + instruction file (exit ≠ 0 if missing), and **prints** one line **`Thực thi bước hiện tại theo: @<path>`** — for scripts/logs aligned with chat. The Agent **must still** read **that** file and execute; stdout **does not** replace the step.

### `approve` (slash / Agent)

- **Approval gate:** when a step is `awaiting_approval`, ask the user to approve via the ask-question tool (**`AskUserQuestion`** / **`AskQuestion`** on Cursor; Approve / Reject; rule **`kaopiz-devkit-current-instruction`** step 3) rather than waiting for a typed `approve`. The phases below run after the user picks **Approve** (typed `approve` still works as fallback).
- **`/kaopiz-devkit approve`** is **two phases** in chat: (1) **Agent** drafts notes, (2) **CLI** finalizes the step and copies notes into **`.vibe/logs/.../steps/...`**.
- **Before** `npm run kaopiz-devkit -- approve`: resolve **`task_id`** / **`session_segment`**, follow Hub skill **`approve.step-notes`**, and **write** **`.vibe/sessions/<session_segment>/summary.md`** and **`improvements.md`** with substantive markdown (not only default headings). Then run approve (add **`--task`** when needed).
- The CLI **does not** synthesize LLM text; non-TTY mode only reads staging files or flags. If the user wants **no** log files for that step, use **`--skip-summary-prompt --skip-improvements-prompt`** explicitly.

### `init`: Agent vs TTY (prompts)

- **Default `init`** uses **prompts** (needs TTY) to ask for profile — in the **Agent window** this usually **cannot** interact like a user terminal.
- **Agent:** prefer **`init -y`** (non-interactive, default profile) — **unless** the user wants to pick a profile **interactively** in the terminal (then the user runs `init` themselves or answers prompts outside chat).
- Slash command / CLI↔Agent handoff design: Cursor rule **`agent-orchestrated-commands`** (file **`.agents/rules/agent-orchestrated-commands.md`** after `devkit-sync`).

### `archive` (slash)

When the user invokes **`/kaopiz-devkit archive`** (with or without `--force`, `--skip-session-logs`, `--skip-improvements-rollup`, etc.) **from Cursor chat**, the intent is **one combined turn**: snapshot via CLI **and** (when rollup applies) **LLM-quality follow-up in the same response** — **not** a second message asking the user to “remember to fill Agent review later.”

1. **Run the CLI** at project root: `npm run kaopiz-devkit -- archive …` (pass through the user’s flags).
2. **If exit ≠ 0:** summarize stderr; **stop** (no file edits).
3. **If stdout shows `Improvements rollup:`** with a path (and not `(already current for this archive — skipped)`):
   - In the **same turn**, open that **`.logs/improvements/workflows/<workflow>.md`** (main file = **one** latest `## Rollup …` section; optional **`Improvements rollup (previous main saved):`** → `workflows/history/<workflow-id>/`) and each linked **`improvements.md`** under the session mirror path printed as **Session logs mirror:** (or under `.vibe/logs/.../steps/` if there was no mirror).
   - **Edit** the rollup: under **`### Agent review`**, **replace** the CLI placeholder paragraph(s) with substantive content per **`approve.step-notes`** → *After `kaopiz-devkit archive`* (Summary, Themes, Prioritized follow-ups, Open questions). **Do not** remove HTML markers or CLI tables above **`### Agent review`**.
   - If stdout says **already current for this archive — skipped**, briefly explain the main rollup file was not rewritten; **do not** duplicate work unless the user asked to refresh the review.
4. **If** `--skip-improvements-rollup` **or** no rollup line in stdout: state that in the summary; **no** rollup edit.
5. End the turn with a **short** user-facing summary: archive path, mirror path if any, and that **Agent review** was written (or why it was skipped).

**CI / scripts / plain terminal without Agent:** only step 1 applies; there is no LLM step — that is expected.

---

## Agent: shell map (prefer `npm run kaopiz-devkit --`)

| User input (after `/kaopiz-devkit`) | Terminal command |
|-------------------------------------|------------------|
| `init` / `init -y` | `npm run kaopiz-devkit -- init` or `… -- init -y` |
| `sync` | `npm run kaopiz-devkit -- sync` |
| `doctor` | `npm run kaopiz-devkit -- doctor` |
| `run` | `npm run kaopiz-devkit -- run` — CLI: check state/file + print `@<instruction-path>` stdout; **Agent:** must read **that path** and **execute** (exit ≠ 0 if no state/file) |
| `step` | `npm run kaopiz-devkit -- step` |
| `approve` | **Agent first:** write substantive **`.vibe/sessions/<task>/summary.md`** & **`improvements.md`** per **`approve.step-notes`**; **then** `npm run kaopiz-devkit -- approve` ( **`--task`** if scoped) |
| `reject` | `npm run kaopiz-devkit -- reject` (add `-m "…"` if the user gives a reason) |
| `start …` *(includes `--skill`)* | Full `start` … fragment as typed |
| `start …` *(missing `--skill`)* | **Do not** run — infer router from below, then insert `--skill` |
| `skill-authoring …` *(with description, same message)* | **Do not** run the rows above — follow **`/kaopiz-devkit-skill-authoring`**: `npm run kaopiz-devkit -- skill-authoring prompt -m "…"` (extract description from quotes or text after `skill-authoring`); show **stdout** to the user. See **`kaopiz-devkit-skill-authoring.md`**. |
| `archive …` | `npm run kaopiz-devkit -- archive …` — then **same-turn** rollup **Agent review** per **`### archive (slash)`** above (unless `--skip-improvements-rollup` or CLI failed) |

---

## Agent: after CLI — what next (by subcommand)

| Subcommand | Agent must / should (see column; `step` applies even when CLI exits ≠ 0 because of `awaiting_approval`) |
|------------|------------------------------------------|
| **`init`**, **`sync`**, **`doctor`** | Summarize output; usually **no** feature implementation — only report changed files/rules if any. For **`init`:** Agent prefers **`init -y`** (see **`init`: Agent vs TTY** above). |
| **`start`** | CLI already **attached the first step** (writes canonical + mirrors). Agent reads the **resolved** instruction file and executes; **no** separate `step` for step 1 unless stderr shows an error. |
| **`step`** | Try CLI `step`. **Exit 0:** read the **resolved** instruction file just written, **execute** the step (code, structure, config, …); do not stop at “ran step”. **Exit ≠ 0** because **`awaiting_approval`:** treat instruction as **already present** — read current file + **session state** ( **`.vibe/sessions/.../state.json`** ), **execute** as above; explain: CLI `step` is blocked because the step is already open, not because there is no guidance. If information is missing (stack, ticket), **ask the user** or log a backlog item. |
| **`approve`** | Only when the user **explicitly** asks (human approval gate). **Before** the approve CLI: draft **`summary.md`** / **`improvements.md`** under **`.vibe/sessions/<segment>/`** per **`approve.step-notes`** (substantive, not stubs). After approve: if the workflow is not finished, **re-read** the **resolved** instruction file (new step) and repeat `step` logic if the user wants to “continue the new step” in the same session; if **completed**, report completion. |
| **`reject`** | Summarize; do not force-continue the workflow step. |
| **`run`** | **Required:** read the instruction file indicated by **`run`** stdout (and **session state** when needed) and **execute** the step — this is the main meaning of slash `run`. Run CLI `run` to validate the session and get the **`@` path**; **do not** stop after seeing that line alone. |
| **`archive`** | **Slash / Cursor:** follow **`### archive (slash)`** — CLI then **same-turn** edit of **`### Agent review`** in the rollup file when improvements rollup ran. **Terminal-only:** CLI only. |

**Note:** `approve` **finalizes a step** — the Agent **must not** `approve` for the user when the step requires human confirmation; only run the CLI when the user (or an explicit instruction) asks.

---

## Agent: `start` — missing `--skill`

The binary **always** needs `--skill`. When the user omits it:

1. Read context (ticket, description, type of work).
2. **Optional but recommended:** MCP **`suggest_common_task_routers`** with `query` from the task (and `project_root` if you have the repo path — merges `daily_workflow` hints into the response).
3. Pick `id` only from real files: **`.agents/vibe-coding/devkit/routers/*.json`** (after `devkit-sync`) — must match an allowed router id (MCP does not invent ids).
4. If your pick **differs** from profile `preferred_router_skill_id` or the user insists on a different router: **ask in chat** to confirm before running `start` (no TTY prompt).
5. Run `… start <task-id> --skill <id>`. The CLI may print **`warning:`** lines on stderr when `--skill` disagrees with profile hints — that does **not** block; for automation use **`--quiet-router-hints`**. See **`guideline/WORKFLOWS-DEV.md`** if needed.
6. If unsure: **ask the user** to pick a router, then run.

More docs: **`guideline/SKILL-DISCOVERY-WORKFLOW.md`**, **`packages/kaopiz-devkit/docs/user-guide/CLI-REFERENCE.md`** (`start` section).

---

## Per-task workflow: each command, processing flow, where to verify output

Use this when answering: *for one `task_id`, what does each subcommand do, what is the processing order, and where do I look (terminal vs files)?*

**Resolve the session first:** `session_segment` usually equals the sanitized task id folder under **`.vibe/sessions/<segment>/`**. Confirm with **`.vibe/active.json`** when using the default active task.

### Typical lifecycle (happy path)

```text
start → (work per current_instruction.md) → approve → step → (work) → approve → … → completed
run — anytime: validate session + print @path to current instruction (does not replace doing the work in chat)
```

- **`start`** creates/opens the session and **renders the first step** (instruction files on disk).
- Between CLI calls, the **Agent** (chat mode) does the real work per **`current_instruction.md`**.
- **`run`** validates state and prints the **`@`** path to the same instruction file; it does not replace doing the work in chat.
- **`step`** advances to the **next** step after the current one was approved (or when the CLI allows opening the next step).
- **`approve`** finalizes the current step and moves the workflow forward (next instruction or `completed`).

### Command-by-command: flow, side effects, where to check

| Subcommand | What the CLI does (high level) | Main files / dirs touched | Check success here |
|------------|--------------------------------|---------------------------|-------------------|
| **`start <id> --skill …`** | Creates/updates session for `task_id`, flattens workflow, **writes first step** instruction. | **`.vibe/sessions/<segment>/state.json`**, **`current_instruction.md`**, **`.vibe/current_instruction.<segment>.md`**, **`.vibe/active.json`** | Exit **0**. Read **`state.json`**: `status`, current step id. Open **`current_instruction.md`**. stderr may show **`warning:`** (router vs profile) — often non-fatal. |
| **`run`** | Asserts session + instruction exist; **prints one line** with repo-relative `@` path. | No rewrite of instruction by default; read-only check. | Exit **0** + stdout: **`Thực thi bước hiện tại theo: @…`**. Exit **≠ 0** → missing session/instruction or bad state (see stderr). |
| **`step`** | If allowed, **opens next step**: re-renders **`current_instruction.md`**, may set **`awaiting_approval`**. | Updates **`state.json`**, instruction files. | Exit **0** → new step; re-read **`current_instruction.md`**. Exit **≠ 0** (e.g. must approve first) → instruction **may still be valid on disk**; read canonical path anyway (see `step` subsection above). |
| **`approve`** | Finalizes step, copies staged **`summary.md`** / **`improvements.md`** into **`.vibe/logs/.../steps/...`** when present; advances workflow. | **`.vibe/sessions/<segment>/summary.md`**, **`improvements.md`** (inputs); **`.vibe/logs/`** (outputs); **`state.json`**; next **`current_instruction.md`** if not completed. | Exit **0**. Inspect **`state.json`** (`status`, completed vs next step). If not completed, open new **`current_instruction.md`**. |
| **`reject`** | Records rejection; does **not** pretend the step succeeded. | **`state.json`**, logs under **`.vibe/logs/`** (layout depends on CLI version). | Exit **0** + message; read **`state.json`** and CLI stdout/stderr. |
| **`archive`** | Snapshots session / logs; may print **Improvements rollup:** path. | Archive tree under **`.vibe/`** (and paths printed on stdout); optional **`.logs/improvements/workflows/...`** for rollup. | Exit **0**. Read **stdout** for **`Improvements rollup:`**, **`Session logs mirror:`**, archive path. In chat, edit **`### Agent review`** in rollup when applicable. |

**Setup / utility (not step-by-step task flow):**

| Subcommand | What the CLI does | Where to check |
|------------|-------------------|----------------|
| **`init` / `init -y`** | Creates **`.vibe/`** skeleton and default profile. | **`.vibe/`** exists; **`state.json`** may be absent until **`start`**. Prefer **`-y`** in Agent chat (no TTY). |
| **`sync`** (devkit CLI) | Project sync path for devkit package (not **`devkit-sync`** Hub materialize). | CLI stdout; project files per subcommand help. |
| **`doctor`** | Environment / config diagnostics. | Exit code + stdout/stderr summary. |

### Where output lives (quick reference)

| Output kind | Location |
|-------------|----------|
| **Instruction for the current step** | **`.vibe/sessions/<segment>/current_instruction.md`** (canonical), **`.vibe/current_instruction.<segment>.md`** (mirror) |
| **Session machine state** | **`.vibe/sessions/<segment>/state.json`** (`status`, step metadata, `awaiting_approval`, …) |
| **Active task pointer** | **`.vibe/active.json`** |
| **Per-step approve notes (staging)** | **`.vibe/sessions/<segment>/summary.md`**, **`improvements.md`** |
| **Copied step logs after approve** | **`.vibe/logs/<workflow>/…/steps/…`** (exact path echoed by CLI / docs) |
| **Terminal** | **stdout** (human messages, `@` line from **`run`**, rollup paths from **`archive`**); **stderr** (warnings, errors) |

### Agent (chat) vs terminal-only

- **Chat:** after any of **`start`**, **`run`**, **`step`** (even when **`step`** exits non-zero for approval), the Agent still **reads** the resolved instruction and **performs** the step, then summarizes. **`approve`** requires **prior** substantive **`summary.md`** / **`improvements.md`** when using the staging workflow.
- **Terminal-only:** rely on **exit code** + **stdout/stderr** + on-disk files above; no automatic “do the step” behavior.

---

Orchestration details and router table: skill **`@kaopiz-devkit`** after `devkit-sync`.

**Note:** This file syncs from the Hub to **`.agents/commands/`** and **`.agents/workflows/`** (same markdown, alongside workflow YAML for Antigravity).
