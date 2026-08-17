# Example flow — quality reviewer dispatch

Walkthrough of SHAs → subagent → fix → re-dispatch. Keep the main `SKILL.md` for rules; use this for a concrete pattern.

```
[Task 3 complete — implementer reported DONE]

Get SHAs:
  BASE_SHA = a7981ec
  HEAD_SHA = 3df7661

[Dispatch quality-reviewer subagent]
  Task: Task 3 — Add verification function
  Range: a7981ec..3df7661
  Stack: TypeScript/Node.js, npm test

[Reviewer returns]:
  ISSUES FOUND
  Strengths: Clean architecture, real tests
  Important: Missing progress indicators on long loops
  Minor: Magic number (100) — should be named constant

[Fix Important issue: add progress indicators]

[Re-dispatch quality-reviewer]
[Reviewer returns]: APPROVED

→ Proceed to Task 4
```
