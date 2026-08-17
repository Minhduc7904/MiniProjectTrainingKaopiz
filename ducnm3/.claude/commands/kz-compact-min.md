# kz-compact-min

Shortcut for router `task_compact_min`. Delegates to `/kaopiz-devkit` with `--skill task_compact_min`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill task_compact_min [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
