---
name: phase.deliver.final
description: Use when implementation and verification are complete to finalize documentation, PRs, and task closure.
when_to_use: Activate at the end of the workflow only when all tests and compliance checks are GREEN. Skip for investigations or spikes that do not ship code.
license: MIT
---

# Deliver — Final handoff

**Skill id:** `phase.deliver.final`

Concluding a task professionally.

## Prerequisites

Do not start Deliver until **tests and compliance** are green per team policy (`phase.verify.run-tests`, `phase.verify.compliance`), unless the ticket is explicitly docs-only or the team documented a waiver in `PLAN.md`.

## Composite workflow vs real handoff

Some parent manifests (e.g. **`default`** / **`default.with-ingest`**) mark the **Deliver** composite step as **`optional: true`**. That flag only means the **workflow engine may skip rendering that step** in a session — it does **not** automatically waive **PR, ticket update, or team Definition of Done**. If your team still requires a handoff, document that in **`CONTRIBUTING`**, **`devkit.workflow-profile.json`**, or process docs. See [PLAN-VERIFY-DELIVER-SKILL-REFACTOR.md](../../../../../../docs/technical/PLAN-VERIFY-DELIVER-SKILL-REFACTOR.md) §1.3.

## Handoff Actions

1.  **Living documentation (`docs/specs/`)** — **required before handoff**
    - Update or add the domain file under **`docs/specs/`** (see **`docs/specs/README.md`**; use **`kaopiz-devkit spec touch <domain>`** to scaffold a new file) so agreed behavior is recorded for the next session and included in the session **`docs_specs_baseline_sha256`** tree.
    - If nothing under **`docs/specs/`** changed for this task, add a short **"Deferred / out of scope"** note in the nearest domain file or in `DELIVER.md` explaining why (spike, follow-up ticket, etc.).
    - Optional: add **ADDED / MODIFIED** bullets mirroring what shipped (no need for full OpenSpec delta syntax on day one).
2.  **Optional audit snapshot**
    - Run **`kaopiz-devkit archive`** to copy `PLAN.md`, `REVIEW.md`, `DELIVER.md` (when present) into `.vibe/archive/YYYY-MM-DD-<task>/` with a `manifest.json` audit trail — each file is taken from **`.vibe/sessions/<task>/`** first, then repo root if legacy.
3.  **Pull Request (PR) Preparation**:
    - Describe what changed.
    - Provide testing steps (Steps to test).
    - List any remaining risks.
4.  **Update Ticket/Issue**:
    - Mark the task as `Ready for QA` or `Done`.
    - Attach the link to the PR created.
5.  **Cleanup**:
    - Delete temporary files and local branches no longer in use.
    - Update project documentation if necessary.

## Rule
A product is only considered "DONE" when the recipient (User/Lead) can receive and run it without needing to ask the Agent questions.

## Iron Law — No guesswork delivery

The handoff must include testing steps and evidence. If a user can't verify the work in 60 seconds, the delivery is incomplete.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "The PR description is enough, no need for specs/docs." | PRs are ephemeral. Specs in **`docs/specs/`** are permanent system knowledge (versioned with the repo; baseline-hashed by the CLI). |
| "I'll update the docs later." | Later never comes. Documentation is part of the implementation, not a follow-up. |

## See also

- `phase.verify.run-tests` / `phase.verify.compliance` — verification must be GREEN before delivery.
- `phase.review.code-request` — optional **fresh-context** code review; use workflow **`qa.review`** when the team wants a dedicated pass (not part of the default Verify → Deliver chain in composite `default*`).

---
**Summary:** Finish strong. Leave the system better than you found it.
