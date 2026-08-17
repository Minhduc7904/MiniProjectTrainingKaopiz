---
name: kz-spec
description: "Shortcut for /kaopiz-devkit start --skill spec_discovery"
---

> DevKit agent command `kz-spec`, published as a skill because Codex has no repo-level slash commands.

# kz-spec

Shortcut for router `spec_discovery`. Delegates to `/kaopiz-devkit` with `--skill spec_discovery`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill spec_discovery [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
