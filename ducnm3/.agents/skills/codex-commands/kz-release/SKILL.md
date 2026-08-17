---
name: kz-release
description: "Shortcut for /kaopiz-devkit start --skill release_prep"
---

> DevKit agent command `kz-release`, published as a skill because Codex has no repo-level slash commands.

# kz-release

Shortcut for router `release_prep`. Delegates to `/kaopiz-devkit` with `--skill release_prep`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill release_prep [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
