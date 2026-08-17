# Comment Fix Summary Template

Use this shape for `.vibe/sessions/<task_segment>/COMMENT-FIX-SUMMARY.md`:

Use the stable comment ids from `PR-COMMENTS.md`, `COMMENT-TRIAGE.md`, `COMMENT-FIX-PLAN.md`, and `PLAN.md`.

```md
# Comment Fix Summary

## Counts
- Reviewed:
- Fixed:
- Need discussion:
- Skipped:

## Resolution Map
| Comment | Status | What changed | Verify |
|---|---|---|---|

## Need Discussion
| Comment | Question | Suggested reply |
|---|---|---|

## Skipped
| Comment | Reason |
|---|---|

## Verify
- Commands:
- Result:

## Blockers
- None

## PR Reply Draft
<copyable reply text>
```

Statuses in `Resolution Map`:

- `fixed`
- `needs-discussion`
- `skipped`
- `blocked`
