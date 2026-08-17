---
name: kz-refactor
description: "Shortcut for /kaopiz-devkit start --skill refactor"
---

> DevKit agent command `kz-refactor`, published as a skill because Codex has no repo-level slash commands.

# kz-refactor

Shortcut for router `refactor`. Delegates to `/kaopiz-devkit` with `--skill refactor`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill refactor [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
