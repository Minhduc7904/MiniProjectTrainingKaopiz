---
name: init.documentation.brownfield
description: Use to audit existing docs (README, CONTRIBUTING, specs, ADRs) in a brownfield repo and gap-fill only missing files — never rewrite docs that are still accurate. Measures readiness by onboarding-time proxies (command validity, step count ≤ 7, prereqs listed, verification step). Guarantees `CONTRIBUTING.md` exists with required sections. Enforces short, scannable, code-block-first writing. Trigger for "audit our README", "write CONTRIBUTING.md for this repo", "our README install commands are stale", "add missing onboarding docs without rewriting what we have".
when_to_use: Step 6 of setup.init.brownfield. Skip entirely if init.enabled_phases excludes documentation.
license: MIT
---

# Init Brownfield — Documentation (audit + gap-fill)

The README already exists. The question is whether a new engineer can go from `git clone` to first green test using only what's written. If yes → leave it alone. If no → fix the gap, don't rewrite.

## Iron Law — Onboarding time is the only metric

Every change in this step must shorten first-green-test time for a new engineer. Prose improvements that don't pass that test belong in a future docs task, not here.

## Inputs & Tool Discipline

Reads `.vibe/research/brownfield-init.md`:

- **§5 Docs** → README, CONTRIBUTING, docs tree, ADR folder presence.
- **§7 Gaps** → missing docs flagged by discover.

If either is missing, stop and request a re-run of discover. Do not re-scan the docs tree. When auditing a specific doc, open it once, scan the headings, check against the required sections below. Do not open every doc in `docs/` unless §5 flags a specific file. To verify command accuracy (e.g. README says `npm test` but repo uses `pnpm`), grep the manifest once — do not run the commands.

## Onboarding-time proxy

You cannot literally time a new hire. Use these proxies, in this order — each failed proxy is one row in the gap list:

1. **Runnable command check** — every `$ ...` command block in README must be valid for the repo's current tooling. If README says `npm install` but repo uses `pnpm-lock.yaml`, that is a gap. Cross-check against manifest.
2. **Setup step count** — README should get from clone to first passing test in **≤ 7 concrete steps**. A 15-step setup is a failure even if each step is correct.
3. **Required tooling list** — every tool referenced in setup commands must be listed in a "Prerequisites" section with the version tested (e.g. "Node 20+", "Docker 24+"). Missing prereqs → gap.
4. **Sample test run** — README must tell the reader how to verify setup worked (e.g. "run `pnpm test` — you should see N specs pass"). No verification step → gap.

## Writing style — mandatory for new content

LLM-generated docs tend to be long, vague, and full of filler. This step produces docs humans actually read:

- **Bullet-driven, not narrative.** Bullets, numbered lists, tables. Prose paragraphs belong in explainer docs, not operational setup.
- **Code blocks over prose for any command.** If the reader must run something, show a fenced block.
- **Short sections.** Each section scannable; if a section exceeds ~20 lines of prose, split it.
- **Concrete over generic.** "Run `pnpm test` — 42 specs should pass in < 30s on M-series Mac", not "run the test suite and verify results".
- **No emoji in setup docs** unless the existing file already uses them heavily and stripping would look odd.
- **Length ceilings**: README setup ≤ 50 lines · CONTRIBUTING.md ≤ 150 lines total · `docs/specs/*.md` split if > 300 lines.

**Forbidden vocabulary** (AI-writing tells that add zero information):
- `delve into`, `dive deep`, `deep dive`
- `leverage` (use "use")
- `paramount`, `crucial` (use "required" or delete)
- `seamlessly`, `effortlessly`, `robust`
- `comprehensive`, `holistic`, `best-in-class`
- `navigate` as a verb (use "go to" or "run")
- `it's worth noting that`, `it is important to note`
- `in this section, we will ...` (just do it)

If existing prose violates these rules but the content is accurate, **leave it alone** (Iron Law — don't restyle working docs). Apply writing-style rules only to **new** content this step writes.

## Apply vs Propose policy

- **Apply**: adding missing files (CONTRIBUTING, specs/README), adding missing README sections, patching wrong commands, adding the ADR link.
- **Propose** (TODO comment only): restructuring an existing working file, editing narrative prose, removing content, renaming files.
- **One commit per file touched.** "docs: add CONTRIBUTING.md", "docs: fix install commands in README", "docs: link ADR index from README". Never bundle.

## Handoff to candidate

| Candidate | Trigger |
|---|---|
| `qa.docs.writing` | Gap is narrative / tone / explanatory content (not missing files). Example: "README structure is fine but explanations are unclear" → delegate. This step handles **structural gaps**; the candidate handles **quality passes**. |

Decision rule: missing file / missing section / wrong command → fix here. "This paragraph is confusing" / "the overview reads badly" → out of scope; record as handoff.

## Required output — CONTRIBUTING.md

This is the workflow's `expected_artifact`. The step is **not complete** until a conforming `CONTRIBUTING.md` exists.

- **If absent**, create from the template below. Fill each section from real evidence (CI files for "what CI enforces"; hooks for "pre-commit commands"). Where evidence is missing, write `_TBD: confirm with team_` — do not fabricate.
- **If present**, audit against the template. For each missing or stale section, add or patch — do not restructure. If the file is > 150 lines and much of it is narrative filler, do **not** rewrite here — record a handoff to `qa.docs.writing`. The step still passes as long as the required sections (Prerequisites · Setup · Branch & commits · Before you open a PR · PR checklist) are present and accurate.
- **Never delete prose** from an existing CONTRIBUTING in this step.

```markdown
# Contributing

## Prerequisites

- <Runtime> <version range> (e.g. Node 20+)
- <Package manager> (e.g. pnpm 9+)
- <Optional> Docker 24+ (for integration tests)

## Setup

```bash
git clone <repo>
cd <repo>
<install command>
<env setup — e.g. cp .env.example .env, edit as needed>
<boot command if needed — e.g. docker compose up -d>
<verify command — e.g. pnpm test>
```

## Branch & commits

- Branch naming: `<pattern>` (from existing convention, e.g. `feat/<ticket>-<slug>`)
- Commit style: <Conventional Commits | team style | _TBD_>
- Protected branches: `main` requires passing CI + N reviews (enforced via GitHub branch protection — admin-configured)

## Before you open a PR

Run locally (same commands CI runs):

```bash
<lint command>
<format check command>
<test command>
```

## Pull request checklist

- [ ] CI is green
- [ ] Tests added or updated for the change
- [ ] Docs updated if behavior changed (README / ADRs / specs)
- [ ] No secrets or generated artifacts committed

## Architecture decisions

New architecture-level changes need an ADR under `docs/adrs/`. See `docs/adrs/0001-record-architecture-decisions.md`.

## Where things live

- `<src path>` — <what lives here>
- `<test path>` — <what lives here>
- `docs/adrs/` — architecture decision records
- `docs/specs/` — per-domain behavior specs
```

## README audit

Required sections, in this order (if existing README has a different order and it works, leave the order):

1. **Title + one-line description** — what this project is.
2. **Prerequisites** — versions of runtime + tooling + optional services.
3. **Setup / Quickstart** — ≤ 7 steps from clone to first passing test.
4. **Verification step** — "if you see X, setup worked".
5. **Common scripts** — table of `pnpm <script>` + what each does.
6. **Where to go next** — links to CONTRIBUTING, docs/adrs, docs/specs.

For each missing section, add. For each stale command (`npm` → `pnpm`), patch. Do not restructure. Do not rewrite working prose.

## ADR index linking

If `docs/adrs/0001-record-architecture-decisions.md` exists and README does not link to it:

- Add the link under README section "Where to go next".
- Format: `` - Architecture decisions: [`docs/adrs/`](./docs/adrs/) ``
- Do not copy ADR content into README.

If the ADR index does not exist, flag as an upstream issue — `init.architecture.brownfield` should have produced it. Do not create an empty index here.

## `docs/specs/` index

If §5 shows `docs/specs/` contains spec files but no `README.md` index, create `docs/specs/README.md` from this template:

```markdown
# Specifications

One file per non-obvious domain behavior. Read when you need the **why** a specific feature works the way it does.

## Index

- [<spec filename>](./<spec filename>.md) — <one-line purpose>
- ...

## How to add

- Filename: kebab-case domain description.
- Start with a "Problem" section and a "Behavior" section. Cite code.
- Keep each spec focused on one domain — split before a file exceeds ~300 lines.
```

If `docs/specs/` is empty or absent, do not create it — that is a proactive docs task out of scope.

## Stale content rules

| Situation | Action |
|---|---|
| Command reference is wrong (e.g. `npm` when repo uses `pnpm`) | Patch in place. Commit: "docs: fix install command in README". |
| Section describes an unimplemented feature | Do not delete. Mark with `> **TODO (stale):** feature removed — section retained for history.` |
| Section is accurate but poorly worded | Leave alone. Handoff to `qa.docs.writing`. |
| Duplicate info (e.g. setup in README and CONTRIBUTING diverge) | Pick one source of truth (CONTRIBUTING for contributor flow, README for first-time reader). Other copy shortened to a link. |
| Onboarding proxy check fails | Gap — fix per the rules above. |

## Implementation Steps

1. **Audit** — read §5 and §7. Apply onboarding-time proxy checks. Build a gap list.
2. **Classify** — each gap is (a) structural → fix here, or (b) quality/tone → handoff.
3. **Guarantee CONTRIBUTING.md** — create from template if absent, audit if present. Non-negotiable before step passes.
4. **Patch README** — missing sections added, stale commands corrected. No rewrite.
5. **Link ADR index** — from README under "Where to go next".
6. **Create docs/specs/README.md** — only if specs exist without an index.
7. **Record handoffs** for quality-tone gaps.
8. **Validate** — run Acceptance Criteria.

## Output

Two artifacts:

1. **Commits** — one per file touched.
2. **Summary** appended to `.vibe/research/brownfield-init.md` under `## Documentation outcome`:

```markdown
## Documentation outcome

### Audit
| File | State | Action |
| --- | --- | --- |
| README.md | setup steps use `npm`; repo uses `pnpm` | patched |
| CONTRIBUTING.md | missing | created from template |
| docs/adrs/ index | exists; not linked from README | link added |
| docs/specs/README.md | missing; 4 specs present | created |

### Onboarding proxy
- Setup step count: 5 ✓
- All commands valid against manifest: ✓ after patch
- Prereqs listed: ✓
- Verification step: ✓

### Handoff to qa.docs.writing
- README "Overview" paragraph is unclear but accurate — quality pass recommended.
- CONTRIBUTING long-form sections (>150 lines) carried over; prune/clarify recommended.

### Deferred / TODO
- `docs/specs/` has no specs; creation is a separate docs task.
```

## Forbidden

- "Modernizing" working docs (emoji, new heading style, reordering) — formatting-only diffs hide real content changes.
- Creating docs for unimplemented features.
- Deleting stale sections without explicit confirmation — mark with a `> TODO: stale` note instead.
- Writing with forbidden vocabulary (see Writing style).
- Letting CONTRIBUTING.md grow past ~150 lines.
- Bundling multiple file changes in one commit.
- Rewriting prose for style in this step — delegate to `qa.docs.writing`.

## Acceptance criteria

Confirm all of the below:

1. `CONTRIBUTING.md` exists at repo root with every required section (Prerequisites · Setup · Branch & commits · Before you open a PR · PR checklist · Architecture decisions · Where things live).
2. README contains all required sections, or existing equivalents, with setup step count ≤ 7.
3. Every command in README and CONTRIBUTING is valid against the current manifest (spot-checked via grep).
4. ADR index is linked from README (or flagged as upstream issue if index missing).
5. `docs/specs/README.md` exists if and only if `docs/specs/` has content.
6. No forbidden vocabulary appears in content this step wrote.
7. No rewrite of existing working prose — only additions, patches to wrong commands, and structural fill-ins.
8. Commits are one-file-each.
9. Handoffs to `qa.docs.writing` are recorded with specific file / line pointers.

## See also

- `init.documentation` — greenfield baseline (write docs from scratch).
- `qa.docs.writing` — ongoing docs quality / tone pass; receives handoffs from this step.
- `init.brownfield.discover` — reads §5 and §7 as input.
- `init.architecture.brownfield` — produces the ADR index this step links.
- `init.pipeline.brownfield` — supplies the required-check list and secret list documented in CONTRIBUTING.
