---
name: devkit-answer-interview
description: "Index: choose brownfield vs greenfield workflow-profile slash commands and MCP interview_mode."
---

> DevKit agent command `devkit-answer-interview`, published as a skill because Codex has no repo-level slash commands.

# devkit-answer-interview

**Split flows:** Pick **one** slash that fits the project — there is no longer a single shared checklist.

| Situation | Slash | MCP `interview_mode` |
|-----------|--------|------------------------|
| **Existing codebase** — needs repo reconciliation | **`devkit-workflow-profile-brownfield`** | `"brownfield"` (default if omitted) |
| **New project / greenfield** | **`devkit-workflow-profile-greenfield`** | `"greenfield"` |

**Both** call `collect_workflow_profile_context` → interview per `interview_sections` (**must** cover every `question.id`; **`AskQuestion`** only when UI is needed for `select`/`confirm`/`multiselect`, otherwise structured chat) → **audit every `question.id`** (`answer_sources` + `interview_audit` in the profile, or CLI **`workflow-profile write-with-audit`**) → `finalize_workflow_profile_interview` → remind `devkit-sync`.

**Skill:** `setup.workflow-profile-brownfield` or `setup.workflow-profile-greenfield`.

See the matching slash file under `.agents/commands/` (after `devkit-sync`) for step-by-step detail.
