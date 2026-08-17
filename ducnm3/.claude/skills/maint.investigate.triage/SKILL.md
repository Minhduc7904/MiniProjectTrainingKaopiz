---
name: maint.investigate.triage
description: Use when analyzing incoming bug reports, performance issues, or system-wide technical debt before planning a fix.
when_to_use: Activate at the very beginning of a maintenance or support task. Mandatory for triaging multi-module regressions.
license: MIT
---

# Maintenance — Investigate & triage

**Skill id:** `maint.investigate.triage`

Act like a diagnostic engineer: perform a wide scan before narrowing down.

## Iron Law — No Premature Fixes

You are forbidden from proposing a fix until you have triaged the severity and mapped the affected areas.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I see the bug, it's right there in line 10." | Line 10 might be the symptom, but the triage must find the cause (maybe line 5). |
| "Triaging is just for big teams." | Triage is for anyone who values their time and wants to avoid regressions. |

## See also

- `dev.debug.systematic` — the detailed protocol after triage is complete.
- `phase.ingest.read-sources` — ingest the results of your triage into a new task.

---
**Summary:** Wide scan first, narrow down later. Traceability is key.
