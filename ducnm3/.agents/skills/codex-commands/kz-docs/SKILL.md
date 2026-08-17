---
name: kz-docs
description: "Shortcut for /kaopiz-devkit start --skill write_docs"
---

> DevKit agent command `kz-docs`, published as a skill because Codex has no repo-level slash commands.

# kz-docs

Shortcut for router `write_docs`. Delegates to `/kaopiz-devkit` with `--skill write_docs`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill write_docs [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
