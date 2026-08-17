---
name: kz-compact-ingest
description: "Shortcut for /kaopiz-devkit start --skill task_compact_with_ingest"
---

> DevKit agent command `kz-compact-ingest`, published as a skill because Codex has no repo-level slash commands.

# kz-compact-ingest

Shortcut for router `task_compact_with_ingest`. Delegates to `/kaopiz-devkit` with `--skill task_compact_with_ingest`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill task_compact_with_ingest [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
