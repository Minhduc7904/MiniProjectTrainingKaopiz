# Comment Taxonomy

## Issue types

Use one primary type per comment:

| Type | Use when |
|---|---|
| `logic/correctness` | Behavior is wrong or incomplete. |
| `missing-test` | Reviewer asks for coverage or behavior lacks tests. |
| `naming/readability` | Names, structure, or clarity are the issue. |
| `edge-case` | Boundary input, empty state, race, null, or error path. |
| `security/data/authz` | Auth, permission, privacy, secret, PII, or data safety concern. |
| `performance` | Time, memory, query count, bundle size, or scalability concern. |
| `maintainability` | Duplication, layering, complexity, coupling, or long-term cost. |
| `docs/api-contract` | Docs, public API, schema, request/response, or OpenAPI contract. |
| `unknown/needs-discussion` | The comment cannot be acted on safely without clarification. |

## Risk levels

| Risk | Meaning | Planning rule |
|---|---|---|
| `low` | Localized change, no public behavior or data risk. | Can proceed after plan. |
| `medium` | Touches shared helper, cross-file behavior, or non-trivial tests. | Mention affected areas and run targeted checks. |
| `high` | Security/data/API/schema/config/migration/shared runtime, broad refactor, or behavior with significant regression risk. | Requires approval before implementation. |

## Decision rules

- Use `fix` only when the requested change is clear and in scope.
- Use `ask` when the comment conflicts with another comment, lacks enough detail, or requests product judgment.
- Use `skip` only when the comment is obsolete, already addressed, out of scope, or intentionally rejected with a reason.
