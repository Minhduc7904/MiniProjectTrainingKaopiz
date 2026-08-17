---
name: phase.ingest.lock-scope
description: Use when research exists and you need to explicitly define inclusion/exclusion boundaries and assumptions before planning — only after the spec snapshot is concrete enough to list real scope.
when_to_use: After `phase.ingest.read-sources` (and any brainstorm/clarify) when `.vibe/research/<task-id>.md` has a usable Spec snapshot. Skip only if that file already lists clear in/out scope and concrete assumptions.
license: MIT
---

# Ingest — Lock scope & assumptions

**Skill id:** `phase.ingest.lock-scope`

Prevents scope creep by **concrete** in/out lists. This is **L3** — not a substitute for L1/L2 spec work.

## Prerequisite (do not fake)

You may run this skill **only if** one of the following holds:

1. **Spec snapshot** in **`.vibe/research/<task-id>.md`** is concrete enough to name files/behaviors to change or add; **or**
2. You **cannot** lock scope yet — then do **not** invent vague bullets. Instead, under **Ambiguities**, write **BLOCKED: cannot lock scope until …** and list what is missing (user answer, MCP, design decision).

**If** the spec is still fuzzy: return to **`phase.ingest.read-sources`** or **`phase.spec.brainstorm`** / clarify — **not** to `PLAN.md`.

## Implementation steps

1. **Define In-Scope**  
   - List **specific** files, modules, or user-visible behaviors to be added or changed.  
   - Name test files to add/update if known.

2. **Define Out-of-Scope (Won’t Do)**  
   - List items that look related but are **explicitly excluded** this task.

3. **Record Assumptions**  
   - State any behavior you assume (e.g. “API X returns JSON array”) so the user can confirm.

## Expected output

- **Update the same canonical file:** **`.vibe/research/<task-id>.md`** — refresh the **In scope / Out of scope** section (and tie **Confirmed vs Assumptions** to match).  
- Confirm scope via the editor's **ask-question tool** (`AskUserQuestion` on opencode/Claude Code · `AskQuestion` on Cursor), NOT plain chat text: summarize “Will do A, B, C; will not do X, Y; assumptions: …” as context and ask to confirm before Plan. Fall back to a typed reply only if the tool is unavailable.

## Iron Law — No placeholders

Every line must be concrete. Forbidden: “refactor if needed”, “review later”, “see above” without file paths.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| “It’s obvious what I’ll skip.” | Unlisted exclusions become scope creep. |
| “I’ll lock scope after the first PR.” | Lock scope **before** Plan approval, per workflow. |

## See also

- `phase.ingest.read-sources` — canonical research file and L0–L2.  
- `phase.plan.write-plan` — **after** scope is agreed.

---
**Summary:** Lock scope only on a real spec snapshot; otherwise block and clarify.
