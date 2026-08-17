---
name: dev.tdd
description: Use when implementing any feature, bugfix, or behavior change to enforce the Red-Green-Refactor cycle.
when_to_use: Always active during the implementation phase. Mandatory for every new function or logical change. Skip only for non-functional config updates or documentation.
license: MIT
---

# Test-Driven Development (TDD)

Write the test first. Watch it fail. Write minimal code to pass.

## Why this skill is `dev.tdd` — not “test only”

The word **Test** names the *entry*: an **executable specification** for the next bit of behavior. Most of the cycle is **development work on production code**:

| Phase | What you write | Role |
| ----- | -------------- | ---- |
| **RED** | Usually a small automated test (or other failing check defined in `PLAN.md`) | Locks *what* “done” means before you invent implementation details. |
| **GREEN** | **Application / library code** — minimal change to satisfy the spec | This *is* normal dev: APIs, UI wiring, services, migrations, etc. |
| **REFACTOR** | **Production code** cleanup (names, structure) with tests staying green | Pure engineering; no new behavior. |

So TDD is **development discipline**: tests drive the sequence; **the deliverable is still features and fixes in real code**. If a step only talked about tests and never touched app code, that would be QA-only — that is not this skill.

## Stack-specific skills in this Hub (use during GREEN / REFACTOR)

**`dev.tdd`** stays **stack-agnostic** (order: spec → fail → code → refactor). **Stack skills** under `kit/setup/content/skills/common/dev.stack.*` describe **how** to write production code: layout, APIs, navigation, performance — load the right one **when you enter GREEN or REFACTOR**, not instead of RED.

| Skill id | When to open it |
|----------|-----------------|
| [`dev.stack.flutter`](../dev.stack.flutter/SKILL.md) | Flutter / Dart — widgets, Riverpod/Bloc, GoRouter; `references/testing.md`. |
| [`dev.stack.react-native`](../dev.stack.react-native/SKILL.md) | RN / Expo — Jest, RTL, Maestro in references. |
| [`dev.stack.ios`](../dev.stack.ios/SKILL.md) | UIKit / SwiftUI, HIG, accessibility. |
| [`dev.stack.android`](../dev.stack.android/SKILL.md) | Kotlin / Compose, Material 3, Gradle. |
| [`dev.stack.fullstack`](../dev.stack.fullstack/SKILL.md) | Service layers, API ↔ client, auth, testing pyramid. |
| [`dev.stack.frontend-web`](../dev.stack.frontend-web/SKILL.md) | Rich web UI, motion, media — **PLAN** often must spell non-unit checks (build, E2E, visual). |

Repo root: `packages/devkit-hub/kit/setup/content/skills/common/dev.stack.<platform>/SKILL.md`. Naming convention: **`dev.stack.*`** (folder name = YAML `name` = skill id).

**Contract:** stack skill = *where files go and which patterns to use*; **this skill** = *evidence-first sequence* (`PLAN.md` / `write-plan` overrides for doc-only slices).

## Required session artifact (workflow)

Hub step **`implement_commits`** expects **`.vibe/sessions/<task-id>/IMPLEMENT.md`**. Before `approve`, append a brief entry per slice:

- PLAN checkboxes addressed
- Commits (subjects) or PR ref
- Repo paths touched (high level)

**What “dev” means in GREEN / REFACTOR** (concrete steps, examples by layer): [`references/tdd-detail.md`](references/tdd-detail.md) — sections *GREEN — what you build* and *REFACTOR — safe changes*.

**Core principle:** If you didn't watch the test fail, you don't know if it tests the right thing.

> **IRON LAW: NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST.**
>
> Wrote code before the test? Delete it. Start over. No exceptions.

### When `PLAN.md` overrides the default (non-TDD slice)

`phase.plan.write-plan` allows slices that are **docs-only**, **config/infra**, or **manual / E2E** verification. For those slices, Execute follows **`PLAN.md` Run / Expected**, not the unit-test IRON LAW above. For **all code-changing behavior**, default TDD still applies unless the PLAN explicitly names the alternative signal.

### Surface → typical “RED” signal (hints only)

| Surface | Typical automated RED | If PLAN uses something else |
|--------|------------------------|-----------------------------|
| Backend service | Unit / integration test failing for the right reason | Contract test, schema check |
| Web UI | Component / RTL / a11y assertion | Storybook interaction, E2E step |
| Mobile | Widget test, JVM unit test, snapshot (use sparingly) | Integration / device — may be slower; PLAN should slice |
| Full stack | API test + client test | E2E; often one RED at boundary per slice |

Source of truth remains **`PLAN.md`** and **`devkit.workflow-profile.json`** (`verify_commands`).

## The RED-GREEN-REFACTOR Cycle

```
RED      → Write one failing test for the next behavior
VERIFY   → Run test, confirm it fails for the RIGHT reason
GREEN    → Write minimal code to make it pass (no extras)
VERIFY   → Run test, confirm it passes + no other tests broken
REFACTOR → Clean up (names, duplication) — keep tests green
REPEAT   → Next behavior
```

**Examples, VERIFY commands, rationalizations, full checklist, and "when stuck":** [`references/tdd-detail.md`](references/tdd-detail.md).

## See also

- `phase.plan.write-plan` — when a slice is **not** classic TDD, it must be written in `PLAN.md` (doc-only / config / manual QA / E2E-only).
- [`references/tdd-detail.md`](references/tdd-detail.md) — **GREEN / REFACTOR** + **linked stack skills table** (same links as above, from `references/`).
- **Stack guides:** `dev.stack.flutter` · `dev.stack.react-native` · `dev.stack.ios` · `dev.stack.android` · `dev.stack.fullstack` · `dev.stack.frontend-web` (see table in this file).
- `dev.debug.systematic` — when RED/GREEN behavior is unclear before you add more code.
- `phase.execute.tidy-refactor` — optional tidy between chunks (same “green-only” rule as REFACTOR).
- `phase.verify.run-tests` — full suite in the workflow phase.
- **Init vs stack:** `init.*` = bootstrap / design-time; **`dev.stack.*`** = Execute-time patterns — [PLAN-EXECUTE-SKILL-REFACTOR.md](../../../../../../docs/technical/PLAN-EXECUTE-SKILL-REFACTOR.md) §7.
