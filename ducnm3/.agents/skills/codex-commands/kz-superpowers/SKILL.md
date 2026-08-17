---
name: kz-superpowers
description: "Shortcut for /kaopiz-devkit start --skill feature_with_superpowers"
---

> DevKit agent command `kz-superpowers`, published as a skill because Codex has no repo-level slash commands.

# kz-superpowers

Shortcut for router `feature_with_superpowers`. Delegates to `/kaopiz-devkit` with `--skill feature_with_superpowers`.

## Agent

- `start <TASK_ID> [flags]` → `npm run kaopiz-devkit -- start <TASK_ID> --skill feature_with_superpowers [flags]`
- Other subcommands → pass-through to `npm run kaopiz-devkit -- <subcommand> [args...]`
- If user passes `--skill`: error, do not run
- After CLI: follow `/kaopiz-devkit` behavior
