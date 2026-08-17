> kaopiz-devkit session — resolve task (--task → chat/context → DEVKIT_TASK_ID/docs → active.json); instruction contract is per-session files under .vibe/sessions/

# Kaopiz DevKit — current step (`current_instruction`)

## Interaction principle — ALWAYS use the ask-question tool (no plain-text gates)

**For ANY point where you need the user to decide** — step approval, PLAN approval, brainstorm clarifications, scope confirmation, choosing between approaches — you **MUST** call the editor's native ask-question tool. **NEVER** ask the question as plain chat text and wait for the user to type a reply.

- **Tool name by editor:** `AskUserQuestion` (opencode, Claude Code) · `AskQuestion` (Cursor). Use whichever your editor exposes.
- **Why:** native modal = fewer tokens (no long typed answers re-fed into context) + consistent UI. Free-text is still available via the built-in "type your own answer" option.
- **Rule:** always use the ask-question tool, never ask gating questions as plain text. Ask one question per call (options + optional free-text). Only fall back to typed input if the tool is genuinely unavailable in the current editor/mode.
- **Cursor note:** the `AskQuestion` tool is reliable with GPT-5.3 Codex / Claude / Haiku models; the `composer-1.5` model may not expose it in Agent mode — use another model or Plan mode there. Typed `approve` / CLI remains the universal fallback.

## Resolve which task / session (order)

Before opening instruction markdown, decide **which workflow task** you are executing:

1. **`--task <id>`** on the **`kaopiz-devkit`** command (when the Agent or user runs the CLI with this flag) — **wins for that invocation**.
2. **Chat / context:** explicit `task_id` or ticket in the user message, thread subject, or **tool / MCP payload** that supplies a `task_id` (must match a session under `.vibe/sessions/` after `start`, or the user is starting that id next).
3. **Project config / environment:** `DEVKIT_TASK_ID` (or other **documented** defaults for scoped CLI in this repo) — same convention as `kaopiz-devkit --task` when documented.
4. **Default:** **`.vibe/active.json`** → `task_id` and `session_segment` (directory name under `.vibe/sessions/`).

If the **resolved** task differs from **`active.json`**, align before executing: **`kaopiz-devkit session activate <task-id>`** and/or pass **`--task <id>`** on CLI so state and files match.

**CLI vs Agent:** the **`kaopiz-devkit`** binary only applies **`--task`** → **`DEVKIT_TASK_ID`** → **`.vibe/active.json`** (no chat). After you resolve task from **chat/context** (steps 2–4), pass **`--task <id>`** on the CLI when that id must override env or active.

## Where instruction markdown lives (contract)

| Path | Role |
|------|------|
| **`.vibe/sessions/<session_segment>/current_instruction.md`** | **Canonical** — this is the instruction contract; use for reads / `@` (with the real `session_segment`). |
| **`.vibe/current_instruction.<session_segment>.md`** | Per-task mirror (same body as canonical; useful when you know `task_id` but not only active session). |

**Optional:** the CLI may still write **`.vibe/current_instruction.md`** as a convenience copy for some setups — **do not** treat it as the primary handoff path; always prefer **session canonical** (or the **`@` path** from **`kaopiz-devkit run`** stdout, which resolves to canonical first).

**Practical:** run **`npm run kaopiz-devkit -- run`** (add **`--task <id>`** when scoped) and read the repo-relative path printed after **`@`**; or open **`.vibe/sessions/<session_segment>/current_instruction.md`** using `session_segment` from **`active.json`** or from the resolved task.

## Model (matches CLI)

- **`kaopiz-devkit start … --skill …`** writes **`.vibe/sessions/<segment>/state.json`**, sets **`.vibe/active.json`**, and renders the current step into **canonical** + per-task mirror under **`.vibe/`**. Legacy **`.vibe/state.json`** at the repo root is migrated automatically the first time CLI reads state.
- **`kaopiz-devkit step`** re-renders instruction files for the **resolved** session and sets `awaiting_approval` after you finish work for that step.
- **`kaopiz-devkit approve`** finalizes the current step and **rewrites** instruction content for the **next** step (if any), or “Completed” content when finished.

## What the Agent must do

When a **session** exists for the **resolved task** ( **`.vibe/sessions/<segment>/state.json`**, or legacy **`.vibe/state.json`** until migrated) and `status` is **not** `failed`:

1. **Read the instruction** from **`.vibe/sessions/<session_segment>/current_instruction.md`** (or **`.vibe/current_instruction.<session_segment>.md`**, or the path from **`run`** stdout).
2. Prefer **Hub skill (resolved)** and **Step notes** in that file; combine with **Project rules** in the same file and repo-wide rules when they do not conflict.
3. When **`state.status`** becomes **`awaiting_approval`** and your work for the step is done: **MUST ask the user to approve via the ask-question tool** (`AskUserQuestion` / `AskQuestion` — see the interaction principle at the top), NOT plain text. Ask `Approve step <title>?` with options **Approve** / **Reject** (add more options when the step needs a multi-choice decision). Only fall back to typed `approve` / CLI if the tool is unavailable.
4. On the user's answer:
   - **Approve** → follow **`approve.step-notes`** and **write** **`.vibe/sessions/<session_segment>/summary.md`** + **`improvements.md`** with substantive content (not heading-only stubs), **then** run **`kaopiz-devkit approve`** (with **`--task <id>`** when not active-only). The CLI copies these into **`.vibe/logs/.../steps/...`** on success.
   - **Reject** → do **not** approve; stay on the step and address the user's reason.
5. After **`step`** or **`approve`** runs, **re-read** the **same resolved instruction path** because it may have moved to a new step.

If the user explicitly says otherwise (e.g. skip step, change task), follow the user.

## Parallel sessions — always use `--task`

Multiple tasks can run simultaneously on the same repo. Each task has its own instruction file and session state; **`active.json` points to only one at a time**.

### Instruction files per task

| File | Owner |
|------|-------|
| `.vibe/current_instruction.md` | The task that last ran `start` without `--no-update-active` (root facade) |
| `.vibe/current_instruction.<segment>.md` | Per-task stable mirror — always written, safe to @-mention in any window |
| `.vibe/sessions/<segment>/current_instruction.md` | Canonical source (per session dir) |

Every instruction file starts with:
```
**Task:** <task-id> …
```

### Detecting the current task ID

Before running any session-scoped devkit command (`run`, `step`, `approve`, `reject`):

1. Read whichever instruction file is in the current context (root facade or per-task mirror).
2. Extract the `task_id` from the **`**Task:**`** line — the first token after the colon is the ID (e.g. `**Task:** TASK-123 Fix login bug` → `TASK-123`).
3. **Always pass `--task <task-id>`** to the command — this is safe for both single and parallel sessions.

### Examples

```bash
# Extracted task-id from instruction file → TASK-123
kaopiz-devkit step   --task TASK-123
kaopiz-devkit run    --task TASK-123
kaopiz-devkit approve --task TASK-123

# If also using DEVKIT_TASK_ID env var (equivalent)
DEVKIT_TASK_ID=TASK-123 kaopiz-devkit approve
```

### When no instruction file is in context

If the user asks to run a devkit command and no instruction file has been read yet:

1. List running sessions: check `.vibe/sessions/*/state.json` for `”status”: “running”` or `”status”: “awaiting_approval”`.
2. If exactly one running session → use that `task_id`.
3. If multiple running sessions → ask the user which task this window is working on before proceeding.
