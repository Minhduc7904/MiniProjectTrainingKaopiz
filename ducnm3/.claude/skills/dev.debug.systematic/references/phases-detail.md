# Systematic debugging — phases 2–4 and reference tables

Full detail for **Phase 2–4**, red flags, and quick reference. **Phase 1** stays in `SKILL.md`.

---

### Phase 2: Pattern Analysis

**Find the pattern before fixing:**

1. **Find working examples** — Where does similar code work in the same codebase?
2. **Compare against references** — If implementing a pattern, read the reference COMPLETELY (not just the first example)
3. **Identify differences** — List every difference between working and broken, however small
4. **Understand dependencies** — What config, environment, or state does this require?

---

### Phase 3: Hypothesis and Testing

**Scientific method — one variable at a time:**

1. **Form single hypothesis** — "I think X is the root cause because Y" — write it down
2. **Test minimally** — make the SMALLEST possible change to test the hypothesis
3. **Verify result:**
   - Fixed? → Phase 4
   - Not fixed? → Form NEW hypothesis. Do NOT stack more fixes on top.

When you don't know: say "I don't understand X" — don't pretend to.

---

### Phase 4: Implementation

**Fix the root cause, not the symptom:**

1. **Create failing test** — Write the simplest automated test that reproduces the bug. Must exist before fix.
   - Follow `dev.tdd` for the test-writing process.

2. **Implement single fix** — Address the root cause. ONE change at a time. No "while I'm here" improvements.

3. **Verify fix** — Test passes? No other tests broken? Issue actually resolved?

4. **If fix doesn't work:**
   - Tried 1–2 times → Return to Phase 1.
   - **Tried 3+ times → STOP. Question the architecture.**

**When 3+ fixes fail — Architectural Problem:**

Pattern signals:
- Each fix reveals new coupling or problem in a different place
- Fixes require "massive refactoring" to implement
- Each fix creates new symptoms elsewhere

→ STOP. Discuss with the human before attempting more fixes. This is not a failed hypothesis — this is a wrong architecture.

---

## Red Flags — Return to Phase 1

If you're thinking any of these:
- "Quick fix for now, investigate later"
- "Just try changing X and see if it works"
- "I don't fully understand but this might work"
- "One more fix attempt" (when already tried 2+)
- Proposing solutions before tracing data flow
- "Each fix reveals a new problem in a different place"

**ALL of these mean: STOP. Return to Phase 1.**

---

## Quick Reference

| Phase | Key Activity | Done When |
|-------|-------------|-----------|
| 1. Root Cause | Read errors, reproduce, gather evidence, trace | Understand WHAT and WHY |
| 2. Pattern | Find working examples, identify differences | Know the gap |
| 3. Hypothesis | One theory, minimal test | Confirmed or new theory |
| 4. Implementation | Failing test → fix → verify | Bug resolved, tests pass |

## Supporting Techniques

- **Root-cause tracing** — Trace bugs backward through call stack to original trigger
- **Defense in depth** — Add validation at multiple layers after finding root cause
- **`dev.tdd`** — For writing the failing test case in Phase 4
