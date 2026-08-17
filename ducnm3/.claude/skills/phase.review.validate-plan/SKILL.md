---
name: phase.review.validate-plan
description: "Run MCP validate_plan on PLAN.md, present findings, walk AC traceability — keywords: validate plan MCP, plan lint gate, check PLAN before approval."
when_to_use: Activate only on Hub step validate_and_present (phase.review). Do not use this skill to draft PLAN.md from scratch; use phase.plan.write-plan for drafting.
license: MIT
---

# Review — Validate & present PLAN

**Skill id:** `phase.review.validate-plan`

This step is **review / gate**, not **authoring**. You are validating an existing `PLAN.md` and preparing the user to approve or request changes.

## Iron Law — This is not “write plan”

| Do here | Do **not** do here |
| :--- | :--- |
| Read `PLAN.md`, call **`validate_plan`**, summarize MCP output | Replace the whole plan because you “had a better idea” without user direction |
| Walk **requirements → plan sections / checkboxes** (traceability) | Start coding or “spike” implementation |
| Surface gaps; propose **minimal edits** to `PLAN.md` if MCP flags issues | Treat this as a second pass of **drafting** — that belongs in **`phase.plan.write-plan`** |

## Steps

1. **Load artifact**: Read **`PLAN.md`** from **`.vibe/sessions/<task-id>/PLAN.md`** when present (canonical for parallel tasks); otherwise legacy **`PLAN.md`** at repo root — same resolution as workflow `expected_artifacts` / `kaopiz-devkit verify`.
2. **MCP**: Call **`validate_plan`** with the plan content (and project/task context if your MCP schema requires it).
3. **Summarize**: Short, structured summary — ok / warnings / errors; what to fix before approval.
4. **Present**: Walk the user through **scope**, **Won’t do / Later**, and **traceability** (AC or requirement → plan section or checkbox).
5. **Handoff**: Next step is **`phase.review.plan-approval`** on **`approval_and_outcome`** — wait for explicit approval before Execute. Criteria (đủ chi tiết, khả thi, đúng phạm vi, MCP) are in that skill.

## Expected relationship to other skills

- **`phase.plan.write-plan`**: Produces and refines **`PLAN.md`** during the **Plan** phase. After ingest, planning may include **`phase.plan.risk-assess`** when the workflow uses the detailed plan path.
- **`phase.review.plan-approval`**: Human gate on PLAN — approval, cancel/blocked, or “revise plan and repeat”.

## Planning depth (Hub / flatten) — not a failure of this skill

Some projects want **one** plan micro-step **without** a separate **risk-assess** row. That is controlled by **workflow resolution**, not by this skill:

- **`flattenWorkflow`** in `kaopiz-devkit` defaults to **`enableDetailedPlanning: true`**, which expands composite steps whose **`variants[0]`** points to a **detailed** sub-workflow (e.g. `phase.plan.detailed.post-ingest` includes **map_domains** / **risk-assess**).
- To flatten **without** that extra step: pass **`enableDetailedPlanning: false`**, or change the Hub **`sub_workflow_id`** for the Plan phase to the atomic manifest you want (e.g. `phase.plan.post-ingest` only — confirm with your team’s fork).

This skill still applies unchanged: it only runs **after** `PLAN.md` exists, regardless of how many plan micro-steps preceded it.

## See also

- `phase.plan.write-plan` — drafting `PLAN.md`
- `phase.review.plan-approval` — approval and outcome after validation
- `phase.review.code-request` — independent **code** review (not this gate)
- `phase.plan.risk-assess` — domain/risk mapping when the workflow includes that step

---
**Summary:** Validate, present, then stop at the human gate — never confuse this step with “write the plan.”
