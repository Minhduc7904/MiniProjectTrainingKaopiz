---
name: init.api.trpc
stacks: ["node"]
description: Use when building a full-stack TypeScript application to get end-to-end type safety without a formal schema.
when_to_use: Activate for internal monorepo projects where the client and server are both TypeScript.
license: MIT
---

# Init — tRPC API

**Skill id:** `init.api.trpc`

tRPC provides the best DX for TypeScript teams: zero-codegen, 100% type safety.

## Iron Law — No Manual Type Casting

If you find yourself using `as` or `any` in your tRPC procedures or client calls, you've defeated the purpose of the tool.

## Key Patterns

- **Inferred Types**: Let TypeScript infer the types from your procedures. No manual interface sharing needed.
- **Validation**: Use **Zod** (or similar) to validate every input at the procedure boundary.
- **Context**: Use the `ctx` object for authentication and shared dependencies (DB clients).

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "tRPC is only for small projects." | tRPC scales beautifully for multi-package monorepos with hundreds of procedures. |
| "I'll just pass the whole DB object to the client." | tRPC types are for DX, but you still shouldn't leak sensitive data. Use `.select()` or `.omit()`. |

## See also

- `init.api.rest` — required for non-TypeScript clients or external webhooks.
- `init.scaffold.backend-node` — tRPC usually runs within a Node environment.

---
**Summary:** tRPC is the "secret weapon" for high-velocity TypeScript teams.
