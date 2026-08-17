---
name: code-review
description: Reviews code for quality, security, and maintainability following team standards. Use when reviewing pull requests or when the user asks for a code review.
type: general
when_to_use: Use on every non-trivial PR before merge, when the user asks for review, or when validating a completed slice. Skip only for automated formatting-only diffs if policy allows.
license: MIT
---

# Code review

Give **actionable**, **specific** feedback. Prefer findings over praise; keep tone professional.

## Scope (decide first)

- **In scope:** Diff + linked issue/spec; tests that exercise the change; security/data paths touched by the diff.
- **Out of scope:** Rewriting unrelated modules, style debates already covered by linter (unless the rule is wrong).

## Checklist

### Correctness & design

- [ ] Behavior matches spec / AC; edge cases and error paths handled intentionally.
- [ ] No silent failure paths; logging/metrics appropriate for new failure modes.
- [ ] APIs are stable or versioned; breaking changes called out explicitly.

### Security & data

- [ ] AuthZ on every mutating or sensitive read path; no trust of client-only checks.
- [ ] No secrets, tokens, or PII in logs/tests; inputs validated/sanitized at trust boundaries.
- [ ] SQL/NoSQL queries parameterized; file paths not built from unchecked user input.

### Tests & quality

- [ ] New logic covered by automated tests at the right layer (unit vs integration).
- [ ] Tests assert behavior, not implementation details, unless necessary.
- [ ] Flaky patterns avoided (timers, race conditions, shared global state without isolation).

### Readability & maintainability

- [ ] Names and structure match project conventions; dead code removed.
- [ ] Complexity justified; large functions split or documented *why* they stay large.
- [ ] Dependencies minimal; no unnecessary new transitive risk.

### PR hygiene

- [ ] Diff size reasonable for review; unrelated refactors split when possible.
- [ ] Commit messages / PR description explain *what* and *why*.
- [ ] Migration/rollback noted when schema or runtime config changes.

## Severity rubric

| Level | Meaning | Block merge? |
|-------|---------|----------------|
| **Blocker** | Wrong behavior, security issue, data loss risk | Yes |
| **Major** | Missing tests for critical path, clear bug risk | Usually yes |
| **Minor** | Style, naming, non-blocking nits | No |

## Output format (for the reviewer agent)

1. **Summary** (2–4 sentences): intent of change + overall risk.
2. **Blockers / majors** — file:line — issue — suggestion.
3. **Minors** — optional bullet list.
4. **Tests run** — commands + result if known.

## See also

- `phase.review.code-request` — fresh-context review dispatch.
- **No separate `auth-checklist` (or other `*-checklist`) skills** in the default Hub: use the **Security & data** section above for every review, and call **`fetch_knowhow(domain)`** (e.g. `auth`, `payment`) when the diff touches that domain so Hub/project constraints apply.
- Hub layout for long review playbooks: split detail into `references/` per `hub-skill-layout` (skill-authoring guide).
