# kz-full-ingest

Shortcut for router `full_task_with_ingest`. Delegates to `/kaopiz-devkit` with `--skill full_task_with_ingest`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill full_task_with_ingest [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
