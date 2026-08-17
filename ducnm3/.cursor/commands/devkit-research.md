# devkit-research

**Ingest / Research phase** — gather sources, lock scope, **do not** create `PLAN.md` in this step (per `phase.ingest.*`).

**In Cursor:** slash **`/devkit-research`** (command name matches the file).

---

## Agent — required

1. **Valid Vibe session** (state under **`.vibe/sessions/<task>/state.json`**; resolve **which task** per **`kaopiz-devkit-current-instruction`**: **`--task`** if passed → **chat/context** → **`DEVKIT_TASK_ID`** / documented scoped CLI → **`.vibe/active.json`**; legacy **`.vibe/state.json`** until migrated; `status` not `failed` / `completed`):
   - Read the **resolved** step instruction (canonical **`.vibe/sessions/<segment>/current_instruction.md`**, **`.vibe/current_instruction.<segment>.md`**, or **`@` path** from **`kaopiz-devkit run`**) per **`kaopiz-devkit-current-instruction`**. For **`task_id`** / step context, use that session’s **`state.json`** (or **`kaopiz-devkit session list`** / CLI output) — you do **not** need to `@` a root-only **`state.json`** if it no longer exists after migration.
   - If the current step is **Ingest** (or an ingest skill): follow the **Hub skill** and **Step notes** — write **`.vibe/research/<task-id>.md`** (sanitize the filename), update in-scope / out-of-scope; **do not** create `PLAN.md`.
   - If the current step is **not** ingest: tell the user briefly; you may still do **ad-hoc research** for the ticket but should suggest `step` / `approve` to align with the right phase or `start` again with a suitable router.

2. **No session yet:**
   - Propose or run (when the user agrees):  
     `npm run kaopiz-devkit -- start <task-id> --skill feature_dev`  
     (includes Ingest → Plan → …) or  
     `--skill full_task_with_ingest`  
     if you need **full gates** (Review, Deliver, …).
   - After `start`, the CLI has already opened the first step — read the **resolved** instruction file (same resolution order as above) and perform research as above.

3. **MCP:** if the workflow step suggests `get_task_context`, prefer calling it when `task_id` is available.

---

## See also

- Hub skills: **`phase.ingest.read-sources`**, **`phase.ingest.lock-scope`**
- **`/kaopiz-devkit`**, rule **`kaopiz-devkit-current-instruction`**

This file syncs to **`.cursor/commands/`** and **`.agents/workflows/`** (Antigravity).
