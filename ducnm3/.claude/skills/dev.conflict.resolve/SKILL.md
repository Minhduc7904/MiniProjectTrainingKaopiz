---
name: dev.conflict.resolve
description: Use when facing merge conflicts or logical inconsistencies between code branches.
when_to_use: Activate immediately upon encountering a merge conflict or conflicting logic. Mandatory for all multi-developer integration tasks.
license: MIT
---

# Conflict — Resolution strategy

**Skill id:** `dev.conflict.resolve`

When facing conflicts, stay calm and logical. Never "accept both" blindly.

## Iron Law — No Mystery Resolves

You MUST be able to explain why you chose one version over another or how you merged them. If you don't understand the logic on both sides, you cannot resolve the conflict.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'll just accept 'theirs' since they are the lead." | Leading developers make mistakes too. Logical consistency is more important than seniority. |
| "It's just a small conflict, I'll fix the build error after." | Conflicts often mask deeper logical incompatibilities. Fix the logic, not just the syntax. |

## See also

- `phase.verify.run-tests` — proof that the resolution didn't break functionality.
- `dev.debug.systematic` — use if the resolution leads to weird new bugs.

---
**Summary:** Resolving conflicts is the art of combining ideas without losing intent.
