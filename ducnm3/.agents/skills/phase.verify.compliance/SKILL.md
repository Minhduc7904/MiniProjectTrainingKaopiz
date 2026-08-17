---
name: phase.verify.compliance
description: Use after tests pass to verify linting, formatting, security hygiene, and architectural standards via verify_compliance.
when_to_use: Activate in the Verify phase, immediately after run-tests is Green. Mandatory before the Deliver phase for all tasks.
license: MIT
---

# Verify — Compliance

**Skill id:** `phase.verify.compliance`

Before "delivering," the Agent must ensure the code is not only correct but also "polished" according to general standards.

**Note:** **Layer 2** of `phase.execute.subagent-two-layer-review` (code quality, conventions) **overlaps** with compliance in spirit but **does not replace** `verify_compliance` / project automation — run this skill (or project scripts) for authoritative checks.

**Pipeline order (default composite):** Execute (per-slice **two-layer review** + optional chunk **tidy-refactor**) → Verify **suite** (`run_tests`) → (optional **post-green tidy**, same tidy skill, different step context — see [PLAN-VERIFY-DELIVER-SKILL-REFACTOR.md](../../../../../../docs/technical/PLAN-VERIFY-DELIVER-SKILL-REFACTOR.md) §4.1) → **compliance** → Deliver.

## Required session artifact (workflow)

Hub step **`compliance`** expects **`.vibe/sessions/<task-id>/VERIFY-COMPLIANCE.md`**. Before `approve`, record:

- Outcome of **`verify_compliance`** (pass / fail)
- Checklist or artifact id if the tool returns one
- Summarize violations (no raw secrets)

Verify remains blocked unless **`.vibe/sessions/<task-id>/VERIFY-EVIDENCE.json`** is valid. Add machine-readable records for compliance/test commands with:

- `command` (non-empty string)
- `exit_code` (integer)
- `timestamp` (ISO-8601 string)
- `commit_sha` (40-char git SHA)
- `evidence_path` (repo-relative path)

`Not run` is acceptable only with a clear waiver (reason, approver, timestamp) documented in artifacts.

## Workflow

1. **Call Compliance Tool**: Use MCP **`verify_compliance`**.
2. **Audit Checklist**:
    - Linting & Formatting.
    - File/Folder naming conventions.
    - Security (ensure no API Keys or Secrets are exposed).
3. **Address Violations**: If standards are not met, return to **Execute** to polish the code.

## Phase Conclusion

The verify phase is officially complete only when tests are green and compliance has passed. Proceed to **Deliver**.

## Iron Law — No bypass

Compliance failure = Task failure. Never merge or delivery code that has linting or security warnings unless a waiver is documented.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "Linting is just about commas and spaces." | Linting catches real bugs and enforces architectural boundaries. |
| "I'll fix compliance in a separate PR." | Deferred compliance is tech debt. Pay as you go. |

## See also

- `phase.verify.run-tests` — behavior proof must come before or with compliance.
- `phase.deliver.final` — handoff phase follows successful verification.

---
**Summary:** Be strict with yourself, and consistent with the team.
