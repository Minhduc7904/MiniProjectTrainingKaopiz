---
name: dev.debug.systematic
description: Use when encountering a bug, test failure, or unexpected behavior to find the root cause before attempting a fix.
when_to_use: Activate at the start of any debugging session, especially when a 'quick fix' seems obvious. Mandatory for all non-trivial investigations.
license: MIT
---

# Systematic Debugging

Random fixes waste time and create new bugs. Quick patches mask underlying issues.

**Core principle:** ALWAYS find root cause before attempting fixes. Symptom fixes are failure.

> **IRON LAW: NO FIXES WITHOUT ROOT CAUSE INVESTIGATION FIRST.**
>
> Haven't completed Phase 1? You cannot propose fixes.

## The Four Phases

Complete each phase before moving to the next. **Phases 2–4** (pattern analysis, hypothesis, implementation details) are in [`references/phases-detail.md`](references/phases-detail.md).

---

### Phase 1: Root Cause Investigation

**BEFORE attempting ANY fix:**

**1. Read error messages carefully**
- Don't skip past errors or warnings — they often contain the exact solution
- Read stack traces completely — note line numbers, file paths, error codes

**2. Reproduce consistently**
- Can you trigger it reliably?
- What are the exact steps?
- If not reproducible → gather more data, do not guess

**3. Check recent changes**
- What changed that could cause this?
- `git diff`, recent commits, new dependencies, config changes

**4. Gather evidence in multi-component systems**

When the system has multiple components (API → service → database, CI → build → deploy):

```
For EACH component boundary, BEFORE proposing fixes:
  - Log what data enters the component
  - Log what data exits the component
  - Verify environment/config at each layer

Run once to see WHERE it breaks
THEN analyze evidence
THEN investigate that specific component
```

Example diagnostic pattern:
```bash
echo "=== Input at boundary A: ===" && echo "$VAR"
echo "=== State at boundary B: ===" && cat config.json
echo "=== Output at boundary C: ===" && curl -s http://localhost/health
```

**5. Trace data flow**

For errors deep in the call stack:
- Where does the bad value originate?
- What called this with the bad value?
- Keep tracing UP until you find the source
- Fix at the SOURCE, not the symptom

---

## Next steps

- Continue with **Phase 2–4**, red-flag list, and quick-reference table: [`references/phases-detail.md`](references/phases-detail.md).
- **`dev.tdd`** — failing test before fix in Phase 4.
