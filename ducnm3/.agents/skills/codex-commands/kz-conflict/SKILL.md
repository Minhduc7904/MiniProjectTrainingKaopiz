---
name: kz-conflict
description: "Shortcut for /kaopiz-devkit start --skill resolve_conflict"
---

> DevKit agent command `kz-conflict`, published as a skill because Codex has no repo-level slash commands.

# kz-conflict

Shortcut for router `resolve_conflict`. Delegates to `/kaopiz-devkit` with `--skill resolve_conflict`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill resolve_conflict [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
