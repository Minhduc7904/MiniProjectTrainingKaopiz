---
name: phase.pr-comments.ingest
description: Use at the start of a post-PR comment resolution workflow to identify the Bitbucket PR, fetch open review comments when available, fall back to manual comments, and write PR-COMMENTS.md.
when_to_use: Step 1 of qa.fix-comments. Do not edit product code in this phase.
license: MIT
---

# PR comments - Ingest

**Skill id:** `phase.pr-comments.ingest`

Collect the complete comment set before any code changes. Do not triage, plan, or implement in this step.

## Required artifact

Write **`.vibe/sessions/<task_segment>/PR-COMMENTS.md`**.

Resolve `<task_segment>` from `.vibe/active.json` and `.vibe/sessions/<segment>/state.json` when present. If no session state exists, use the active task identifier provided by the user and normalize it for filenames by replacing characters outside `[a-zA-Z0-9._-]` with `_`.

## Source order

1. Use an explicit PR URL from the user or current task context.
2. If not provided, inspect local branch/session notes for PR metadata.
3. If a PR URL is available and MCP exposes `bitbucket_fetch_pr_comments`, call it with `open_only=true`.
4. If Bitbucket MCP is unavailable, missing credentials, or returns an auth/config error, record the failure briefly and ask the developer to paste/export comments.
5. If no PR URL can be found, ask the developer for a PR URL or pasted/exported comment list.

## Artifact minimum

`PR-COMMENTS.md` must include:

- PR URL or manual source.
- Source: `bitbucket` or `manual`.
- Fetch time when known.
- Open comment count.
- One stable local id per comment: `C-001`, `C-002`, ...
- Bitbucket comment id when known.
- Author, file, line, state, and text when available.
- Replies when available.

## Zero open comments

If Bitbucket returns zero open comments, write `PR-COMMENTS.md` with the zero count and stop the workflow with a short user-facing summary. Do not enter implementation.

## Boundaries

- Do not edit application/runtime source.
- Do not answer or resolve comments on Bitbucket.
- Do not drop comments because they lack file or line anchors.
- Do not infer a fix for unclear comments in this phase.

## Reference

See [`references/bitbucket-comment-ingest.md`](references/bitbucket-comment-ingest.md) for the MCP call shape, manual fallback, and Markdown rendering.

---
**Summary:** Get every open PR comment into `PR-COMMENTS.md`; no code changes yet.
