# TDD — RED / VERIFY / GREEN / REFACTOR detail

Expanded examples, rationalizations, checklist, and "when stuck" — companion to [`../SKILL.md`](../SKILL.md).

## Linked stack skills (Hub — same as parent `SKILL.md` table)

Use during **GREEN** and **REFACTOR** (not instead of RED). Paths from this file (`references/` → parent `common/`):

| Skill id | Open |
|----------|------|
| `dev.stack.flutter` | [`../../dev.stack.flutter/SKILL.md`](../../dev.stack.flutter/SKILL.md) |
| `dev.stack.react-native` | [`../../dev.stack.react-native/SKILL.md`](../../dev.stack.react-native/SKILL.md) |
| `dev.stack.ios` | [`../../dev.stack.ios/SKILL.md`](../../dev.stack.ios/SKILL.md) |
| `dev.stack.android` | [`../../dev.stack.android/SKILL.md`](../../dev.stack.android/SKILL.md) |
| `dev.stack.fullstack` | [`../../dev.stack.fullstack/SKILL.md`](../../dev.stack.fullstack/SKILL.md) |
| `dev.stack.frontend-web` | [`../../dev.stack.frontend-web/SKILL.md`](../../dev.stack.frontend-web/SKILL.md) |

**vs `init.*`:** init skills bootstrap greenfield / API design; stack skills guide day-to-day implementation. Overlap / tension: [PLAN-EXECUTE-SKILL-REFACTOR.md](../../../../../../../docs/technical/PLAN-EXECUTE-SKILL-REFACTOR.md) §7.3.

## RED — Write the Failing Test

Write **one** test showing exactly what should happen. The test should:
- Have a behavior-describing name (`'rejects empty email'` not `'test1'`)
- Test one thing (if name contains "and" — split it)
- Use real code, not mocks of mocks

**Good:**
```typescript
test('retries failed operation 3 times before throwing', async () => {
  let attempts = 0;
  const operation = () => {
    attempts++;
    if (attempts < 3) throw new Error('fail');
    return 'success';
  };
  const result = await retryOperation(operation);
  expect(result).toBe('success');
  expect(attempts).toBe(3);
});
```

**Bad:** Vague name (`'retry works'`), tests mock behavior, not real code.

## VERIFY RED — Watch It Fail (MANDATORY, NEVER SKIP)

```bash
# Run the new test only
npm test path/to/test.test.ts
# or: pytest tests/path/test.py::test_name -v
```

- Test must **fail** (not error out due to typo)
- Failure message must be **expected** (feature missing, not undefined variable)
- Test **passes immediately**? You're testing existing behavior — fix the test.

## GREEN — Write Minimal Code (this *is* the dev work)

Write the **simplest production code** that makes the failing signal turn green. “Minimal” means **only what the spec asserts** — not the smallest file count, but the **smallest behavior surface** that satisfies RED.

### What you are allowed to touch in GREEN

| Kind of work | Examples (not exhaustive) |
|--------------|---------------------------|
| **Domain / pure logic** | Functions, value objects, validation rules — no I/O, or I/O behind interfaces the test injects. |
| **Application / use-case** | Orchestrate domain + ports; map DTOs; one clear entry per behavior under test. |
| **Adapters** | HTTP handler, DB query, queue consumer — **only** what the integration test for this slice needs. |
| **UI** | One component/screen behavior: render, event, state update — match RTL/component test or accessibility assertion. |
| **Mobile** | Widget / composable / SwiftUI view + view model hook — follow stack skill for **where** files live. |
| **Wiring** | Register route, provider, module export — if the test fails without it. |

### How to dev inside GREEN (recommended order)

1. **Load the stack skill** (if this slice is Flutter, RN, iOS, Android, full-stack web, …) — see `SKILL.md` parent: [Hub stack skills table](../SKILL.md). Use it for **paths, patterns, and idioms**, not to skip RED.
2. **Prefer inner layers first** when the failing test is a unit test: make pure logic pass, then lift to callers. When the test is integration/E2E, implement the **shortest path** through real boundaries (real DB test container, real HTTP server) as the test demands.
3. **Implement the narrowest change**: one function, one branch, one screen state — until VERIFY GREEN passes.
4. **Stop** when the test passes. Do not “also fix” nearby style or rename unrelated symbols here — that is **REFACTOR**.

### What GREEN forbids (still “dev”, but wrong phase)

- New behavior not implied by the current RED (including “while I’m here” fixes).
- Extra parameters, feature flags, or abstractions “for later” without a failing test driving them.
- Broad refactors mixed with the fix — do GREEN first, then REFACTOR in a separate pass with tests still green.

## VERIFY GREEN (MANDATORY)

After coding, run the **same** scoped command as in VERIFY RED (often one file or one test); expand to module/package if your policy requires it.

```bash
npm test path/to/test.test.ts
```

- New test passes ✅
- All other tests still pass ✅
- No warnings in output ✅

## REFACTOR — Clean Up (dev work, behavior frozen)

Only after **VERIFY GREEN** succeeds. REFACTOR is **still development**: you edit production code heavily — rename modules, extract classes, move files — but the **observable behavior** must stay identical (same test outcomes).

### Safe moves in REFACTOR

| Move | Purpose |
|------|---------|
| **Rename** | Clarity (`userId` vs `id`), align with stack skill conventions. |
| **Extract** | Pull duplicated logic into a function/class; reduce file size. |
| **Move** | Put code in the layer/package the stack skill expects (e.g. `features/foo/`). |
| **Delete** | Remove dead code, commented blocks, unused imports. |
| **Simplify control flow** | Early returns, guard clauses — without changing outcomes. |

### Forbidden in REFACTOR

- Any change that would require **new or changed assertions** in tests — that is **new behavior** → new RED.
- “Small” behavior tweaks or bugfixes — do a separate TDD cycle.

### Relationship to `phase.execute.tidy-refactor`

The workflow’s **tidy-refactor** step uses the same rule: **green → tidy → still green**. Use it between PLAN chunks; use **REFACTOR** inside the TDD loop after every GREEN when the code is messy but behavior-complete.

### Pairing with Hub stack skills

During REFACTOR, stack skills (`dev.stack.*`, e.g. [`dev.stack.fullstack`](../../dev.stack.fullstack/SKILL.md)) guide **structure and conventions** (feature folders, Riverpod layout, API error shape). Still: **no new behavior** — only alignment and cleanup.

## Common Rationalizations — All Wrong

| Excuse | Reality |
|--------|---------|
| "Too simple to test" | Simple code breaks. Test takes 30 seconds. |
| "I'll write tests after" | Tests written after pass immediately — prove nothing. |
| "Already manually tested" | Ad-hoc ≠ systematic. Can't re-run. |
| "Deleting X hours is wasteful" | Sunk cost. Unverified code is technical debt. |
| "TDD will slow me down" | TDD is faster than debugging. |

## Red Flags — STOP and Start Over

If you catch yourself doing any of these:
- Writing production code before a failing test exists
- Test passes immediately without implementing anything
- "I'll add tests after to verify it works"
- "Keep the existing code as reference while writing tests"
- Rationalizing "just this once"

**All of these mean: Delete the code. Start over.**

## Verification Checklist

Before marking a task complete:

- [ ] Every new function/method has a test
- [ ] Watched each test fail before implementing
- [ ] Each test failed for expected reason (missing feature, not syntax error)
- [ ] Wrote minimal code to pass each test
- [ ] All tests pass (no regressions)
- [ ] Output pristine (no errors, no warnings)
- [ ] Tests use real code (mocks only when unavoidable — no mock-of-mock)
- [ ] Edge cases and error paths covered

Can't check every box? You skipped TDD. Start over.

## When Stuck

| Problem | Solution |
|---------|----------|
| Don't know how to test | Write the wished-for API first. Write assertion first. |
| Test too complicated | Design too complicated. Simplify the interface. |
| Must mock everything | Code too coupled. Use dependency injection. |
| Test setup is huge | Extract helpers. Still complex? Simplify design. |
