---
name: kz-compact
description: "Shortcut for /kaopiz-devkit start --skill task_compact"
---

> DevKit agent command `kz-compact`, published as a skill because Codex has no repo-level slash commands.

# kz-compact

Shortcut for router `task_compact`. Delegates to `/kaopiz-devkit` with `--skill task_compact`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill task_compact [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
