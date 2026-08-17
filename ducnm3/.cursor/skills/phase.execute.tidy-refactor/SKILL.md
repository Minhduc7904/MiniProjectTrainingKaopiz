---
name: phase.execute.tidy-refactor
description: Use when a plan slice is functionally complete and tests are green, to perform naming or structural cleanup before moving to the next task.
when_to_use: Activate after a GREEN state in the TDD cycle, before starting the next RED step. Do not mix with new behavior implementation.
license: MIT
---

# Execute — Tidy refactor (Vibe #9)

**Skill id:** `phase.execute.tidy-refactor`

"Make it work, then make it right."

## Tidy Checklist

1.  **Naming**: Are variable and function names clear? Are there confusing abbreviations?
2.  **Dead Code**: Are there any leftover `console.log` or debug statements?
3.  **Structure**: Is a function too long (exceeding 20-30 lines)? If so, break it down.
4.  **Comments**: Remove comments explaining "what the code does" (the code should be self-explanatory); only keep "why it was done this way."

## Notes

Only refactor when the code is "Green" (working correctly). Do not attempt to refactor and add new logic at the same time.

After non-trivial tidy (moves, renames, signature changes), **re-run the same tests** (and quick lint if your PLAN or team policy says so) before the next chunk or commit.

## Iron Law — Green to Green only

Never start tidying while tests are RED. If tidy causes a RED state, revert immediately rather than trying to fix it while tidying.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'll just rename this while implementing the next part." | Mixing refactoring and features makes debugging regressions impossible. |
| "Tidying is just aesthetic." | Aesthetic code is maintainable code. Refactoring is a primary investment. |

## Same skill, two workflow steps

This skill is referenced from **Execute** (`refactor_chunk`, optional — tidy after a slice) and from **Verify** (`post_green_refactor`, optional — one readability pass after the suite is green, before `verify_compliance`). Same playbook, different **step context**; skip Verify’s pass if the tree is already clean. See [PLAN-VERIFY-DELIVER-SKILL-REFACTOR.md](../../../../../../docs/technical/PLAN-VERIFY-DELIVER-SKILL-REFACTOR.md) §4.1.

## See also

- `phase.verify.run-tests` — proof that tidy didn't break behavior.
- `phase.plan.write-plan` — turn large refactors into plan steps.

---
**Summary:** Leave the campground cleaner than you found it.
