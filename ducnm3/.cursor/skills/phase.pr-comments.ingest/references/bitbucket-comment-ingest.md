# Bitbucket Comment Ingest

## MCP call

When available, call:

```text
bitbucket_fetch_pr_comments(pr_url="<Bitbucket PR URL>", open_only=true)
```

Expected successful response shape:

```json
{
  "success": true,
  "data": {
    "project_key": "PROJECT",
    "repo_slug": "repo",
    "pr_id": "123",
    "counts": { "open": 2, "resolved": 0 },
    "comments": [],
    "markdown": "# PR Comments ..."
  }
}
```

## Source status

If the tool returns `success: false`, copy the concise error into `PR-COMMENTS.md` under `## Source status`, then ask for manual pasted/exported comments.

## Manual fallback

Ask for one of these:

- PR URL plus pasted visible comments.
- Exported Bitbucket activities/comments.
- AI ReviewCode comment report copied from the PR.

Normalize each comment to:

```md
### C-001
- Bitbucket id: <unknown>
- Author: <unknown>
- File: <general>
- Line: <none>
- State: OPEN
- Text:
  <comment text>
```

Use `<unknown>`, `<general>`, and `<none>` only inside this artifact when the source truly lacks the value.

## Stable ids

Assign ids in display order:

- `C-001`
- `C-002`
- `C-003`

Keep these ids stable in `COMMENT-TRIAGE.md`, `COMMENT-FIX-PLAN.md`, and `COMMENT-FIX-SUMMARY.md`.
