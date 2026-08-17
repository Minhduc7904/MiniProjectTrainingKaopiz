---
name: phase.review.code-request
description: Independent code-review pass with fresh context — after a meaningful slice, before merge, or when stuck — without dumping full session history on the reviewer.
when_to_use: After substantive implementation; before merge or PR. Not for PLAN approval — use phase.review.plan-approval for the Review-phase gate on PLAN.md.
license: MIT
---

# Review — Code request (fresh reviewer)

**Skill id:** `phase.review.code-request`

Dispatch a fresh code reviewer with precisely crafted context — never your session's history. For **human approval of `PLAN.md`** before Execute, use **`phase.review.plan-approval`** instead.

**Core principle:** Review early, review often.

## When to request review

**Mandatory:**
- After each task slice in `phase.execute.subagent-two-layer-review` (Layer 2 IS the quality review)
- After completing a major feature
- Before merge to main / PR creation

**Optional but valuable:**
- When stuck (fresh perspective)
- Before a major refactor (establish baseline)
- After fixing a complex bug

## How to request (step by step)

### Step 1: Get git SHAs

```bash
BASE_SHA=$(git rev-parse HEAD~1)   # or: git rev-parse origin/main
HEAD_SHA=$(git rev-parse HEAD)
echo "Range: $BASE_SHA..$HEAD_SHA"
```

### Step 2: Dispatch a fresh reviewer subagent

Use the `quality-reviewer.md` template from `phase.execute.subagent-two-layer-review/prompts/`. Fill in all placeholders:

| Placeholder | What to fill |
|---|---|
| `{TASK_NUMBER}` | Task number from the plan file (canonical: `.vibe/sessions/<task>/PLAN.md`, or legacy root `PLAN.md`) |
| `{TASK_TITLE}` | Short task description |
| `{BASE_SHA}..{HEAD_SHA}` | Commit range from step 1 |
| `{STACK_AND_CONVENTIONS}` | Language, framework, test runner |
| `{IMPLEMENTER_SUMMARY}` | Copy the DONE status report from implementer |

### Step 3: Act on feedback

| Severity | Action |
|---|---|
| **Critical** | Fix immediately before anything else |
| **Important** | Fix before proceeding to next task |
| **Minor** | Note for later — does not block |

If the reviewer is wrong: push back with technical reasoning and show the test or code that proves your point.

**Example flow (full walkthrough):** [`references/example-flow.md`](references/example-flow.md).

## Integration with two-layer review

In `phase.execute.subagent-two-layer-review`:
- **Layer 1** (spec compliance) runs FIRST — verify ACs are met
- **Layer 2** (code quality) = this skill's review — runs only after Layer 1 ✅
- Never swap the order

## Red flags

**Never:**
- Skip review because "it's simple"
- Ignore Critical issues
- Proceed to the next task with unresolved Important issues
- Accept "looks fine" as review output — require specific findings or explicit APPROVED

**If reviewer is wrong:**
- Push back with technical reasoning
- Show the test or code that proves it works
- Request clarification rather than silently accepting wrong feedback

## See also

- `phase.review.plan-approval` — approve or revise **PLAN.md** before implementation.
- `phase.review.validate-plan` — validate & present `PLAN.md` (MCP) before plan approval.
- `phase.execute.subagent-two-layer-review` — Layer 1 / 2 vs external reviewer.
- `phase.verify.compliance` — automated checks after review and before merge.

---
**Summary:** Fresh **code** review — not the PLAN approval gate.
