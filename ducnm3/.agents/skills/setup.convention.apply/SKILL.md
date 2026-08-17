---
name: setup.convention.apply
description: Use when establishing linting, formatting, and naming rules to ensure the entire codebase looks uniform.
when_to_use: Activate during project initialization or when performing a system-wide refactor to improve consistency.
license: MIT
---

# Setup — Convention application

**Skill id:** `setup.convention.apply`

A codebase must look like it was written by a single person, regardless of team size.

## Iron Law — Automate, Don't Negotiate

Conventions that aren't enforced by tools (Linter, Prettier, Git Hooks) do not exist. Never rely on manual documentation for formatting.

## Implementation Steps

1.  **Tooling**: Setup `.eslintrc`, `.prettierrc`, and `.editorconfig`.
2.  **Enforcement**: Use `husky` or `pre-commit` to prevent non-compliant code from entering the repo.
3.  **Governance**: Define naming conventions (e.g., `feature/` for branches, `use*` for hooks).

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "Formatting is a matter of personal taste." | Consistency is more important than anyone's individual taste. Pick a standard and stick to it. |
| "We'll fix the lint errors later when we have time." | Lint errors are technical debt. They hide real logic bugs. Fix them at the gate. |

## See also

- `phase.verify.compliance` — the ongoing verification of these conventions.
- `setup.project-init` — conventions should be established at the foundation.

---
**Summary:** Conventions minimize cognitive load during code reviews.
