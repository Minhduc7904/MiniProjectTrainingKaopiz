# Triage And Plan Templates

## COMMENT-TRIAGE.md

```md
# Comment Triage

## Summary
- Total reviewed:
- Fix:
- Ask:
- Skip:

## Groups

### By file or area
| File/area | Comments | Notes |
|---|---|---|

### By issue type
| Issue type | Comments | Notes |
|---|---|---|

### By risk
| Risk | Comments | Notes |
|---|---|---|

## Comment Decisions
| Comment | Decision | Issue type | Risk | Reason | Related comments |
|---|---|---|---|---|---|
```

## COMMENT-FIX-PLAN.md

```md
# Comment Fix Plan

## Fixes
| Comment | Planned change | Files/areas | Verify |
|---|---|---|---|

## Questions
| Comment | Question | Needed from |
|---|---|---|

## Skipped
| Comment | Reason |
|---|---|

## Approval Required
- Required: no
- Reason: All planned fixes are localized, clear, and low risk.
```

## PLAN.md

`PLAN.md` must carry the same executable plan as `COMMENT-FIX-PLAN.md` because shared implementation, review, and verification skills read `PLAN.md` as the canonical plan artifact.

```md
# Comment Fix Plan

Source plan: `COMMENT-FIX-PLAN.md`

## Fixes
| Comment | Planned change | Files/areas | Verify |
|---|---|---|---|

## Questions
| Comment | Question | Needed from |
|---|---|---|

## Skipped
| Comment | Reason |
|---|---|

## Approval Required
- Required: no
- Reason: All planned fixes are localized, clear, and low risk.
```

If approval is required, set:

```md
## Approval Required
- Required: yes
- Reason: <specific high-risk or ambiguous condition>
```
