> Minimum structure for .vibe/research and per-task PLAN — spec/plan analysis log (on-disk artifacts; does not replace DevKit JSONL)

# Artifacts — spec & plan analysis log

`kaopiz-devkit` session JSONL **does not** contain the full plan file, research, or full MCP payloads. For audit/replay, the Agent writes **structured** content into the two artifacts below. **Do not** dump full tickets/secrets/PII — short excerpts or summaries; use **repo-relative** paths.

## `.vibe/research/<task-id>.md` (ingest / analysis)

**Canonical path only** for ingest + lock-scope: use this file. Do **not** use `.vibe/<task-id>/research/notes.md` (deprecated — use the path below so Hub skills and workspace rules match).

Create or update this file while gathering sources, locking scope, **before** treating PLAN as the source of truth (per phase skill / `devkit-research`). Minimum sections:

| Section | Content |
| ------- | ------- |
| **Sources** | What was read (ticket id, `docs/...`, MCP) — bullets, no long copy-paste |
| **Spec snapshot** | AC / expected behavior — **summary** by the Agent; does not replace the canonical spec file in the repo |
| **Confirmed vs Assumptions** | What is certain vs assumptions needing confirmation |
| **In scope / Out of scope** | Aligned with ingest skills; update when scope changes |
| **Ambiguities & questions** | Questions for user / PO before or while reviewing the plan |
| **Decision log (short)** | Decision → reason → (optional) impact on PLAN steps |

**Do not** create `PLAN.md` in a pure ingest step — per phase policy (see `devkit-research`).

## Plan file (`PLAN.md`) — plan / review

**Canonical path:** **`.vibe/sessions/<sanitized-task-id>/PLAN.md`** (matches Hub workflow `expected_artifacts` with `{{task_segment}}`). Legacy **repo-root `PLAN.md`** still works for verify/approve until you migrate.

Create or update when drafting an execution plan (`devkit-plan`, Vibe step 1, etc.). May start from Hub template **`PLAN.template.md`** (in kit: `setup/content/templates/`). Minimum sections:

| Section | Content |
| ------- | ------- |
| **Task / scope** | `task_id` or a scope description that identifies the work |
| **Requirements traceability** | Table or list: requirement / AC → section or step in PLAN |
| **Plan steps** | Micro-steps (prefer 2–5 min each); checkbox `- [ ]`; each group should cite **files** repo-relative; **no** placeholders like TBD / “add tests later” without naming files + assertions or commands |
| **Verification** (recommended for agents) | For important steps: **Run:** `command` and **Expected:** one-line result (e.g. FAIL → PASS after fix) |
| **Won't do / Later** | Out-of-scope items with reason or follow-up ticket |
| **validate_plan** | Result (ok / warnings) + **changes applied** after validate — short block, not full MCP response |
| **Risks & open questions** | Risks and unknowns before implementation |
| **Revision note** | When PLAN changes mid-flight: 1–2 lines what changed / why (helps Review and spec realignment) |

**HARD-GATE:** Do not change production source / migrations for this task until PLAN is approved per workflow (and Review step if any). Align with **`kaopiz-devkit approve`** after the plan step when using CLI gates.

## Quality principles

- Prefer bullets and small tables; avoid long chat copy-paste.
- Reference files with repo-relative paths.
- Business content lives in **artifacts**; orchestrator timeline lives in **JSONL** — do not mix roles.
