---
name: init.documentation
description: Use when establishing the documentation structure (README, specs, ADRs) for a new project to ensure long-term maintainability.
when_to_use: Activate at the absolute start of a project. Mandatory first-step for any professional repository.
license: MIT
---

# Init — Documentation structure

**Skill id:** `init.documentation`

Documentation is a first-class citizen. If it's not documented, it's not finished.

## Iron Law — The "README" is the UI

The README must allow a new developer to go from `git clone` to `npm test` (Green) in under 5 minutes without asking questions.

## Mandatory Files

1.  **README.md**: Setup, Usage, Architecture overview.
2.  **CONTRIBUTING.md**: Guidelines for PRs and coding style.
3.  **docs/specs/**: Detailed domain-specific behavior records.
4.  **docs/adrs/**: Architecture Decision Records.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I don't have time to write docs while building features." | You don't have time to answer the same questions 10 times later. |
| "The project changes too fast for docs." | Fast-changing projects need docs the MOST to keep the team aligned. |

## See also

- `qa.docs.writing` — the ongoing maintenance of these documents.
- `phase.deliver.final` — the final check that docs are complete.

---
**Summary:** Documentation is an investment in your future self's sanity.
