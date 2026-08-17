---
name: phase.execute.fetch-knowhow
description: Use when implementing a plan slice that touches a protected domain (auth, finance, data-ingestion) to load domain-specific rules first.
when_to_use: Activate during the Execute phase, per domain-specific slice, before making any code changes to that domain. Ensure the plan is approved first.
license: MIT
---

# Execute — Fetch know-how

**Skill id:** `phase.execute.fetch-knowhow`

Before modifying any logic in a specific Domain (e.g., Auth, Payment, UI-Standard), the Agent must master the rules of that domain.

## Scope (align with HARD-GATE)

Use this skill **only in the Execute phase**, after **`PLAN.md` exists** and the workflow allows coding (user **`approve`** where the router has a Review gate). Do **not** treat `fetch_knowhow` as permission to change product code during **Ingest** or while the plan is still draft-only — see **`phase.ingest.read-sources`**.

## Required session artifact (workflow)

Hub step **`per_domain_knowhow`** expects **`.vibe/sessions/<task-id>/KNOWHOW.md`**. Before `approve`, append one short block per slice/domain:

- Domain / slice id
- Whether `fetch_knowhow` was called and key constraints (no secrets; summaries only)

## Workflow

1.  **Identify Domain**: Determine the relevant domain based on the files/modules being modified.
2.  **Call the Tool**: Use MCP **`fetch_knowhow(domain=...)`**.
3.  **Apply Checklist**: Compare the code being written against the constraints and checklists returned from the Hub.

## Choosing `domain` when the slice is API / UI / mobile

Hub know-how is keyed by **domain** strings (e.g. auth, payment). Map the **slice** to the closest domain:

| Slice touches | Typical `domain` values to try | If none match |
|---------------|-------------------------------|---------------|
| HTTP APIs, DTOs, handlers | Whatever your Hub exposes for API/security (often `auth`, payment-related domains) | Use **`PLAN.md`** + repo rules (`.claude/rules`, ADRs). Do **not** skip security-sensitive changes without written rules. |
| Web or RN **UI** (forms, a11y) | Domains for auth, PII, or UI standards if present | Same: `PLAN.md` + design system / lint rules; note gaps in Layer 2 review. |
| **iOS / Android / Flutter** native | Platform security, auth, storage domains if in Hub | Same fallback; prefer **`dev.stack.*`** for platform patterns after know-how. |

If **`fetch_knowhow`** returns nothing useful for this repo, still document in the slice outcome that rules were taken from **`PLAN.md`** / project docs — do not invent policy.

## Important Notes

- **Never Skip**: Even if the Agent is confident, they MUST call `fetch_knowhow` to check for any recent team updates.
- **Integrate into Code**: Add comments if specific logic is derived from a Know-how requirement.

## Iron Law — No Assumptions on Domain

If a domain has know-how rules, you MUST follow them. If you suspect a rule is outdated, stop and ask the user rather than violating it.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I've worked on this domain before, I know the rules." | Rules change. Team conventions evolve. Fetching takes 5 seconds. |
| "The prompt already has enough context." | fetch_knowhow provides project-specific constraints not found in generic LLM training. |

## See also

- `phase.execute.subagent-two-layer-review` — review that work complies with domain rules.
- `phase.verify.compliance` — project standards check.

---
**Summary:** Context is king. Domain rules are the law.
