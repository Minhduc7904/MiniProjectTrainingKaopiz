# devkit-sync

CLI **materializes** the Hub into `.claude/` (rules, skills, workflows, routers, **commands**) and **`.agents/workflows/`** (same command markdown as `.md` files next to workflow YAML for Antigravity), and mirrors `.claude/skills/` when configured. Reads `devkit.config.json`.

**In Cursor:** slash **`/devkit-sync`** or `/devkit-sync` with CLI flags like `--help` — e.g. `/devkit-sync --source mcp`, `/devkit-sync --targets cursor`.

---

## Agent: run this in the terminal (project root)

When the user invokes this slash command, **run the real CLI** at the **repo root** (where `package.json` / `devkit.config.json` live), then summarize stdout/stderr.

### 1) Default sync (bundled, full targets) — common after `install-all.sh`

Many projects define **`devkit:sync`** as `node node_modules/devkit-sync/dist/cli.js --project . --source bundled --targets cursor,claude`.

- If the user types `/devkit-sync` with no extra flags, prefer:  
  `npm run devkit:sync`  
  (do not append extra args — the script is fixed in `package.json`.)

### 2) Custom `--source`, `--mcp-url`, `--hub`, `--profile`, `--targets`, etc.

The npm script may **not** forward extra args depending on how it is declared — then call the CLI **directly**:

```bash
npx devkit-sync --project . <args>
# or
node node_modules/devkit-sync/dist/cli.js --project . <args>
```

**Mapping hint:** text after `/devkit-sync` in chat (strip the slash) — if it looks like CLI flags, append to one of the lines above (always include `--project .` unless the user specifies another path).

| User input (after `/devkit-sync`) | Shell hint |
|-----------------------------------|------------|
| *(empty)* or `sync` | `npm run devkit:sync` if the script exists; else `npx devkit-sync --project . --source bundled --targets cursor,claude` |
| `--help` | `npx devkit-sync --help` |
| `--source mcp` | `npx devkit-sync --project . --source mcp` (add `--mcp-url` if the user has a URL) |
| `--targets cursor` | `npx devkit-sync --project . --targets cursor` |

### Priority order (general)

1. `npm run devkit:sync` — **only when** the user wants a single “standard install-all” sync without changing flags.
2. `npx devkit-sync …` or `node node_modules/devkit-sync/dist/cli.js …`
3. Binary on PATH: `devkit-sync …`

**After sync:** you may remind the user to open **`@kaopiz-devkit`** / new skills if workflow-related.

**Note:** This file is synced to **`.claude/commands/`** and **`.agents/workflows/`**; see `packages/devkit-sync/docs/README.md` in the devkit-mcp repo for package details.

**Antigravity slash menu:** when **`--targets`** includes **`antigravity`**, the same command body is written under **`.agents/workflows/<name>.md`** with an auto-prepended YAML block **`name`** + **`description`** (from each command’s **`title`** / **`summary`** in `agent-commands/*.json`) so slash discovery works; **`.claude/commands/`** copies stay unchanged (no extra frontmatter).
