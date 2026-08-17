# kz-pr-comments

Shortcut for router `fix_pr_comments`. Delegates to `/kaopiz-devkit` with `--skill fix_pr_comments`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill fix_pr_comments [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
