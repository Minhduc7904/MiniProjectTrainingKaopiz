# kaopiz-devkit-skill-authoring

Build a **markdown prompt** to author an Agent skill (`SKILL.md`): merge your description with excerpts from the devkit-hub *writing-skills* and *hub-skill-layout* guides (CLI **`kaopiz-devkit skill-authoring prompt`**). The layout guide covers **progressive disclosure** (`SKILL.md` + optional `references/`, `scripts/`).

**After writing `SKILL.md`:** Cursor rule **`kaopiz-devkit-skill-scan-after-write`** (glob `.claude/skills/**/SKILL.md`) requires the Agent to run **`skill-authoring scan`** — see **REQUIRED** below.

**In chat:** type **`/kaopiz-devkit-skill-authoring`**, then on the **same line or the next line** describe the skill — for example:

- `/kaopiz-devkit-skill-authoring` PR review skill for Node backend, basic security checklist
- `/kaopiz-devkit-skill-authoring "create skill abc for deploy workflow"`

**Aligned with `/kaopiz-devkit`:** if the user types **`/kaopiz-devkit skill-authoring …`** or **`/kaopiz-devkit skill-authoring "…"`** in **one message**, the Agent follows the **same flow** as this slash (see appendix in **`kaopiz-devkit.md`**).

---

## YOU (AI)

### REQUIRED — after creating / saving a skill on disk

If in this session you **wrote** or **just finished editing** a skill directory (with `SKILL.md`) — **before you finish**:

1. Run: `npm run kaopiz-devkit -- skill-authoring scan <skill-directory-path>`
2. Give the user the **report** (stdout).
3. If the scanner cannot run: state the environment issue clearly (missing `skill-scanner` / `SKILL_SCANNER_BIN`) — **do not** stay silent.

(Same content as rule **`kaopiz-devkit-skill-scan-after-write`** after `devkit-sync`.)

---

1. **Extract the “skill description”** from the user message:
   - Text after the slash command name (trim leading space), **or**
   - A string in double quotes `"..."` or `「...」` if present.
2. If **nothing** remains (user only typed the slash): **ask one question** — *"Short description of the skill (when to use, what problem)?"* — then repeat step 1.
3. **Run the terminal** at **repo root** (directory with `package.json`):

   - Prefer:  
     `npm run kaopiz-devkit -- skill-authoring prompt -m '<message>'`  
     (use **single quotes** around the message when bash special chars appear; if the message contains `'`, use a temp file `-m "$(cat .tmp-skill-msg.txt)"`, or escape per shell.)
   - Fallback: `npx kaopiz-devkit skill-authoring prompt -m "..."` (when `node_modules` has `kaopiz-devkit`).

4. **Put the full stdout** (markdown prompt) in the chat response — this is the **main artifact**. Remind the user they can paste the prompt into the same or another LLM thread, or save `skill-authoring-prompt.md`.

5. **Scan after `SKILL.md` exists:** see **REQUIRED** above. You may combine: **`… skill-authoring prompt -m "…" --scan .claude/skills/<id>`** if the path is known upfront.

6. **Optional** (only if the user explicitly asks): from the prompt, draft **`SKILL.md`** at a valid path (e.g. `.claude/skills/<id>/SKILL.md` or Hub staging — repo-dependent).

7. **Scope:** this command **does not** open a Vibe session (`start`/`step`/`run`). **Not required** to read `@.vibe/sessions/<task>/current_instruction.md` unless the user combines with a workflow step that requires it.

### When the CLI fails

- Try **`npx kaopiz-devkit`** from repo root; if it still fails: call MCP **`fetch_skill_authoring_guide`** (`{}` for default *writing-skills*, `{ "part": "hub-skill-layout" }` for folder layout only, or `{ "part": "all" }` for the full bundle — large), merge with the user description into a manual prompt (less complete than CLI `skill-authoring prompt` but usable).

### Forbidden

- Stop only because “the command ran” without giving **stdout content** (or error + fallback) to the user.
- **`approve`** / **`reject`** on behalf of the user for a DevKit session.

---

**Reference:** [CLI-REFERENCE.md](../../../../../kaopiz-devkit/docs/user-guide/CLI-REFERENCE.md) (`skill-authoring prompt` / `scan` / `guide`) in the **kaopiz-devkit** package (monorepo).
