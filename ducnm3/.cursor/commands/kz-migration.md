# kz-migration

Shortcut for router `migration`. Delegates to `/kaopiz-devkit` with `--skill migration`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill migration [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
