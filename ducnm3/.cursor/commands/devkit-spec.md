# devkit-spec — OpenSpec (SBU2 user runtime)

Use this command for **spec-driven** work when `devkit.config.json` has `"runtime_engine": "sbu2-ai-kit"`.

## Entry (recommended for new users)

1. Load and follow **`sbu2.openspec-router`** (Read `.cursor/skills/sbu2.openspec-router/SKILL.md` or vendor path in the wrapper).
2. For CLI operations (`list`, `new change`, `status`, `instructions`, `validate`), run:

```bash
npm run kaopiz-devkit -- openspec <subcommand> [args]
```

## Legacy `/opsx*` slash commands (still supported)

If you already use SBU2/OpenSpec slash names, **`devkit-sync`** copies `/opsx`, `/opsx-apply`, `/opsx-brainstorm`, etc. into `.cursor/commands/` when `runtime_engine=sbu2-ai-kit`. Each file is prefixed to route through the same vendored **`sbu2.*`** skills as native OpenSpec.

Prefer **`/devkit-spec`** or **`/kz-spec`** for discoverability; `/opsx` remains an alias, not removed.

## Full pipeline (parity with native OpenSpec)

When running inside a DevKit session (`start --skill sbu2_spec_driven`), workflow steps map to the same phases as native OpenSpec:

| Phase | Skill / CLI |
|-------|-------------|
| Route / classify | `sbu2.openspec-router` |
| Brainstorm / continue | `sbu2.openspec-brainstorm`, `sbu2.openspec-continue-change` |
| Verify spec gate | `sbu2.openspec-verify-spec` |
| Apply | `sbu2.openspec-apply-change` |
| Verify implementation | `sbu2.openspec-verify-change` |
| Archive | `sbu2.openspec-archive-change` |

Research and multi-agent review use `sbu2.openspec-research` and `sbu2.subagent-orchestration` when the router skill delegates to them.

## Active change metadata

Record the OpenSpec change name in session context as **`openspec_change_name`** (optional metadata). **`task_id`** remains the Jira/ticket id from `kaopiz-devkit start`.

## Runtime mismatch

If `runtime_engine` is `devkit`, this command is not the primary path — use `feature_with_superpowers` / `kz-superpowers` instead.
