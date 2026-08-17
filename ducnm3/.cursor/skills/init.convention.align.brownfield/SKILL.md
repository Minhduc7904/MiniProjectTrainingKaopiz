---
name: init.convention.align.brownfield
description: Use to audit the lint/format/commit-hook/test scaffolding already in a brownfield repo and gap-fill only what is missing — never rewrite tools the team already adopted. New tools land in warn-only / changed-files-only mode so a 500-file legacy codebase does not flood CI with errors on day one. Trigger for "set up linting on this existing repo", "gap-fill conventions", "add prettier but don't break CI", "introduce husky without rewriting the whole repo".
when_to_use: Step 3 of setup.init.brownfield. Replaces the greenfield "scaffold" step. Skip entirely if init.enabled_phases excludes scaffold.
license: MIT
---

# Init Brownfield — Convention Alignment

The team already has habits. Your job is to **check** what quality scaffolding is in place, fill the smallest set of gaps, and introduce new tools in a way that does not drown the repo in red.

## Iron Law — Respect the team's choices

If the repo uses Prettier, don't propose Biome. If it uses Jest, don't propose Vitest. Gap-fill means "add what is missing", not "swap what exists for your preference". Every proposed change must be justified by absence, not taste.

## Inputs & Tool Discipline

Reads `.vibe/research/brownfield-init.md`:

- **§3 Quality scaffolding** → presence / config file paths for lint, format, hooks, test runner, coverage.
- **§7 Gaps** → explicit absences to gap-fill.

If either is missing, stop and request a re-run of discover. Do not re-scan the repo for tooling — discover already captured presence vs absence. When resolving conflicting configs (see Conflict Resolution), open only the specific config files involved; never descend into `src/`. Confirm runner presence by reading `package.json` scripts or equivalent manifest entries, not by executing the tool.

## Apply vs Propose policy

This step **proposes and applies** in the same pass, with strict scope and one change per commit.

- **Apply**: adding new config files, editing `package.json` (or equivalent) to wire scripts, installing dev dependencies for genuinely missing tools, updating `.gitignore` for tool caches.
- **Propose** (do not apply; record as TODO): any edit to source files beyond config, any rename of existing config files, any change to an existing working tool's config, any change visible to production (deploy scripts, published packages).
- **One commit per tool added** — "chore(lint): add ESLint warn-only baseline", "chore(format): add Prettier config and npm script". Never bundle lint + format + hooks into a single commit; it is impossible to revert cleanly.

## Zero-state safety — introducing a new tool to legacy code

The critical failure mode: adding a linter to a 500-file legacy codebase floods CI with thousands of errors on day one, the team reverts the commit, trust is lost. Prevent this **always**:

1. **Warn-only on first landing.** Configure violations as warnings, not errors. ESLint: rule severity `"warn"`. Prettier: install and wire the script but do **not** run `prettier --write` across the repo — only add an npm script for voluntary use.
2. **Changed-files-only in CI.** Wire the CI job to run against the PR diff (e.g. `eslint $(git diff --name-only origin/main...HEAD | grep -E '\\.(ts|tsx|js|jsx)$')`), not the whole repo. Legacy red stays out of the way while new code is clean.
3. **Ratchet plan in the summary.** State explicitly: "Phase 1: warn-only + changed-files CI. Phase 2 (human decides later): raise to error after backlog cleared, or enable across full repo."
4. **Coverage thresholds** follow the same principle — start at 0 or "record only"; never fail the build the first time thresholds are introduced.

## Autofix policy

| Action | Allowed? |
|---|---|
| `--fix` on files you are editing for another reason | Allowed (unrelated to this step). |
| `--fix` on files this step creates (e.g. a new config file) | Allowed on that file only. |
| `--fix` project-wide | Forbidden — record as follow-up TODO. |
| `prettier --write` project-wide | Forbidden — record as follow-up TODO. |
| `prettier --write` on a single new config file | Allowed. |

A separate human-approved commit may later run `--fix` and commit the diff; that is out of scope here.

## Conflict resolution — multiple configs for the same tool

When §3 records multiple config files for the same tool:

| Scenario | Resolution |
|---|---|
| `.eslintrc.js` + `eslint.config.js` (flat migration) | The newer `eslint.config.js` is source of truth **only if** wired into `package.json` scripts. Otherwise keep legacy. Record TODO to complete the migration as a separate task. |
| `.prettierrc` + `prettier.config.js` | Keep whichever the npm script references. Delete the other in a separate commit with a git blame check. |
| Multiple test configs (`jest.config.js` + `jest.config.ts`) | Same rule — whichever is wired. Flag the duplicate. |
| Conflicting rules between root and package config (monorepo) | Package-level overrides root. Record the override pattern; do not try to unify. |

**Do not delete a config file this step did not create.** Record duplicates as TODO for human review.

## Handoff to candidates

The workflow step declares two candidates. Delegate when the matching gap is identified:

| Candidate | Trigger |
|---|---|
| `setup.convention.apply` | Lint / format / commit hooks / EditorConfig need to be applied. Read its SKILL.md for mechanics (dep install, config file shape, script wiring). **This brownfield skill decides what to apply** (gap list); the candidate **executes the application**. |
| `dev.tdd` | Test runner is absent in §3 and §7. Do not scaffold a test runner here — hand off to the candidate, which knows how to introduce a runner with a first small red/green slice on foundation code. |

For each gap row, name which skill applies it (this one vs a candidate). Never duplicate logic.

## Monorepo handling

If discover §1a shows workspaces:

1. Lint / format / hooks go at the **root** if workspaces share language. One config; per-package overrides only if §3 explicitly records them.
2. Test runner may be per-package. Do not unify a multi-runner setup in this step.
3. CI lint job uses changed-files-only across the whole monorepo (simpler; zero-state rules still apply).

## Implementation Steps

1. **Audit** — read §3 and §7. Build an audit table with one row per layer (lint, format, hooks, test runner, coverage); columns: current state · gap · proposed action · handoff target.
2. **Classify actions** — for each gap: apply here, delegate to `setup.convention.apply`, or delegate to `dev.tdd`.
3. **Resolve conflicts** — apply Conflict Resolution to multi-config situations. Record TODO, do not delete.
4. **Apply the smallest set** — one commit per tool added, each in warn-only / non-blocking mode. Never bundle.
5. **Wire CI** — for each new tool that belongs in CI, add a changed-files-only job. Cross-check with `init.pipeline.brownfield` outputs if step 5 has run; otherwise leave a hand-off TODO.
6. **Record follow-ups** — backlog of "raise to error", "run autofix", "migrate legacy config".
7. **Validate** — run Acceptance Criteria.

## Output

Two artifacts:

1. **Commits** — one per tool added, each self-contained.
2. **Summary** — `.vibe/research/convention-alignment.md`, or amend `brownfield-init.md` under `## Convention alignment outcome` if the team prefers single-file evidence:

```markdown
## Convention alignment outcome

### Audit
| Layer | Before | Gap | Action | Applied by |
| --- | --- | --- | --- | --- |
| Lint | none | ESLint missing | add warn-only config, changed-files CI | setup.convention.apply |

### Ratchet plan (follow-ups)
- Raise ESLint severity to `error` after backlog clears (owner: <team>, target: <date>).
- Run `prettier --write` across repo after human review (owner: <team>).

### Deferred (out of scope)
- <duplicate configs flagged for human review>
```

## Forbidden

- Replacing an adopted tool with a personal preference.
- Introducing new frameworks ("adopt Biome because it's faster") unless the team explicitly asked.
- Running a project-wide reformat or autofix — formatting noise hides real diffs and blows up review.
- Landing a new tool at `error` severity on a repo with pre-existing violations.
- Bundling multiple tool additions into one commit.
- Silently deleting a config file this step did not create.
- Scaffolding a test runner inline — delegate to `dev.tdd`.

## Acceptance criteria

Confirm all of the below:

1. `.vibe/research/brownfield-init.md` §3 and §7 were the single source of truth (no re-audit).
2. Every proposed change has one row in the audit table with current state + gap + action + applied-by.
3. Every newly introduced tool is in warn-only / non-blocking mode, with CI scoped to changed files.
4. No commit bundles more than one tool.
5. Conflicts were recorded as TODO, not silently deleted.
6. Ratchet plan exists with named owners.
7. Handoff rows correctly name `setup.convention.apply` or `dev.tdd` where applicable.

## See also

- `setup.convention.apply` — hands-on application of lint / format / hooks. This skill decides **what**; that one executes **how**.
- `dev.tdd` — scaffolds a test runner with a first slice when one is absent.
- `init.brownfield.discover` — audit reads from §3 and §7.
- `init.pipeline.brownfield` — ensures CI enforces these conventions (same commands, same scope).
