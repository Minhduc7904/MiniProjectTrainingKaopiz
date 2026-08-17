---
name: phase.pr-comments.triage-plan
description: Use after PR comments are ingested to read all comments, group related issues, classify each comment as fix/ask/skip, and write COMMENT-TRIAGE.md plus COMMENT-FIX-PLAN.md/PLAN.md before code changes.
when_to_use: Step 2 of qa.fix-comments. Requires approval before implementation when risk, scope, or ambiguity is high.
license: MIT
---

# PR comments - Triage and plan

**Skill id:** `phase.pr-comments.triage-plan`

Read every comment before touching code. The purpose is to avoid fixing comments one by one while missing a shared root cause.

## Inputs

- `.vibe/sessions/<task_segment>/PR-COMMENTS.md`
- Current diff and local code context as needed for planning.

## Required artifacts

Write:

- `.vibe/sessions/<task_segment>/COMMENT-TRIAGE.md`
- `.vibe/sessions/<task_segment>/COMMENT-FIX-PLAN.md`
- `.vibe/sessions/<task_segment>/PLAN.md`

`COMMENT-FIX-PLAN.md` is the PR-comment-specific plan. `PLAN.md` is the canonical execution-plan alias for shared implementation, review, and verification skills. Keep the two files equivalent for fix scope, approval state, and verification commands.

## Required decisions

For each comment id from `PR-COMMENTS.md`, choose exactly one decision:

- `fix`: implement a scoped code/test/doc change.
- `ask`: needs reviewer/developer clarification before implementation.
- `skip`: intentionally not changing code; reason must be explicit.

## Grouping

Group comments by:

- File or area.
- Issue type from [`references/comment-taxonomy.md`](references/comment-taxonomy.md).
- Risk level: low, medium, high.
- Related comments that likely share one root cause.

## Approval gate

Set `## Approval Required` / `- Required: yes` in both plan files when any condition is true:

- Security, privacy, authorization, payment, or data-loss risk.
- Public/external behavior change, shared runtime behavior change, migration, config, schema, or API change.
- Behavior change with data, security, or significant regression risk.
- Refactor larger than the comment scope.
- Contradictory or unclear comments.
- Many related comments point to a larger design issue.

Routine clear low-risk correctness fixes do not require approval solely because behavior changes.

If approval is required, present the plan and wait for approval before implementation.

## Boundaries

- Do not edit product code in this step.
- Do not silently skip comments.
- Do not turn unclear comments into guessed fixes.
- Do not include unrelated cleanup in the fix plan.

## References

- [`references/comment-taxonomy.md`](references/comment-taxonomy.md)
- [`references/triage-template.md`](references/triage-template.md)

---
**Summary:** Every comment gets a decision, reason, and verification path before code changes.
