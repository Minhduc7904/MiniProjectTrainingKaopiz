---
name: phase.verify.run-tests
description: Use when implementation is supposedly complete and you need to prove behavior matches the plan via test execution.
when_to_use: Activate at the end of the Execute phase or between major plan slices. Mandatory before calling any task "DONE". Skip only for doc-only updates.
license: MIT
---

# Verify — Run & add tests (Vibe #3)

**Skill id:** `phase.verify.run-tests`

Do not trust your own code. Trust the Tests.

## Required session artifact (workflow)

Hub step **`run_tests`** expects **`.vibe/sessions/<task-id>/VERIFY-TESTS.md`**. Before `approve`, write a **short** summary (no full CI logs, no secrets/PII):

- Commands run (e.g. `npm test`)
- Result: pass / fail
- If fail: failing suite name + one-line error (not dumps)

Hub verify gate also requires **`.vibe/sessions/<task-id>/VERIFY-EVIDENCE.json`** with machine-readable evidence records. Each record must include:

- `command` (non-empty string)
- `exit_code` (integer)
- `timestamp` (ISO-8601 string)
- `commit_sha` (40-char git SHA)
- `evidence_path` (repo-relative path to supporting artifact/log/summary)

Use `Not run` only with a documented waiver that includes reason, approver, and timestamp.

## Align with `PLAN.md` and profile

When the plan lists **Run:** / **Expected:** lines, use those commands as the primary acceptance checks; extend tests if the plan’s expectations are incomplete but the spec requires more coverage.

For **repo-wide** “what to run at the end of Execute,” treat **`devkit.workflow-profile.json`** **`verify_commands`** (and team CI) as the source of truth alongside **`PLAN.md`** — the Verify step should run the profile’s test/lint entrypoints when they match the task surface.

## Testing Strategy

1. **Vibe #3 - Test Like a User**: Prioritize integration tests that check input/output behavior rather than implementation-specific unit tests.
2. **Red to Green**: If tests fail, return to the **Execute** phase to fix them. Never add new features while tests are failing (Red).
3. **Handling Requirement Gaps**: If tests pass but the logic is incorrect (due to a misunderstanding of requirements), stop, update `PLAN.md`, and seek User approval.

## Tools

Use the project's test runner (Jest, Vitest, Cypress...). The Agent must know how to read error outputs to trace the root cause.

## Iron Law — No Fake Greens

Never skip a failing test or mock out a behavior just to see green. Every test failure is a requirement failure.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "The test is flaky, let's just ignore it for now." | Flaky tests hide real bugs. Fix the test or fix the flakiness. |
| "I manually checked it, it works." | Manual checks are not reproducible. Only automated tests count as proof. |

## See also

- `phase.verify.compliance` — check project standards after behavior is proven.
- `dev.tdd` — apply Red-Green-Refactor logic to fix test failures.

---
**Summary:** Verifying is not just running tests; it's proving the spec.
