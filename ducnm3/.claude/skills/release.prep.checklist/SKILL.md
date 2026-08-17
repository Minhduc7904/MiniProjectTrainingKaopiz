---
name: release.prep.checklist
description: Use when preparing a code merge to the main production branch or cutting a new versioned release.
when_to_use: Activate at the very end of a feature cycle or milestone. Mandatory before updating package version numbers.
license: MIT
---

# Release — Preparation checklist

**Skill id:** `release.prep.checklist`

A release is a high-stakes event. Use a checklist to eliminate "human-error" variance.

## Mandatory Steps

1.  **Fresh Build**: Run `npm run build` (or equivalent) from a clean state.
2.  **Full Test Suite**: All tests (including slow E2E) must be GREEN.
3.  **Changelog Sync**: Summary of user-facing changes must be updated.
4.  **Version Bump**: Apply SemVer (Major.Minor.Patch) logic.

## Iron Law — No "Blind" Releases

You are forbidden from releasing code that hasn't successfully completed a full production build in the last 15 minutes.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "It built 2 hours ago, and I only changed one comment." | Comments can break builds if they contain hidden characters or affect source maps. Build again. |
| "I'll do the changelog after the release." | Changelogs ensure users know what to expect *before* they update. |

## See also

- `phase.deliver.final` — the final handoff step of which release prep is a subset.
- `phase.verify.compliance` — final check for security leaks before shipping.

---
**Summary:** A clean release builds long-term user trust.
