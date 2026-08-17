---
name: kz-test
description: "Shortcut for /kaopiz-devkit start --skill write_test"
---

> DevKit agent command `kz-test`, published as a skill because Codex has no repo-level slash commands.

# kz-test

Shortcut for router `write_test`. Delegates to `/kaopiz-devkit` with `--skill write_test`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill write_test [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
