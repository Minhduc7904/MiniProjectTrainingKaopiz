# Mandatory Workflow Skills

- Classify the requested change before editing any file.
- All skills are in `.agents/skills/`. Read that folder; do not use `skills/`.
- Read `SKILL.md`, `reference.md`, and `template.md` from every matching
  MiniProject workflow folder listed in `AGENTS.md`.
- For `interface-design`, read `.agents/skills/interface-design/SKILL.md`
  and `rules/frontend.md` before frontend or product UI work.
- For a new admin page that calls one API, also read
  `.agents/skills/frontend-api-page/` (`SKILL.md`, `reference.md`,
  `template.md`). Mẫu là trang Sổ học viên.
- Do not use the generic legacy `add-endpoint` workflow.
- Never combine GET detail, GET list, POST, PUT, PATCH, or DELETE instructions
  into one endpoint skill.
- Read the unit, component, and integration test skills independently when those
  test types are required.
- Read `.agents/skills/database-migration/` before creating, editing, applying,
  or scaffolding a database migration.
- From Day 3 onward, read `.agents/skills/developer-task/` before Jira/WBS
  work, estimate updates, ticket branches, push, or PR review. An endpoint
  still requires its HTTP-method skill plus tests; `developer-task` is
  additional, not a substitute.
- Treat missing required skill reading as a blocker: stop and read it before
  coding.
