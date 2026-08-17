---
name: kz-investigate
description: "Shortcut for /kaopiz-devkit start --skill investigate"
---

> DevKit agent command `kz-investigate`, published as a skill because Codex has no repo-level slash commands.

# kz-investigate

Shortcut for router `investigate`. Delegates to `/kaopiz-devkit` with `--skill investigate`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill investigate [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
