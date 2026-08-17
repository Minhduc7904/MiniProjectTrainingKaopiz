---
name: phase.pr-comments.summary
description: Use at the end of the fix PR comments workflow to summarize reviewed/fixed/discussed/skipped comments, verification results, and a PR reply draft.
when_to_use: Final step of qa.fix-comments after implementation and verification.
license: MIT
---

# PR comments - Resolution summary

**Skill id:** `phase.pr-comments.summary`

Create the final evidence artifact for the developer before they push or update the PR.

## Inputs

- `.vibe/sessions/<task_segment>/PR-COMMENTS.md`
- `.vibe/sessions/<task_segment>/COMMENT-TRIAGE.md`
- `.vibe/sessions/<task_segment>/COMMENT-FIX-PLAN.md`
- `.vibe/sessions/<task_segment>/PLAN.md`
- Current git diff.
- `.vibe/sessions/<task_segment>/VERIFY-TESTS.md`
- `.vibe/sessions/<task_segment>/VERIFY-EVIDENCE.json` when present.

## Required artifact

Write **`.vibe/sessions/<task_segment>/COMMENT-FIX-SUMMARY.md`**.

## Required content

Include:

- Total comments reviewed.
- Number fixed.
- Number needing discussion.
- Number skipped.
- Per-comment resolution map.
- Verification commands and result.
- Blockers or verification waivers when present.
- Copyable PR reply draft.

## Boundaries

- Do not claim a comment is fixed unless code/docs/tests changed or the summary explains why existing behavior already satisfies it.
- Do not hide failed verification.
- Do not post to the PR platform or mark comments resolved in MVP.
- Do not include secrets, tokens, or long raw logs.

## References

- [`references/summary-template.md`](references/summary-template.md)
- [`references/reply-draft-template.md`](references/reply-draft-template.md)

---
**Summary:** Produce the final comment-resolution evidence and copyable PR response.
