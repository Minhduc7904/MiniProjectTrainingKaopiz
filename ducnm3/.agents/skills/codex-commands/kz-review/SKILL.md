---
name: kz-review
description: "Shortcut for /kaopiz-devkit start --skill code_review"
---

> DevKit agent command `kz-review`, published as a skill because Codex has no repo-level slash commands.

# kz-review

Shortcut for router `code_review`. Delegates to `/kaopiz-devkit` with `--skill code_review`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill code_review [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
