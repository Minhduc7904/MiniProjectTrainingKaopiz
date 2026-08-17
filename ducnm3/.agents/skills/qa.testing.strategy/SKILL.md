---
name: qa.testing.strategy
description: Use when planning the "safety net" for a project, defining which test layers (unit, integration, e2e) are required.
when_to_use: Activate during the Planning phase (write-plan) or when designing quality gates for a new project. Skip for minor documentation-only tasks.
license: MIT
---

# QA — Testing strategy

**Skill id:** `qa.testing.strategy`

QA is not just writing tests; it's creating a "safety net" that allows for rapid, confident change.

## Strategy Layers

1.  **Happy Path**: Verify the most common successful flow (Integration priority).
2.  **Defensive (Edge Cases)**: Test nulls, empty strings, and malformed inputs.
3.  **Negative (Error Handling)**: Ensure the system fails gracefully with correct error codes.
4.  **Integrity (DB/API)**: Verify that side effects (DB writes, external calls) actually happen.

## Iron Law — Test the Invisible

Never assume a side effect occurred just because a function returned `true`. You MUST verify the persistent state (e.g., query the DB, check the mock call count).

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'm testing the logic, no need to check the DB." | The logic might be right, but the integration (the most common source of bugs) remains unproven. |
| "Integration tests are too slow for TDD." | Slow tests signal bad architecture. Modularize the system so slices can be tested independently. |

## See also

- `dev.tdd` — the execution engine for these strategies.
- `phase.verify.run-tests` — the verification phase where the strategy is proven.

---
**Summary:** A good test strategy ensures you can sleep soundly after a Friday deploy.
