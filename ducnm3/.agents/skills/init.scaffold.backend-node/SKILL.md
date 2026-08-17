---
name: init.scaffold.backend-node
stacks: ["node"]
description: Use when bootstrapping a Node.js backend to establish a consistent directory structure and core service layers.
when_to_use: Activate at the start of a Node-based project. Mandatory for Express, Fastify, or NestJS services.
license: MIT
---

# Init — Node.js backend scaffold

**Skill id:** `init.scaffold.backend-node`

A consistent Node structure makes it easy for any team member to jump into any service.

## Iron Law — Clean Layering

Controllers (Routes) must be thin. Logic belongs in Services. Data access belongs in Repositories. Never mix DB queries with HTTP handling.

## Layout choice (layer-first)

This skill prescribes **layer-first** folders (`routes` / `services` / `repositories`). That is one valid layout for **new** Node services.

### Alignment with Execute-time full-stack guidance (`dev.stack.fullstack`)

`dev.stack.fullstack` often illustrates **feature-first** folders (domain slices under `src/`). That is **not** a contradiction in *layering rules* (thin controllers, logic in services) — only in **where files live**.

**How teams should decide (implement this in CONTRIBUTING or `PLAN.md`):**

1. **Brownfield** — Repo already has `src/routes/`, `src/services/`, … → **keep** layer-first for all routine tasks. Use fullstack skill for **auth, errors, logging, API client**, not for ripping up the tree.
2. **Greenfield** — Pick **one** layout before the first feature: either stay with this scaffold (layer-first) **or** document a feature-first tree; do not mix both without a **dedicated migration** task.
3. **Agent rule** — In a single product slice, **do not** rename/move the whole backend to “match” feature-first. A layout migration is its **own** PLAN with tests and rollback.

See [PLAN-EXECUTE-SKILL-REFACTOR.md](../../../../../../docs/technical/PLAN-EXECUTE-SKILL-REFACTOR.md) §7.3.

### Default paths

- `src/routes/`: HTTP entrypoints and validation.
- `src/services/`: Core business logic (pure JS/TS).
- `src/repositories/`: Database abstraction.
- `src/contracts/`: Shared interfaces/types.
- `tests/`: Separate integration and unit test folders.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'll put the logic in the router, it's faster for this simple API." | Simple APIs don't stay simple. Moving logic to services later is 10x the work. |
| "Node doesn't need a formal structure like Java." | The lack of a formal structure in Node is exactly why you MUST enforce one. |

## See also

- `init.api.rest` — the primary interface for Node backends.
- `setup.project-init` — setting up the basic `package.json` and lints.
- `dev.stack.fullstack` — ongoing integration patterns; **respect existing folder layout** when the repo already follows this scaffold.

---
**Summary:** Layered architecture is the key to Node.js maintainability.
