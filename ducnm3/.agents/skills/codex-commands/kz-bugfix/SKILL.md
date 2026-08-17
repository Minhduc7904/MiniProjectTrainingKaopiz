---
name: kz-bugfix
description: "Shortcut for /kaopiz-devkit start --skill bug_fix"
---

> DevKit agent command `kz-bugfix`, published as a skill because Codex has no repo-level slash commands.

# kz-bugfix

Shortcut for router `bug_fix`. Delegates to `/kaopiz-devkit` with `--skill bug_fix`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill bug_fix [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
