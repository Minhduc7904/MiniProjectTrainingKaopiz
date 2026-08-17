---
name: dev.refactor.identify
description: Use when identify technical debt or "smelly" code that needs cleanup without changing external behavior.
when_to_use: Activate after a task is functionally complete (GREEN) or when the user explicitly requests a cleanup pass. Skip if the code is already at project standards.
license: MIT
---

# Refactor — Identify & scope

**Skill id:** `dev.refactor.identify`

Refactoring is a "cleaning" action to improve readability and maintainability.

## Iron Law — No Feature Creep

Never add new features or fix unrelated bugs while refactoring. If you find a bug, document it and fix it in a separate TDD cycle.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "While I'm changing this name, I'll just add this one parameter..." | This is feature creep. It breaks the "Lock Behavior" safety net. |
| "Refactoring is a waste of time for this small project." | There is no such thing as a "small project" that stays small. Debt compounds. |

## See also

- `phase.execute.tidy-refactor` — the execution phase of these identified refactors.
- `dev.tdd` — requirement for any refactoring pass (must start from GREEN).

---
**Summary:** Good code is code that looks obvious.
