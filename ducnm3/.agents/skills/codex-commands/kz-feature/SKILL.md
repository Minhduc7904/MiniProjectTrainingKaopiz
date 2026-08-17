---
name: kz-feature
description: "Shortcut for /kaopiz-devkit start --skill feature_dev"
---

> DevKit agent command `kz-feature`, published as a skill because Codex has no repo-level slash commands.

# kz-feature

Shortcut for router `feature_dev`. Delegates to `/kaopiz-devkit` with `--skill feature_dev`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill feature_dev [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
