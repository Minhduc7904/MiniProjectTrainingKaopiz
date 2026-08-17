# devkit-review

**Review phase** — review all code changes on the current branch using the **`code-review-excellence`** skill. Produces a structured review report formatted for PR description or PR comment.

**In Cursor:** slash **`/devkit-review`**.

---

## Prerequisite

The **`code-review-excellence`** skill must be available. It is automatically pulled from GitHub during `devkit:sync` (or `kaopiz-devkit sync`). Confirm by checking that the skill exists under `.claude/skills/code-review-excellence/` or `.claude/skills/code-review-excellence/`.

If missing, run: `npm run kaopiz-devkit -- sync`

---

## Agent — required steps

### Phase 1: Context Gathering (2-3 min)

```bash
# Detect base branch (default: master)
BASE_BRANCH="${1:-master}"

# Understand scope
git log "$BASE_BRANCH"..HEAD --oneline
git diff "$BASE_BRANCH"...HEAD --stat
```

1. Read PR description / linked issue if available.
2. Check diff size — if >400 lines, suggest splitting.
3. Verify CI/CD status (tests passing?).
4. Understand the business requirement.

### Phase 2: High-Level Review (5-10 min)

Read the full diff:

```bash
git diff "$BASE_BRANCH"...HEAD
```

Check:
1. **Architecture & Design** — Does the solution fit the problem? Check SOLID, coupling/cohesion. For significant changes, consult `reference/architecture-review-guide.md` from the skill.
2. **Performance** — Algorithm complexity, N+1 queries, memory usage. For performance-critical code, consult `reference/performance-review-guide.md`.
3. **File Organization** — Are new files in the right places?
4. **Testing Strategy** — Are there tests covering edge cases?

### Phase 3: Line-by-Line Analysis (10-20 min)

For each changed file, apply the **`code-review-excellence`** skill checklist:

- **Logic & Correctness** — Edge cases, off-by-one, null checks, race conditions
- **Security** — Input validation, injection risks, XSS, sensitive data. Consult `reference/security-review-guide.md` for thorough checks.
- **Performance** — N+1 queries, unnecessary loops, memory leaks
- **Maintainability** — Clear names, single responsibility, comments

**Language-specific guides** — load from the skill's `reference/` folder based on the tech stack:

| Stack | Guide |
|-------|-------|
| React | `reference/react.md` |
| Vue 3 | `reference/vue.md` |
| TypeScript | `reference/typescript.md` |
| Java | `reference/java.md` |
| Python | `reference/python.md` |
| Go | `reference/go.md` |
| Rust | `reference/rust.md` |
| C/C++ | `reference/c.md`, `reference/cpp.md` |
| CSS/Less/Sass | `reference/css-less-sass.md` |

### Phase 4: Summary & Write REVIEW.md (2-3 min)

Write the review report to **`REVIEW.md`** at the project root using the **PR review template** from the skill (`assets/pr-review-template.md`):

```markdown
## Summary

[Brief overview of what was reviewed - 1-2 sentences]

**PR Size:** [Small/Medium/Large] (~X lines)
**Review Time:** [X minutes]

## Strengths

- [What was done well]
- [Good patterns or approaches used]

## Required Changes

🔴 **[blocking]** [Issue description]
> [Code location or example]
> [Suggested fix or explanation]

## Important Suggestions

🟡 **[important]** [Issue description]
> [Why this matters]
> [Suggested approach]

## Minor Suggestions

🟢 **[nit]** [Minor improvement suggestion]

💡 **[suggestion]** [Alternative approach to consider]

## Questions

❓ [Clarification needed about X]

## Security Considerations

- [ ] No hardcoded secrets
- [ ] Input validation present
- [ ] Authorization checks in place
- [ ] No SQL/XSS injection risks

## Test Coverage

- [ ] Unit tests added/updated
- [ ] Edge cases covered
- [ ] Error cases tested

## Verdict

**[ ] ✅ Approve** - Ready to merge
**[ ] 💬 Comment** - Minor suggestions, can merge
**[ ] 🔄 Request Changes** - Must address blocking issues
```

### 5. Output for PR

After writing `REVIEW.md`, print the path and suggest next steps:

- If **Approve** or **Comment**: suggest creating PR with review included.
- If **Request Changes**: list the blocking/important issues that need fixing first.

To include review output in a PR:

```bash
# Create PR with review in description
gh pr create --title "<title>" --body "$(cat REVIEW.md)"

# Or add as PR comment on existing PR
gh pr comment <pr-number> --body "$(cat REVIEW.md)"
```

---

## Severity labels

| Label | Meaning | Action |
|-------|---------|--------|
| 🔴 `[blocking]` | Must fix before merge | Block merge |
| 🟡 `[important]` | Should fix, discuss if disagree | Usually block |
| 🟢 `[nit]` | Nice to have, not blocking | Non-blocking |
| 💡 `[suggestion]` | Alternative approach to consider | Non-blocking |
| 📚 `[learning]` | Educational comment, no action needed | Non-blocking |
| 🎉 `[praise]` | Good work, keep it up | Non-blocking |

---

## See also

- Skill **`code-review-excellence`** — full 4-phase review process with language-specific guides
- Skill **`code-review`** — lighter-weight review checklist (Hub built-in)
- Skill **`phase.review.code-request`** — fresh-context review dispatch
- **`/kaopiz-devkit`** — main DevKit orchestrator command

This file syncs to **`.claude/commands/`** and **`.claude/commands/`** (Claude Code).
