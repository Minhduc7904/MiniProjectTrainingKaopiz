---
name: setup.project-init
description: Use when bootstrapping a new repository or module to ensure a solid, standardized foundation.
when_to_use: Activate at the absolute start of a new project before any feature code is written. Mandatory for all new service creation.
license: MIT
---

# Setup — Project initialization

**Skill id:** `setup.project-init`

Starting a project correctly is the best way to prevent future technical debt.

## Bootstrap Checklist

1.  **Boilerplate**: Use the official project templates (e.g., `npx create-next-app`).
2.  **Isolation**: Setup `.gitignore`, `.env.example`, and `.dockerignore`.
3.  **Documentation**: Initialize `README.md` and `docs/specs/README.md`.
4.  **Dev-Experience (DX)**: Configure `task` runner or `scripts` for common operations.

## Iron Law — No "Empty" Projects

A project is not initialized until it has a working "Hello World" that passes a basic test and a build command.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'll setup the tests after I have some features." | Without tests from Day 1, you'll never have the confidence to refactor late-stage features. |
| "A README isn't needed for a private project." | Today's private project is tomorrow's legacy nightmare. Document the setup NOW. |

## See also

- `setup.convention.apply` — apply the team's styling rules immediately after init.
- `init.architecture` — the high-level design that precedes project initialization.

---
**Summary:** Solid foundations allow for skyscraper implementations.
