# kz-greenfield

Shortcut for router `init_greenfield`. Delegates to `/kaopiz-devkit` with `--skill init_greenfield`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill init_greenfield [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
