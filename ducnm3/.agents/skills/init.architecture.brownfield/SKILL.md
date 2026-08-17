---
name: init.architecture.brownfield
description: Use to backfill Architecture Decision Records (ADRs) for decisions already baked into an existing codebase — surface the "why" implicit in the code, do not redesign. Produces `docs/adrs/0001-record-architecture-decisions.md` plus one numbered, cross-linked ADR per structural decision (data store, API style, auth, deployment target, etc.). Trigger whenever the user says "retrofit ADRs", "document architecture for this existing repo", "we have no ADRs yet", or when step 2 of setup.init.brownfield runs.
when_to_use: Step 2 of setup.init.brownfield, after discover. Activate when the repo has real code but no (or stale) ADR trail. Skip entirely if init.enabled_phases excludes architecture.
license: MIT
---

# Init Brownfield — Architecture (retrofit ADRs)

The architecture already exists in code. Your job is to **surface** it — name the decision, explain the constraint, record the alternative that was rejected (even if only implicitly).

## Iron Law — Describe, don't redesign

Never rewrite architecture in this step. If the code uses Postgres, the ADR reads "we use Postgres because …" — not "we should consider Mongo". Alternatives go in `Rejected alternatives` only when there is real evidence the team considered them.

## Inputs & Tool Discipline

Reads `.vibe/research/brownfield-init.md` — specifically:

- **§1 Stack & frameworks** → runtime + framework + key library decisions.
- **§2 Runtime & dependencies** → data store, external service decisions.
- **§6 Conventions in use** → layout decisions (monolith vs split, module boundaries).
- **§7 Gaps** → decisions that are notably **open** (e.g. no test strategy chosen).
- **§8 Uncertainty / conflicts** → decisions you must resolve before writing an ADR (see Conflict Resolution).

If any of these sections is missing, stop and request a re-run of discover. Trust discover's findings — do not re-scan the repo. When an ADR requires one specific code citation, open the file and the line range; do not read the whole module. Never list every library as a decision — one ADR per structural choice, not per import.

## What counts as a "decision" worth an ADR

Use this test. A decision gets an ADR **only if all three are true**:

1. **Structural** — changing it forces multiple files to change (data store swap, API style swap, auth provider swap). Library upgrades and config tweaks do not qualify.
2. **Evidenced in code** — concrete proof in the repo (import, connection string, framework pattern). Absence-of-evidence goes in `status: open`, not a fabricated ADR.
3. **Explainable why** — at least one reason this was chosen over an obvious alternative. If you cannot name the alternative, either the decision is trivial (skip) or open (`status: open`).

**Expected canonical set** for most backend services (a checklist, not a quota):

| Decision | Evidence typically in |
|---|---|
| Language runtime + version | manifest (`package.json`, `go.mod`, …) |
| Primary framework | manifest + `src/` entry file |
| Data store | compose / env vars / ORM imports |
| API style (REST / GraphQL / tRPC / RPC) | route/schema files |
| Authentication strategy | middleware + token libs |
| Deployment target (container / serverless / VM) | Dockerfile, deploy scripts, CI |
| Monolith vs split (services, packages) | workspace config, folder layout |
| Testing strategy | test runner + sample specs (or §7 gap) |

Aim for **5–10 ADRs** in a typical single-service repo; a monorepo may justify more, but still one ADR per decision per service, not duplicated.

## Specialized candidates

The workflow step (`brownfield_architecture` in `setup.init.brownfield.json`) declares specialized candidates for deep-dive decisions. If discover §1 or §2 names a matching stack, read the candidate's SKILL.md **before** drafting the ADR, then cite the candidate's guidance in the ADR's `Consequences` section.

| Candidate | Trigger |
|---|---|
| `init.database` | Data store decision needs schema / migrations / ORM / indexing detail. |
| `init.api.rest` | REST endpoints detected — routing / versioning ADR depth. |
| `init.api.graphql` | GraphQL schema detected — schema / resolver ADR depth. |
| `init.api.trpc` | tRPC router detected — apply before the API-style ADR. |

If multiple API styles coexist (e.g. REST + GraphQL), write one ADR per style.

## ADR set — required output

Produce files under `docs/adrs/`. The index is required; one decision file per decision.

### Index file — `docs/adrs/0001-record-architecture-decisions.md`

Always create this file (workflow's `expected_artifact`). Exact template:

```markdown
# Architecture Decision Records

This repository uses ADRs to record non-obvious architecture decisions. Each file below is a standalone decision with its context, the choice made, and the trade-offs accepted.

## How to read

- Read in numeric order on first onboarding.
- Each ADR has a `status` field: `accepted` (in force), `open` (no decision yet — flagged for follow-up), `deprecated` (no longer in force), `superseded by NNNN` (replaced by a later ADR).
- If you are about to change architecture, read the matching ADR first and propose a new ADR that supersedes it — do not edit the accepted record silently.

## Index

| # | Title | Status |
| --- | --- | --- |
| 0001 | Record architecture decisions | accepted |
| 0002 | <decision title> | <status> |
| ... | ... | ... |

## Template

See `0002-` onward for real entries; new ADRs copy the structure of any accepted entry.
```

### Decision file — `docs/adrs/NNNN-<kebab-title>.md`

Numbering: strictly sequential from `0002`. Order decisions in the order discover §1 → §2 → §6. Once assigned, numbers are permanent — never renumber later.

Filename: `NNNN-<short-kebab-title>.md` (e.g. `0003-use-postgres-as-primary-datastore.md`).

```markdown
# NNNN. <Decision title in plain English>

- Status: <accepted | open | deprecated | superseded by NNNN>
- Date: <ISO date of the ADR write, not of the original decision>
- Deciders: <team/role if known, otherwise "Unknown (retrofitted)">

## Context

<Why this decision had to be made. Constraints at the time: team skill, budget, deadline, ops capability. If unknown, write "Unknown (retrofitted) — inferring from code signals.">

## Decision

<One sentence. "We use X.">

## Rationale

<2–4 bullets. Concrete reasons tied to evidence in code or known constraints. No marketing language ("best-in-class", "industry-leading" are forbidden).>

## Rejected alternatives

<Only list alternatives with real evidence of consideration — abandoned code, comments, chat references, or a clearly constrained choice (e.g. hosted service unavailable in region). If you have no such evidence, write "Unknown (retrofitted)." Do not invent alternatives.>

## Consequences

- Positive: <what this choice enables>
- Negative: <what this choice costs — latency, lock-in, operational burden>
- Follow-up: <open questions, migration triggers, related ADRs>

## Evidence

- <file path>:<line range> — <what this line shows>
- <file path> — <what this file is>
```

**Evidence citations:** at least **one**, at most **three** per ADR. Format: `<relative path>:<line>` when a specific line matters (e.g. `src/db/pool.ts:12`); path-only when the whole file is the evidence (e.g. `docker-compose.yml`). Commit hashes are not required.

## Conflict Resolution (discover §8)

When §8 records conflicting signals for a decision:

1. **Pick the primary** — which code path runs in production, which receives recent commits, which is referenced from CI.
2. **Write one ADR for the primary** with status `accepted`.
3. **Write a second ADR for the secondary** with status `deprecated` or `superseded by <primary ADR number>`, citing the legacy/migration surface. Do not delete evidence of the secondary.
4. **If you cannot tell which is primary**, write a single ADR with status `open` describing the conflict, and list it prominently in the index.

Example: `pg` used in `src/db/`, `mongoose` used only in `scripts/legacy-import.ts` → `0003-use-postgres-as-primary-datastore.md` (accepted) + `0004-mongodb-legacy-import.md` (deprecated, references 0003).

## Status values

| Status | When |
|---|---|
| `accepted` | Decision is in force in current code. Default for retrofitted ADRs. |
| `open` | Evidence absent or contradictory; no primary can be chosen. Typically §7 gaps (e.g. no test strategy). |
| `deprecated` | Code evidence still exists but team has explicitly moved on (legacy import scripts, feature-flag-gated paths). Cite the replacement. |
| `superseded by NNNN` | Later ADR replaces this one. Write `superseded by 0012` — numeric reference. |

## Implementation Steps

1. **Load evidence** — read discover §1, §2, §6, §7, §8. If any missing, stop and request re-run.
2. **Classify decisions** — apply the three-part test (structural / evidenced / explainable). Produce a working list; do not write files yet.
3. **Resolve conflicts** — for each §8 entry, decide primary/secondary/open per Conflict Resolution. Update the working list.
4. **Delegate deep-dive** — for decisions matching a candidate (DB / REST / GraphQL / tRPC), read the candidate's SKILL.md.
5. **Scaffold the index** — create `docs/adrs/0001-record-architecture-decisions.md` from the template with one placeholder row per decision in the working list.
6. **Write ADRs** — one file per decision, numbered sequentially from `0002`. Fill the template in full; no TBD sections. `Rejected alternatives` must be either evidenced or `Unknown (retrofitted).`.
7. **Update the index** — fill in title and status for every ADR.
8. **Validate** — run Acceptance Criteria.

## Rerun behavior

If `docs/adrs/` already contains files:

1. **Read the existing index** first. List each present ADR's number + title + status.
2. **Do not renumber existing ADRs.** Continue from the next unused number.
3. **Compare against the working list**:
   - Decision already has an `accepted` ADR and current code still matches → leave alone; mention as "verified" in the output summary.
   - Decision has an ADR but code evidence has changed (e.g. ADR says Mongo but code now uses Postgres) → write a new ADR that supersedes the old; mark old as `superseded by <new number>`.
   - Decision has no ADR → write one.
4. **Never silently edit an accepted ADR.** Edit is only for status field updates (accepted → superseded) with an explicit pointer to the replacement.

## Forbidden

- Recommending architecture changes in this step. If the code is wrong, that is a different workflow.
- Filling `Rejected alternatives` with generic alternatives the team never considered ("we could have used SQLite / DynamoDB / …"). Only list what has evidence.
- Skipping decisions because they feel obvious. Obvious-to-current-team is exactly what onboarding engineers miss.
- Using more than three evidence citations per ADR. If you need more, the decision is too broad — split it.
- Editorializing prose ("clean", "elegant", "modern", "industry-standard"). ADRs explain trade-offs, not praise choices.

## Acceptance criteria

Confirm all of the below:

1. `docs/adrs/0001-record-architecture-decisions.md` exists with the template structure.
2. Every decision from the working list has a corresponding `docs/adrs/NNNN-*.md` file.
3. Every ADR has: status · date · context · decision · rationale · rejected alternatives · consequences · evidence (≥1 citation, ≤3).
4. Every §8 conflict was resolved into a primary + secondary/open ADR pair (or a single open ADR).
5. Numbers are strictly sequential from 0001; no gaps, no duplicates.
6. The index table lists every ADR with current status.
7. Specialized candidates were read for matching decisions (DB / REST / GraphQL / tRPC) — note which ones in the output summary.

## See also

- `init.architecture` — greenfield variant; choose instead of retrofit when the repo is empty.
- `init.brownfield.discover` — evidence source this step reads from (§1, §2, §6, §7, §8).
- `init.database` / `init.api.rest` / `init.api.graphql` / `init.api.trpc` — deep-dive candidates when a matching stack is detected.
- `init.documentation.brownfield` — step 6 links the ADR index from README; this skill's index file is the anchor.
