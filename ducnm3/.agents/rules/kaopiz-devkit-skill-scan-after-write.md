---
description: After creating or materially editing SKILL.md under .agents/skills — run skill-scanner (kaopiz-devkit skill-authoring scan)
globs: .agents/skills/**/SKILL.md
alwaysApply: false
---

# Skill just written — run skill-scanner

When you (the Agent) **just created or materially updated** this file (`SKILL.md`) in the same working session:

1. **Required** — run from **repo root** (where `package.json` is):

   `npm run kaopiz-devkit -- skill-authoring scan <parent-directory-of-SKILL.md>`

   Example: if the file is `.agents/skills/my-skill/SKILL.md`, `<parent-directory>` = `.agents/skills/my-skill`.

2. **Include the full report** (stdout) in the response to the user — **in the same turn** or **immediately after**, before considering the “create skill” task done.

3. If the command fails because **no** `skill-scanner` binary (or wrong Python arch): write **one clear paragraph** for the user — install scanner / set `SKILL_SCANNER_BIN`; see **devkit-handbook** `docs/SCRIPTS-AND-DATA.md`. **Do not** skip this without reporting.

4. Optional: `skill-authoring scan --json` if the user needs raw JSON; default is text report.

**Forbidden:** Ending a “skill created” task **without** running scan **and** without explaining an environment failure.
