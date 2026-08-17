---
name: phase.ingest.read-sources
description: Use when starting a new task or requirement before drafting an execution plan or touching code — gather sources, record a canonical research file, and branch to brainstorm/clarify when the spec is missing or MCP fails.
when_to_use: Ingest phase (including spec-clarity clarify steps). Skip filling a new file only when `.vibe/research/<task-id>.md` already reflects current sources and spec snapshot for this task.
license: MIT
---

# Ingest — Read requirement sources

**Skill id:** `phase.ingest.read-sources`

Focus on **raw data gathering** and **one canonical research artifact**. Do **not** write `PLAN.md` here.

## Canonical research file (required)

- **Path:** **`.vibe/research/<task-id>.md`**
- **`<task-id>`:** From the **active session** state (`task_id` in **`.vibe/sessions/<task>/state.json`**, or legacy **`.vibe/state.json`** until migrated) when present; normalize for filenames: replace characters outside `[a-zA-Z0-9._-]` with `_` (same idea as `phase.spec-clarity` verify scripts).
- **Do not** use `.vibe/<task-id>/research/notes.md` — that layout is **deprecated** in favor of the path above (aligns with workspace **artifact-spec-plan-log**).

Create `.vibe/research/` if needed. This file is the **single handoff** into lock-scope, Plan, and audit.

## Layer model (do in order)

| Layer | What | Output in `.vibe/research/<task-id>.md` |
| ----- | ---- | --------------------------------------- |
| **L0** | Source availability | Short **Sources** + status lines (MCP ok / failed / no task_id) |
| **L1a** | Formal ingest | Ticket/docs/repo → AC, constraints, affected areas |
| **L1b** | Informal spec | Clarify or **`phase.spec.brainstorm`** until **Spec snapshot** is usable |
| **L2** | Merge spec | **Spec snapshot**, **Confirmed vs Assumptions**, **Ambiguities** (closed or explicit) |

## L0 — Sources & MCP

1. If **`task_id`** exists: call MCP **`get_task_context`** when available.
2. If MCP **fails** or returns nothing useful: record under **Sources** exactly that (e.g. “get_task_context: failed / empty”) — **do not invent** ticket text.
3. If there is **no** `task_id`: record that the driver is **user chat / pasted docs** and list what was provided.
4. Still **read the repo**: `README.md`, `specs/`, `design/`, code paths relevant to the request.

## When to use `phase.spec.brainstorm` (L1b) — not lock-scope yet

| Situation | Action |
| --------- | ------ |
| MCP failed or ticket empty, but user described intent | L1b: clarify; use **brainstorm** if exploration / trade-offs are needed before AC exist |
| Requirements vague (“make it faster”, “improve UX”) | L1b: brainstorm or structured clarify until **Spec snapshot** is concrete |
| Conflicting sources (ticket vs code) | Document conflict in **Ambiguities**; resolve with user before L3 lock-scope |
| Formal AC already in repo/ticket | L1a only; merge into **Spec snapshot** |

**Do not** draft **`PLAN.md`** or production code because ingest is blocked — stay in L1/L2 until the spec is writable.

## Minimum sections (match artifact-spec-plan-log)

Update **`.vibe/research/<task-id>.md`** with at least:

| Section | Content |
| ------- | ------- |
| **Sources** | Ticket id, MCP result, `docs/...`, paths read — bullets, no secrets/PII dumps |
| **Spec snapshot** | Expected behavior / AC summary (Agent-written; not a substitute for canonical repo spec) |
| **Confirmed vs Assumptions** | What is verified vs guessed |
| **In scope / Out of scope** | May be preliminary before **`phase.ingest.lock-scope`** |
| **Ambiguities & questions** | For PO/user; empty when none |
| **Decision log (short)** | Optional during ingest |

## Optional domain depth (agent-chosen)

If the task touches these areas, open the matching file under this skill — **do not** paste full checklists into the research file; answer briefly and link paths:

| Topic | Reference |
| ----- | --------- |
| NFR (a11y, i18n, perf, logging) | [`references/nfr-checklist.md`](references/nfr-checklist.md) |
| API / versioning / breaking changes | [`references/api-contracts.md`](references/api-contracts.md) |
| Security / privacy / PII | [`references/security-privacy.md`](references/security-privacy.md) |

## Ingest vs brainstorming

- **`read-sources` (this skill):** Ticket/doc/chat-driven ingest into **`.vibe/research/<task-id>.md`** — still **no** `PLAN.md`.
- **`phase.spec.brainstorm`:** Structured **idea → design** when AC are missing; output must be **merged** into the same research file (see that skill).

## Iron Law — No production implementation (ingest phase)

Until an **approved** Plan (and Execute where applicable), do **not**:

- Edit **application/runtime source** (e.g. `src/`, `app/`), **new behavior tests**, or **migrations** for this task.
- Change **deploy/runtime config** to “try” the feature.

**Allowed:** Read-only exploration; updates to **`.vibe/research/<task-id>.md`**; scratch notes under `.vibe/` that are **not** imported by the app.

## See also

- `phase.ingest.lock-scope` — L3 workscope after L2 spec is coherent.
- `phase.spec.brainstorm` — when formal requirements are missing.
- `phase.plan.write-plan` — **after** ingest + scope gate per workflow.

---
**Summary:** One canonical research file; L0→L2 before lock-scope; brainstorm when the spec does not exist yet.
