---
name: devkit-verify
description: "Verify phase: run tests, lint/format, and compliance checks before treating the step as done."
---

> DevKit agent command `devkit-verify`, published as a skill because Codex has no repo-level slash commands.

# devkit-verify

**Verify phase** — run tests, lint/format (per project config), check compliance / quality before treating the step as done.

**In Cursor:** slash **`/devkit-verify`**.

---

## Agent — required

1. **Valid Vibe session:**
   - Resolve **which task** and **which instruction file** per rule **`kaopiz-devkit-current-instruction`** (`--task` if passed → chat/context → `DEVKIT_TASK_ID` / documented scoped CLI → **`.vibe/active.json`**); read **`.vibe/sessions/<segment>/current_instruction.md`**, **`.vibe/current_instruction.<segment>.md`**, or the **`@` path** from **`kaopiz-devkit run`**.
   - If the step is **Test & compliance** / **Verify**: follow the skill (e.g. `phase.verify.run-tests`, `phase.verify.compliance`) and **Step notes**; call MCP **`verify_compliance`** when available with a suitable artifact/checklist.
   - If the step is different: you may still **run verify ad-hoc** (test/lint) per section 2, and tell the user to align with the workflow (`step`/`approve`) if needed.

2. **Project commands (prefer synced config):**
   - Read **`devkit.config.json`** and/or **`package.json`** scripts.
   - If **`.agents/vibe-coding/devkit/sync-summary.json`** exists, you may reference the synced profile; usually run:
     - `npm test` (or `coverage` if the user asks)
     - `npm run lint` / `npm run format` if scripts exist
   - On shell reject / command-not-run / non-zero exit: report step as failed immediately; do not mark pending and continue.
   - Always write **`.vibe/sessions/<task>/VERIFY-EVIDENCE.json`** with records containing:
     - `command` (string)
     - `exit_code` (integer)
     - `timestamp` (ISO-8601)
     - `commit_sha` (40-char git SHA)
     - `evidence_path` (repo-relative path)
   - `Not run` is only valid with explicit waiver metadata (reason + approver + timestamp).
   - Short report: pass/fail, main failing files, next steps (fix → Execute again, or `approve` if green).

3. **Do not replace** the user’s **`approve`** gate — only run checks and suggest.

---

## See also

- **`/kaopiz-devkit`**, workflow **`phase.verify`**
- Skills **`phase.verify.run-tests`**, **`phase.verify.compliance`**

This file syncs to **`.agents/commands/`** and **`.agents/workflows/`** (Antigravity).
