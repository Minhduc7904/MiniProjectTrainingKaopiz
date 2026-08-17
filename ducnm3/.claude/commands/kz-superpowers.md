# kz-superpowers

Shortcut for router `feature_with_superpowers`. Delegates to `/kaopiz-devkit` with `--skill feature_with_superpowers`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill feature_with_superpowers [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
