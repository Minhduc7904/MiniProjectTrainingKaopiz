# DevKit managed output

This folder is **written by `devkit-sync`** / `materialize()` from the Knowledge Hub. The next sync **overwrites** files here.

- **`sync-summary.json`** — inventory of rules, skills, workflows, and commands written in the last run (for auditing and tooling).
- **`workflows/`** — workflow manifests (`*`) as **canonical YAML** (same schema as Hub `*.json`). Target **`antigravity`** also writes an Antigravity-style **YAML** under **`<project>/.agents/workflows/`**.
- **`routers/`** — router JSON (`*` → `routes_to_workflow`). Target **`antigravity`** mirrors each router to **`.agents/skills/<id>/router.json`**.

## Custom vs Hub-managed

| Area | Hub-managed (listed in `sync-summary.json`) | Your customizations (not overwritten unless same id/name) |
| ---- | --------------------------------------------- | ---------------------------------------------------------- |
| Skills | `.cursor/skills/<id>/` contains `.devkit-managed.json` | Same path **without** that marker, or use `.vibe/skills/<id>/` per your stack |
| Rules | Filenames listed under `rules` in the summary | Other `.mdc` files in `.cursor/rules/` |
| Workflows / routers | Only under this `devkit/` tree | — |

